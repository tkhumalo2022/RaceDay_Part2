using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace RaCeDay.Services;

// Stores uploaded profile pictures under a generated name, never a client-supplied filename.
public sealed class ProfileImageStorage(IWebHostEnvironment environment)
{
    private const long MaxBytes = 2 * 1024 * 1024;
    private static readonly Regex StoredName = new(
        @"^[0-9a-f]{32}\.(jpg|png)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private const string PublicFolder = "/profile-images/";

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxBytes)
            throw new InvalidDataException("Choose a picture smaller than 2 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png"))
            throw new InvalidDataException("Only JPG and PNG pictures are supported.");

        // Read the first eight bytes; ContentType and the original name cannot be trusted.
        await using var source = file.OpenReadStream();
        var header = new byte[8];
        var bytesRead = await source.ReadAsync(header, cancellationToken);
        var jpeg = bytesRead >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff;
        var png = bytesRead == 8 && header.AsSpan().SequenceEqual(
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        if (extension == ".png" && !png || extension is ".jpg" or ".jpeg" && !jpeg)
            throw new InvalidDataException("That file is not a valid JPG or PNG picture.");

        var storedExtension = png ? ".png" : ".jpg";
        var name = Guid.NewGuid().ToString("N") + storedExtension;
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "profile-images");
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, name);

        // Open a new file so existing files cannot be overwritten.
        await using var destination = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write,
            FileShare.None, 81920, FileOptions.Asynchronous);
        await destination.WriteAsync(header.AsMemory(0, bytesRead), cancellationToken);
        await source.CopyToAsync(destination, cancellationToken);
        return PublicFolder + name;
    }

    public void Delete(string? storedPath)
    {
        if (string.IsNullOrEmpty(storedPath) || !storedPath.StartsWith(PublicFolder, StringComparison.Ordinal))
            return;
        var name = storedPath[PublicFolder.Length..];
        if (!StoredName.IsMatch(name)) return;
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRoot, "profile-images", name);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}

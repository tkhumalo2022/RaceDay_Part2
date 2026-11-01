using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaCeDay.Services;

// Azure is the sole persistent image store for Part 3. Never accept user-defined blob paths.
public sealed class ImageStorageUnavailableException : Exception
{
    public ImageStorageUnavailableException(Exception? cause = null) : base("Azure Blob Storage is not configured or reachable.", cause) { }
}
public sealed class InvalidRaceImageException(string message) : Exception(message);
public sealed class AzureRaceImageStore(IConfiguration configuration)
{
    private const long MaxImageSize = 3L * 1024 * 1024;
    private BlobContainerClient Container()
    {
        var connection = configuration["AzureBlob:ConnectionString"];
        var name = configuration["AzureBlob:Container"] ?? "raceday-images";
        if (string.IsNullOrWhiteSpace(connection))
            throw new ImageStorageUnavailableException();
        return new BlobContainerClient(connection, name);
    }

    public static string EventKey(int id) => id > 0
        ? $"events/{id}/banner" : throw new ArgumentOutOfRangeException(nameof(id));
    public static string ProfileKey(string userId)
    {
        if (!Guid.TryParse(userId, out var guid))
            throw new ArgumentException("Invalid authenticated user ID.", nameof(userId));
        return $"profiles/{guid:D}/picture";
    }

    // Validate content length and signatures; do not trust file names or declared MIME types.
    public static async Task<(byte[] Bytes, string Type)> ValidateAsync(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length is < 1 or > MaxImageSize)
            throw new InvalidRaceImageException("Choose a JPG or PNG image smaller than 3 MB.");
        using var memory = new MemoryStream();
        await file.CopyToAsync(memory, ct);
        var bytes = memory.ToArray();
        if (bytes.LongLength != file.Length || bytes.Length < 8)
            throw new InvalidRaceImageException("The uploaded image is incomplete.");
        var png = bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var jpeg = bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff;
        if (!png && !jpeg)
            throw new InvalidRaceImageException("Only actual JPG or PNG image files are permitted.");
        return (bytes, png ? "image/png" : "image/jpeg");
    }

    public async Task SaveAsync(string key, IFormFile file, CancellationToken ct)
    {
        var (bytes, contentType) = await ValidateAsync(file, ct);
        var container = Container();
        try
        {
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);
            await using var input = new MemoryStream(bytes, writable: false);
            await container.GetBlobClient(key).UploadAsync(input,
                new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } }, ct);
        }
        catch (RequestFailedException e)
        {
            throw new ImageStorageUnavailableException(e);
        }
    }

    public async Task<(byte[] Bytes, string Type)?> ReadAsync(string key, CancellationToken ct)
    {
        var container = Container();
        try
        {
            var blob = container.GetBlobClient(key);
            // Stream and bound reads. DownloadContentAsync would buffer an arbitrarily
            // large blob before the MVC/API size checks could reject it.
            var download = await blob.DownloadStreamingAsync(cancellationToken: ct);
            await using var source = download.Value.Content;
            using var output = new MemoryStream();
            var buffer = new byte[64 * 1024];
            int count;
            while ((count = await source.ReadAsync(buffer.AsMemory(), ct)) > 0)
            {
                if (output.Length + count > MaxImageSize)
                    throw new ImageStorageUnavailableException();
                await output.WriteAsync(buffer.AsMemory(0, count), ct);
            }
            var bytes = output.ToArray();
            var png = bytes.Length >= 8 && bytes.AsSpan(0, 8)
                .SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            var jpeg = bytes.Length >= 3 && bytes[0] == 0xff &&
                bytes[1] == 0xd8 && bytes[2] == 0xff;
            if (!png && !jpeg)
                throw new ImageStorageUnavailableException();
            return (bytes, png ? "image/png" : "image/jpeg");
        }
        catch (RequestFailedException e) when (e.Status == 404) { return null; }
        catch (RequestFailedException) { throw new ImageStorageUnavailableException(); }
    }
}

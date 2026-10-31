using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RaCeDay.Services;

namespace RaCeDay.Tests;

public sealed class AzureImageValidationTests
{
    private static IFormFile File(byte[] data, string name = "file.png")
    {
        var stream = new MemoryStream(data);
        return new FormFile(stream, 0, data.Length, "file", name);
    }

    [Fact]
    public async Task RealPngSignatureAccepted()
    {
        var bytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 5, 6 };
        var (payload, mime) = await AzureRaceImageStore.ValidateAsync(File(bytes), CancellationToken.None);
        Assert.Equal("image/png", mime);
        Assert.Equal(bytes, payload);
    }

    [Fact]
    public async Task RealJpegSignatureAccepted()
    {
        var bytes = new byte[] { 0xff, 0xd8, 0xff, 0xe0, 0, 0, 0, 0, 5 };
        var (_, mime) = await AzureRaceImageStore.ValidateAsync(File(bytes, "photo.jpg"), CancellationToken.None);
        Assert.Equal("image/jpeg", mime);
    }

    [Fact]
    public async Task RenamedExecutableRejected()
    {
        var bytes = new byte[] { 77, 90, 0, 0, 0, 0, 0, 0, 0, 0 };
        await Assert.ThrowsAsync<InvalidRaceImageException>(() =>
            AzureRaceImageStore.ValidateAsync(File(bytes), CancellationToken.None));
    }

    [Fact]
    public async Task OversizedImageRejected()
    {
        var bytes = new byte[3 * 1024 * 1024 + 1];
        await Assert.ThrowsAsync<InvalidRaceImageException>(() =>
            AzureRaceImageStore.ValidateAsync(File(bytes), CancellationToken.None));
    }

    [Fact]
    public void BlobKeysCannotBeSuppliedByClient()
    {
        Assert.Equal("events/7/banner", AzureRaceImageStore.EventKey(7));
        Assert.Throws<ArgumentOutOfRangeException>(() => AzureRaceImageStore.EventKey(0));
        Assert.Throws<ArgumentException>(() => AzureRaceImageStore.ProfileKey("../../secret"));
        var id = Guid.NewGuid();
        Assert.Equal($"profiles/{id:D}/picture", AzureRaceImageStore.ProfileKey(id.ToString()));
    }

    [Fact]
    public async Task AzureNotConfiguredReturnsTypedError()
    {
        var store = new AzureRaceImageStore(new ConfigurationBuilder().Build());
        var png = File(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 0 });
        await Assert.ThrowsAsync<ImageStorageUnavailableException>(() =>
            store.SaveAsync(AzureRaceImageStore.EventKey(5), png, CancellationToken.None));
    }
}
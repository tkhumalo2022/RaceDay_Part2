using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RaCeDay.Services;

namespace RaCeDay.Tests;

// Run locally against Azurite: RACEDAY_TEST_BLOB_CONNECTION=UseDevelopmentStorage=true.
// CI without emulator exercises validation tests; no fake cloud-success assertion.
public sealed class AzureBlobRoundTripTests
{
    [Fact]
    public async Task UploadAndReadBlob_ThroughActualAzureSdk()
    {
        var connection = Environment.GetEnvironmentVariable("RACEDAY_TEST_BLOB_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection)) return;
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["AzureBlob:ConnectionString"] = connection,
                ["AzureBlob:Container"] = "raceday-qa-" + Guid.NewGuid().ToString("N")[..16]
            }).Build();
        var storage = new AzureRaceImageStore(config);
        var data = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 42, 7 };
        var file = new FormFile(new MemoryStream(data), 0, data.Length, "file", "race.png");
        const string key = "events/19/banner";
        await storage.SaveAsync(key, file, CancellationToken.None);
        var actual = await storage.ReadAsync(key, CancellationToken.None);
        Assert.NotNull(actual);
        Assert.Equal("image/png", actual.Value.Type);
        Assert.Equal(data, actual.Value.Bytes);
        Assert.Null(await storage.ReadAsync("events/19/missing", CancellationToken.None));
    }
}
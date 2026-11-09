using RaCeDay.Portal.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options => options.Filters.Add<ApiUnavailableFilter>());
builder.Services.AddHttpClient<RaceDayApiClient>((provider, client) =>
{
    var value = provider.GetRequiredService<IConfiguration>()["RaCeDayApi:BaseUrl"];
    if (!Uri.TryCreate(value, UriKind.Absolute, out var address) ||
        address.Scheme is not ("http" or "https"))
        throw new InvalidOperationException("Set a valid RaCeDayApi:BaseUrl.");
    client.BaseAddress = address;
    // The peer-review API may wake from the free cloud tier after an idle period.
    client.Timeout = provider.GetRequiredService<IHostEnvironment>().IsStaging()
        ? TimeSpan.FromSeconds(90) : TimeSpan.FromSeconds(6);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = false, AllowAutoRedirect = false
});
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.Run();
public partial class Program { }

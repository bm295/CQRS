using ACCORD;
using ACCORD.Api;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAccordBackend();
builder.Services.AddRateLimiter(options => options.AddPolicy("auth", context =>
    RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })));
var app = builder.Build();
await ACCORD.DatabaseMigrator.ApplyAsync(app.Configuration, app.Logger, app.Lifetime.ApplicationStopping);
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; style-src 'self'; script-src 'self'; frame-ancestors 'none'; base-uri 'self'";
    try { await next(context); }
    catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
    {
        app.Logger.LogWarning("Supabase request failed: {Type}", exception.GetType().Name);
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 502;
            await context.Response.WriteAsJsonAsync(new { message = "Không thể kết nối Supabase. Vui lòng thử lại." });
        }
    }
});
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-cache"
});
app.UseRateLimiter();
app.MapAccordApi();
app.Run();

public partial class Program { }


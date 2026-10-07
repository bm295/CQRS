using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ACCORD.Infrastructure.Supabase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACCORD.Tests;

public sealed class ApiContractTests
{
    private TestApplication application = null!;
    private HttpClient client = null!;

    [SetUp]
    public void SetUp()
    {
        application = new TestApplication();
        client = application.CreateClient();
    }

    [TearDown]
    public void TearDown() { client.Dispose(); application.Dispose(); }

    [TestCase("GET")]
    [TestCase("POST")]
    [TestCase("PATCH")]
    [TestCase("DELETE")]
    public async Task TasksWithoutBearerNeverReachSupabase(string method)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method),
            method is "PATCH" or "DELETE" ? "/api/tasks/93f9137b-27c0-4a08-9fbb-9bb433605ee7" : "/api/tasks");
        if (method == "POST") request.Content = JsonContent.Create(new { title = "Valid title" });
        if (method == "PATCH") request.Content = JsonContent.Create(new { isComplete = true });
        using var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(application.Upstream.Calls, Is.Empty);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task InvalidCreateDoesNotWrite(string? title)
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", "user-token");
        using var response = await client.PostAsJsonAsync("/api/tasks", new { title });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(application.Upstream.Calls, Is.Empty);
    }

    [Test]
    public async Task TitleLengthLimitIsAppliedBeforeWrite()
    {
        using var response = await client.PostAsJsonAsync("/api/tasks", new { title = new string('x', 201) });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(application.Upstream.Calls, Is.Empty);
    }

    [Test]
    public async Task QueryRetainsReadShapeOrderingAndUserIdentity()
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", "user-token");
        application.Upstream.ResponseBody = "[{\"id\":\"fixture\",\"title\":\"Task\",\"is_complete\":false,\"created_at\":\"2026-10-07T00:00:00Z\"}]";
        using var response = await client.GetAsync("/api/tasks");
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo(application.Upstream.ResponseBody));
        var call = application.Upstream.Calls.Single();
        Assert.Multiple(() =>
        {
            Assert.That(call.Method, Is.EqualTo("GET"));
            Assert.That(call.Path, Is.EqualTo("/rest/v1/tasks?select=id,title,is_complete,created_at&order=created_at.desc"));
            Assert.That(call.Authorization, Is.EqualTo("Bearer user-token"));
            Assert.That(call.ApiKey, Is.EqualTo("test-publishable"));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
        });
    }

    [Test]
    public async Task CreateTrimsTitleAndPreservesCreatedResponse()
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", "user-token");
        application.Upstream.Status = HttpStatusCode.Created;
        using var response = await client.PostAsJsonAsync("/api/tasks", new { title = "  New task  " });
        var call = application.Upstream.Calls.Single();
        using var body = JsonDocument.Parse(call.Body!);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(call.Method, Is.EqualTo("POST"));
            Assert.That(body.RootElement.GetProperty("title").GetString(), Is.EqualTo("New task"));
            Assert.That(call.Authorization, Is.EqualTo("Bearer user-token"));
            Assert.That(call.Prefer, Is.EqualTo("return=representation"));
        });
    }

    [Test]
    public async Task CompletionAndDeleteTargetOnlyRequestedId()
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", "user-token");
        const string id = "93f9137b-27c0-4a08-9fbb-9bb433605ee7";
        using var patch = await client.PatchAsJsonAsync("/api/tasks/" + id, new { isComplete = true });
        using var delete = await client.DeleteAsync("/api/tasks/" + id);
        var calls = application.Upstream.Calls;
        using var body = JsonDocument.Parse(calls[0].Body!);
        Assert.Multiple(() =>
        {
            Assert.That(calls.Select(c => c.Method), Is.EqualTo(new[] { "PATCH", "DELETE" }));
            Assert.That(calls.All(c => c.Path == "/rest/v1/tasks?id=eq." + id), Is.True);
            Assert.That(calls.All(c => c.Authorization == "Bearer user-token"), Is.True);
            Assert.That(body.RootElement.GetProperty("is_complete").GetBoolean(), Is.True);
            Assert.That(patch.IsSuccessStatusCode && delete.IsSuccessStatusCode, Is.True);
        });
    }

    [Test]
    public async Task GuestDisabledHasActionableMessage()
    {
        application.Upstream.Status = HttpStatusCode.UnprocessableEntity;
        application.Upstream.ResponseBody = "{\"error_code\":\"anonymous_provider_disabled\"}";
        using var response = await client.PostAsync("/api/session/guest", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Allow anonymous sign-ins"));
        Assert.That(application.Upstream.Calls.Single().Authorization, Is.Null);
    }

    [Test]
    public async Task InvalidRefreshDoesNotCallSupabase()
    {
        using var response = await client.PostAsJsonAsync("/api/session/refresh", new { refreshToken = "" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(application.Upstream.Calls, Is.Empty);
    }

    [Test]
    public async Task RefreshMapsTokenWithoutAddingBearer()
    {
        using var response = await client.PostAsJsonAsync("/api/session/refresh", new { refreshToken = "refresh-token" });
        var call = application.Upstream.Calls.Single();
        using var body = JsonDocument.Parse(call.Body!);
        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(call.Path, Is.EqualTo("/auth/v1/token?grant_type=refresh_token"));
            Assert.That(body.RootElement.GetProperty("refresh_token").GetString(), Is.EqualTo("refresh-token"));
            Assert.That(call.Authorization, Is.Null);
        });
    }

    [Test]
    public async Task NonJsonUpstreamFailureIsSanitized()
    {
        application.Upstream.Status = HttpStatusCode.InternalServerError;
        application.Upstream.ResponseBody = "private diagnostic";
        using var response = await client.PostAsync("/api/session/guest", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Not.Contain("private diagnostic"));
    }

    private sealed class TestApplication : WebApplicationFactory<Program>
    {
        public FakeSupabase Upstream { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:AutoMigrate"] = "false",
                ["Supabase:Url"] = "https://test.supabase.co",
                ["Supabase:PublishableKey"] = "test-publishable"
            }));
            builder.ConfigureServices(services => services.AddHttpClient<SupabaseClient>()
                .ConfigurePrimaryHttpMessageHandler(() => Upstream));
        }
    }

    private sealed record UpstreamCall(string Method, string Path, string? Body, string? Authorization, string ApiKey, string? Prefer);
    private sealed class FakeSupabase : HttpMessageHandler
    {
        public List<UpstreamCall> Calls { get; } = [];
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public string ResponseBody { get; set; } = "[]";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls.Add(new(request.Method.Method, request.RequestUri!.PathAndQuery,
                request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken),
                request.Headers.Authorization?.ToString(), request.Headers.GetValues("apikey").Single(),
                request.Headers.TryGetValues("Prefer", out var prefer) ? prefer.Single() : null));
            return new HttpResponseMessage(Status) { Content = new StringContent(ResponseBody) };
        }
    }
}

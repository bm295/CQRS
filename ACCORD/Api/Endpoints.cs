using System.Net.Http.Headers;
using ACCORD.Application.Common;
using ACCORD.Application.Sessions;
using ACCORD.Application.Tasks.Commands;
using ACCORD.Application.Tasks.Queries;
using ACCORD.Infrastructure.Supabase;

namespace ACCORD.Api;

public static class Endpoints
{
    public static void MapAccordApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");
        api.AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            return await next(context);
        });
        api.MapGet("/health", (IConfiguration configuration) => Results.Ok(new
        {
            application = "ACCORD", configured = SupabaseClient.IsConfigured(configuration)
        }));
        api.MapPost("/session/guest", async (ICommandHandler<CreateGuestSessionCommand> handler, CancellationToken ct) =>
            ToHttp(await handler.HandleAsync(new(), ct))).RequireRateLimiting("auth");
        api.MapPost("/session/refresh", async (RefreshSessionRequest body, ICommandHandler<RefreshGuestSessionCommand> handler, CancellationToken ct) =>
            ToHttp(await handler.HandleAsync(new(body.RefreshToken), ct))).RequireRateLimiting("auth");
        api.MapGet("/tasks", async (HttpContext context, IQueryHandler<ListTasksQuery> handler, CancellationToken ct) =>
            ToHttp(await handler.HandleAsync(new(AccessToken(context)), ct)));
        api.MapPost("/tasks", async (CreateTaskRequest body, HttpContext context, ICommandHandler<CreateTaskCommand> handler, CancellationToken ct) =>
            ToHttp(await handler.HandleAsync(new(body.Title, AccessToken(context)), ct)));
        api.MapPatch("/tasks/{id:guid}", async (Guid id, SetTaskCompletionRequest body, HttpContext context, ICommandHandler<SetTaskCompletionCommand> handler, CancellationToken ct) =>
            ToHttp(await handler.HandleAsync(new(id, body.IsComplete, AccessToken(context)), ct)));
        api.MapDelete("/tasks/{id:guid}", async (Guid id, HttpContext context, ICommandHandler<DeleteTaskCommand> handler, CancellationToken ct) =>
            ToHttp(await handler.HandleAsync(new(id, AccessToken(context)), ct)));
    }

    private static string? AccessToken(HttpContext context) =>
        AuthenticationHeaderValue.TryParse(context.Request.Headers.Authorization, out var header) &&
        header.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) ? header.Parameter : null;

    private static IResult ToHttp(OperationResult result) => result.Json is null
        ? Results.StatusCode(result.StatusCode)
        : Results.Content(result.Json, "application/json", statusCode: result.StatusCode);
}

public sealed record CreateTaskRequest(string? Title);
public sealed record SetTaskCompletionRequest(bool IsComplete);
public sealed record RefreshSessionRequest(string? RefreshToken);

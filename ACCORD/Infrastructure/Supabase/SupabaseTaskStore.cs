using ACCORD.Application.Common;
using ACCORD.Application.Tasks;

namespace ACCORD.Infrastructure.Supabase;

public sealed class SupabaseTaskReader(SupabaseClient client) : ITaskReader
{
    public Task<OperationResult> ListAsync(string? accessToken, CancellationToken cancellationToken) =>
        client.SendAsync(HttpMethod.Get, "rest/v1/tasks?select=id,title,is_complete,created_at&order=created_at.desc", null, accessToken, true, cancellationToken);
}

public sealed class SupabaseTaskWriter(SupabaseClient client) : ITaskWriter
{
    public Task<OperationResult> CreateAsync(string title, string? accessToken, CancellationToken cancellationToken) =>
        client.SendAsync(HttpMethod.Post, "rest/v1/tasks", new { title }, accessToken, true, cancellationToken);

    public Task<OperationResult> SetCompletionAsync(Guid id, bool isComplete, string? accessToken, CancellationToken cancellationToken) =>
        client.SendAsync(HttpMethod.Patch, $"rest/v1/tasks?id=eq.{id}", new { is_complete = isComplete }, accessToken, true, cancellationToken);

    public Task<OperationResult> DeleteAsync(Guid id, string? accessToken, CancellationToken cancellationToken) =>
        client.SendAsync(HttpMethod.Delete, $"rest/v1/tasks?id=eq.{id}", null, accessToken, true, cancellationToken);
}

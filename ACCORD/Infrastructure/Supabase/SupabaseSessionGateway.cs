using ACCORD.Application.Common;
using ACCORD.Application.Sessions;

namespace ACCORD.Infrastructure.Supabase;

public sealed class SupabaseSessionGateway(SupabaseClient client) : ISessionGateway
{
    public Task<OperationResult> CreateGuestAsync(CancellationToken cancellationToken) =>
        client.SendAsync(HttpMethod.Post, "auth/v1/signup", new { data = new { } }, null, false, cancellationToken);

    public Task<OperationResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
        client.SendAsync(HttpMethod.Post, "auth/v1/token?grant_type=refresh_token", new { refresh_token = refreshToken }, null, false, cancellationToken);
}

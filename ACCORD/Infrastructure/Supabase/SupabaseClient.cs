using System.Net.Http.Headers;
using System.Text.Json;
using ACCORD.Application.Common;

namespace ACCORD.Infrastructure.Supabase;

public sealed class SupabaseClient(HttpClient client, IConfiguration configuration)
{
    public static bool IsConfigured(IConfiguration config) =>
        Uri.TryCreate(config["Supabase:Url"], UriKind.Absolute, out var url) && url.Scheme == "https" &&
        !string.IsNullOrWhiteSpace(config["Supabase:PublishableKey"]);

    public async Task<OperationResult> SendAsync(HttpMethod method, string path, object? body,
        string? accessToken, bool authenticated, CancellationToken cancellationToken)
    {
        if (!IsConfigured(configuration))
            return OperationResult.Error(503, "Chưa cấu hình Supabase. Xem README của ACCORD.");
        if (authenticated && string.IsNullOrWhiteSpace(accessToken)) return new(401);

        using var request = new HttpRequestMessage(method, configuration["Supabase:Url"]!.TrimEnd('/') + "/" + path);
        request.Headers.Add("apikey", configuration["Supabase:PublishableKey"]);
        if (authenticated)
        {
            // Supabase validates the JWT and enforces RLS for both read and write operations.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("Prefer", "return=representation");
        }
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var status = (int)response.StatusCode;
        if (response.IsSuccessStatusCode) return new(string.IsNullOrEmpty(content) ? 204 : status, string.IsNullOrEmpty(content) ? null : content);

        if (path == "auth/v1/signup" && IsAnonymousDisabled(content))
            return OperationResult.Error(503, "Chưa bật phiên khách trên Supabase. Bật Allow anonymous sign-ins trong Authentication → Sign In / Providers rồi tải lại trang.");
        return OperationResult.Error(status >= 500 ? 502 : status, status switch
        {
            400 or 422 => "Thông tin không hợp lệ. Kiểm tra email, mật khẩu hoặc xác nhận email.",
            401 or 403 => "Phiên đăng nhập không hợp lệ hoặc bạn không có quyền truy cập.",
            429 => "Quá nhiều yêu cầu. Vui lòng thử lại sau.",
            _ => "Supabase chưa xử lý được yêu cầu. Kiểm tra cấu hình và schema."
        });
    }

    private static bool IsAnonymousDisabled(string content)
    {
        try
        {
            using var error = JsonDocument.Parse(content);
            return error.RootElement.ValueKind == JsonValueKind.Object &&
                error.RootElement.TryGetProperty("error_code", out var code) &&
                code.ValueKind == JsonValueKind.String && code.GetString() == "anonymous_provider_disabled";
        }
        catch (JsonException) { return false; }
    }
}

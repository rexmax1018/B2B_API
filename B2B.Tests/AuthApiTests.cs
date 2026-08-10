using System.Net;
using System.Net.Http.Json;
using B2B.WebApi.Model.Auth;
using B2B.WebApi.Model.Common;

namespace B2B.Tests;

/// <summary>
/// 驗證 Auth API 的 HTTP 回應行為。
/// </summary>
public sealed class AuthApiTests(B2BWebApiFactory factory) : IClassFixture<B2BWebApiFactory>
{
    /// <summary>
    /// 驗證 Health API 可匿名存取。
    /// </summary>
    [Fact]
    public async Task Health_ReturnsOkApiResponse()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.Equal("OK", payload.Data);
        Assert.False(string.IsNullOrWhiteSpace(payload.TraceId));
    }

    /// <summary>
    /// 驗證 liveness health check 可匿名存取。
    /// </summary>
    [Fact]
    public async Task LiveHealthCheck_ReturnsHealthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// 驗證相符的加密 Entry 憑證會回傳權杖。
    /// </summary>
    [Fact]
    public async Task Login_WithMatchingEncryptedCredential_ReturnsToken()
    {
        var client = factory.CreateClient();
        var request = new LoginRequest
        {
            EncryptedCredential = ReadEntryCredential()
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data?.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.TraceId));
    }

    /// <summary>
    /// 驗證不相符的加密 Entry 憑證會被拒絕。
    /// </summary>
    [Fact]
    public async Task Login_WithUnrecognizedEncryptedCredential_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var request = new LoginRequest
        {
            EncryptedCredential = "AES-GCM-V1:AAAAAAAAAAAAAAAAA4jazmC2o5LzKMK5cbL+eKtuR9Qs7BO99TpnshJXvd0="
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("INVALID_ENTRY_CREDENTIAL", payload.Error?.Code);
        Assert.False(string.IsNullOrWhiteSpace(payload.TraceId));
    }

    /// <summary>
    /// 驗證 Auth API 會依用戶端分區套用速率限制。
    /// </summary>
    [Fact]
    public async Task Login_WhenRateLimitExceeded_ReturnsTooManyRequestsApiResponse()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("B2B.Tests.RateLimit");
        var request = new LoginRequest
        {
            EncryptedCredential = ReadEntryCredential()
        };

        HttpResponseMessage? response = null;

        for (var i = 0; i < 6; i++)
        {
            response?.Dispose();
            response = await client.PostAsJsonAsync("/api/auth/login", request);
        }

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("RATE_LIMITED", payload.Error?.Code);
        Assert.False(string.IsNullOrWhiteSpace(payload.TraceId));
    }

    /// <summary>
    /// 讀取測試 Host 複製到輸出目錄的 Entry 憑證。
    /// </summary>
    /// <returns>AES 加密的 Entry 憑證。</returns>
    private static string ReadEntryCredential()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Entry.ini");
        return File.ReadAllText(path).Trim();
    }
}

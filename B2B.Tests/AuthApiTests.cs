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
    /// 驗證錯誤帳密會回傳未授權的標準 API 回應。
    /// </summary>
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedApiResponse()
    {
        var client = factory.CreateClient();
        var request = new LoginRequest
        {
            Account = "missing-user",
            Password = "wrong-password"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("AUTH_FAILED", payload.Error?.Code);
    }

    /// <summary>
    /// 驗證模型驗證錯誤會回傳標準 API 回應。
    /// </summary>
    [Fact]
    public async Task Login_WithInvalidModel_ReturnsValidationApiResponse()
    {
        var client = factory.CreateClient();
        var request = new LoginRequest
        {
            Account = "",
            Password = "short"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("VALIDATION_FAILED", payload.Error?.Code);
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
            Account = "missing-user",
            Password = "wrong-password"
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
    }
}

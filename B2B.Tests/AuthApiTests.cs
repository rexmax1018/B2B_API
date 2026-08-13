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

    [Fact]
    public async Task LiveHealthCheck_ReturnsHealthy()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WhenCredentialValidationIsNotMigrated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Credential = "credential"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("AUTHENTICATION_NOT_CONFIGURED", payload.Error?.Code);
    }

    [Fact]
    public async Task Login_WhenRateLimitExceeded_ReturnsTooManyRequestsApiResponse()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("B2B.Tests.RateLimit");
        var request = new LoginRequest { Credential = "credential" };

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

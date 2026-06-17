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
}

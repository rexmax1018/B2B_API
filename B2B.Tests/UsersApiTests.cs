using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using B2B.WebApi.Model.Auth;
using B2B.WebApi.Model.Common;
using B2B.WebApi.Model.User;

namespace B2B.Tests;

/// <summary>
/// 驗證已授權服務可透過 Web API 查詢使用者資料。
/// </summary>
public sealed class UsersApiTests(B2BWebApiFactory factory) : IClassFixture<B2BWebApiFactory>
{
    /// <summary>
    /// 驗證 Entry 憑證取得的 Service JWT 可查詢使用者，且不暴露密碼雜湊。
    /// </summary>
    [Fact]
    public async Task GetByAccount_WithServiceJwt_ReturnsSanitizedUser()
    {
        var client = factory.CreateClient();
        var accessToken = await GetServiceAccessTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/users/by-account/admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Data);
        Assert.Equal(1, payload.Data.UserId);
        Assert.Equal("admin", payload.Data.Account);
        Assert.Equal("系統管理員", payload.Data.DisplayName);
        Assert.True(payload.Data.IsActive);
    }

    /// <summary>
    /// 驗證服務 JWT 不可查得不存在的使用者。
    /// </summary>
    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var client = factory.CreateClient();
        var accessToken = await GetServiceAccessTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/users/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("USER_NOT_FOUND", payload.Error?.Code);
    }

    private static async Task<string> GetServiceAccessTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            EncryptedCredential = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Entry.ini")).Trim()
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        return Assert.IsType<string>(payload?.Data?.AccessToken);
    }
}

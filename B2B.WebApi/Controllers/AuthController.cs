using B2B.Service.Interfaces;
using B2B.WebApi.Mappings;
using B2B.WebApi.Model.Auth;
using B2B.WebApi.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace B2B.WebApi.Controllers;

/// <summary>
/// 提供登入、更新權杖與登出 API。
/// </summary>
/// <param name="authService">驗證服務。</param>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[EnableRateLimiting("Auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// 驗證帳號密碼並取得權杖。
    /// </summary>
    /// <param name="request">登入請求。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>登入結果。</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(
            request.Account ?? string.Empty,
            request.Password ?? string.Empty,
            cancellationToken);

        if (!result.Success || result.Token is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail(
                result.Message ?? "登入失敗",
                new ErrorResponse("AUTH_FAILED", result.Message ?? "登入失敗")));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result.Token.ToLoginResponse()));
    }

    /// <summary>
    /// 使用 Refresh Token 換發新的權杖。
    /// </summary>
    /// <param name="request">更新權杖請求。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>更新權杖結果。</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (!result.Success || result.Token is null)
        {
            var message = result.Message ?? "更新權杖失敗";

            return Unauthorized(ApiResponse<RefreshTokenResponse>.Fail(
                message,
                new ErrorResponse(result.ErrorCode ?? "INVALID_REFRESH_TOKEN", message)));
        }

        return Ok(ApiResponse<RefreshTokenResponse>.Ok(result.Token.ToRefreshTokenResponse()));
    }

    /// <summary>
    /// 登出並撤銷 Refresh Token。
    /// </summary>
    /// <param name="request">登出請求。</param>
    /// <param name="cancellationToken">取消權杖。</param>
    /// <returns>登出結果。</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object?>>> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);

        return Ok(ApiResponse<object?>.Ok(null, "登出成功"));
    }
}

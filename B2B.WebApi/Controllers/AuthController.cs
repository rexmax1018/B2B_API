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
    /// 驗證應用程式憑證並取得權杖。
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
        var result = await authService.LoginAsync(request.Credential, cancellationToken);

        if (!result.Success || result.Token is null)
        {
            var message = result.Message ?? "登入失敗";
            return Unauthorized(ApiResponse<LoginResponse>.Fail(
                message,
                new ErrorResponse(result.ErrorCode ?? "AUTH_FAILED", message)));
        }

        // TODO: 若舊版登入回應包含額外欄位，於 AuthResponseMapping 接回。
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
        var result = await authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (!result.Success || result.Token is null)
        {
            var message = result.Message ?? "更新權杖失敗";
            return Unauthorized(ApiResponse<RefreshTokenResponse>.Fail(
                message,
                new ErrorResponse(result.ErrorCode ?? "INVALID_REFRESH_TOKEN", message)));
        }

        // TODO: 若舊版換發回應包含額外欄位，於 AuthResponseMapping 接回。
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
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);

        // TODO: 若舊版登出回應包含額外資訊，於此接回。
        return Ok(ApiResponse<object?>.Ok(null, "登出成功"));
    }
}

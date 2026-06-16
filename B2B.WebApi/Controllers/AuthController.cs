using B2B.Domain;
using B2B.Service.Interfaces;
using B2B.WebApi.Model.Auth;
using B2B.WebApi.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2B.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(
            request.Account,
            request.Password,
            cancellationToken);

        if (!result.Success || result.Token is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail(
                result.Message ?? "登入失敗",
                new ErrorResponse("AUTH_FAILED", result.Message ?? "登入失敗")));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result.Token.ToLoginResponse()));
    }

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
            return Unauthorized(ApiResponse<RefreshTokenResponse>.Fail(
                result.Message ?? "Refresh Token 失敗",
                new ErrorResponse("REFRESH_TOKEN_FAILED", result.Message ?? "Refresh Token 失敗")));
        }

        return Ok(ApiResponse<RefreshTokenResponse>.Ok(result.Token.ToRefreshTokenResponse()));
    }
}

internal static class AuthResponseMapping
{
    public static LoginResponse ToLoginResponse(this TokenDomain token) => new()
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        TokenType = token.TokenType,
        ExpiresIn = token.ExpiresIn
    };

    public static RefreshTokenResponse ToRefreshTokenResponse(this TokenDomain token) => new()
    {
        AccessToken = token.AccessToken,
        RefreshToken = token.RefreshToken,
        TokenType = token.TokenType,
        ExpiresIn = token.ExpiresIn
    };
}
using B2B.Service.Interfaces;
using B2B.WebApi.Mappings;
using B2B.WebApi.Model.Common;
using B2B.WebApi.Model.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2B.WebApi.Controllers;

/// <summary>
/// 提供已驗證服務查詢使用者資料的 API。
/// </summary>
[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// 依使用者識別碼查詢使用者。
    /// </summary>
    [HttpGet("{userId:long}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(long userId, CancellationToken cancellationToken)
    {
        // TODO[MIGRATE-CONTROLLER]: 將舊版 Controller 的輸入檢核／權限規則搬到此處；查詢本身交給 IUserService。
        var user = await userService.GetByIdAsync(userId, cancellationToken);
        return ToResponse(user);
    }

    /// <summary>
    /// 依登入帳號查詢使用者。
    /// </summary>
    [HttpGet("by-account/{account}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetByAccount(string account, CancellationToken cancellationToken)
    {
        // TODO[MIGRATE-CONTROLLER]: 將舊版 Controller 的帳號格式／輸入檢核規則搬到此處；查詢本身交給 IUserService。
        var user = await userService.GetByAccountAsync(account, cancellationToken);
        return ToResponse(user);
    }

    private ActionResult<ApiResponse<UserResponse>> ToResponse(B2B.Domain.UserDomain? user)
    {
        if (user is null)
        {
            const string message = "找不到使用者";
            return NotFound(ApiResponse<UserResponse>.Fail(message, new ErrorResponse("USER_NOT_FOUND", message)));
        }

        // TODO[MIGRATE-RESPONSE]: 將舊版回應欄位搬到 UserResponseMapping；目前刻意不回傳 PasswordHash。
        return Ok(ApiResponse<UserResponse>.Ok(user.ToUserResponse()));
    }
}

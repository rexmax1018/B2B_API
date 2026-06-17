using B2B.WebApi.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2B.WebApi.Controllers;

/// <summary>
/// 提供健康檢查 API。
/// </summary>
[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// 取得服務健康狀態。
    /// </summary>
    /// <returns>健康檢查結果。</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<string>> Get()
    {
        return Ok(ApiResponse<string>.Ok("OK"));
    }
}

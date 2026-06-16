using B2B.WebApi.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2B.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<string>> Get()
    {
        return Ok(ApiResponse<string>.Ok("OK"));
    }
}
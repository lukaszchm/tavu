using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tavu.Exceptions;
using Tavu.Storage;

namespace tavu.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IExcerciseStore store;

    public string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? string.Empty;

    public UserController(IExcerciseStore store)
    {
        this.store = store ?? throw new TavuServiceConfigurationException($"{nameof(IExcerciseStore)} component was not registered.");
    }

    [HttpGet("id")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<string> GetExcerciseRawText()
    {
        if (string.IsNullOrEmpty(UserId))
        {
            return Unauthorized();
        }
        return Ok(UserId);
    }
}
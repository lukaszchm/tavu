using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tavu.Exceptions;
using Tavu.Storage;

namespace tavu.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ExcerciseController : ControllerBase
{
    private readonly IExcerciseStore store;

    public string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? string.Empty;

    public ExcerciseController(IExcerciseStore store)
    {
        this.store = store ?? throw new TavuServiceConfigurationException($"{nameof(IExcerciseStore)} component was not registered.");
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> GetExcerciseRawText()
    {
        var words = await store.GetWords(UserId);
        return Ok(words);
    }
}
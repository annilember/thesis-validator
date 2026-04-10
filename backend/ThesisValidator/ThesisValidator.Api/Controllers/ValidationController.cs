using Microsoft.AspNetCore.Mvc;
using ThesisValidator.BLL.Interfaces;
using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Api.Controllers;

[ApiController]
[Route("api/validate")]
public class ValidationController : ControllerBase
{
    private readonly IValidationEngine _validationEngine;

    public ValidationController(IValidationEngine validationEngine)
    {
        _validationEngine = validationEngine;
    }

    [HttpPost]
    public async Task<IActionResult> Validate(
        IFormFile? file,
        [FromQuery] ESupportedLanguage language = ESupportedLanguage.Et,
        [FromQuery] string templateId = "taltech-it")
    {
        // TODO: maybe validate MIME type.

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "Faili laadimine ebaõnnestus" });
        }

        await using var stream = file.OpenReadStream();
        var result = await _validationEngine.ValidateAsync(stream, file.FileName, language, templateId);

        return Ok(result);
    }
}

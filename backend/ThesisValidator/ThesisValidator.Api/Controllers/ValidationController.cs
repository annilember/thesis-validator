using Microsoft.AspNetCore.Mvc;
using ThesisValidator.BLL;
using ThesisValidator.BLL.Interfaces;
using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Api.Controllers;

[ApiController]
[Route("api/validate")]
public class ValidationController : ControllerBase
{
    private readonly IValidationEngine _validationEngine;

    private static readonly string[] AllowedMimeTypes =
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"];

    public ValidationController(IValidationEngine validationEngine)
    {
        _validationEngine = validationEngine;
    }

    [HttpPost]
    public async Task<IActionResult> Validate(
        IFormFile? file,
        [FromForm] ESupportedLanguage language = ESupportedLanguage.Et,
        [FromForm] string templateId = "taltech-it",
        [FromForm] EThesisType thesisType = EThesisType.Bachelor,
        [FromForm] ESupportedLanguage curriculumLanguage = ESupportedLanguage.Et,
        [FromForm] string? foreignTitle = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "Faili laadimine ebaõnnestus" });
        }

        if (!AllowedMimeTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = "Valitud failiformaat ei ole toetatud" });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _validationEngine.ValidateAsync(
                stream,
                file.FileName,
                language, templateId,
                thesisType,
                curriculumLanguage,
                foreignTitle);

            return Ok(result);
        }
        catch (TemplateNotFoundException exception)
        {
            return NotFound(new { error = exception.Message });
        }
        catch (UnsupportedFormatException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (Exception ex)
        {
            //TODO: change back to - return StatusCode(500, new { error = "Valideerimise käigus tekkis ootamatu viga" });
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.ToString() });
        }
    }
}

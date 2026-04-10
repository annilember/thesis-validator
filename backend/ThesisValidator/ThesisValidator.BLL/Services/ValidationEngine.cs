using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Models;
using ThesisValidator.DAL;
using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Services;

public class ValidationEngine : IValidationEngine
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IEnumerable<IDocumentValidator> _validators;

    public ValidationEngine(IRuleRepository ruleRepository, IEnumerable<IDocumentValidator> validators)
    {
        _ruleRepository = ruleRepository;
        _validators = validators;
    }

    public async Task<ValidationResult> ValidateAsync(
        Stream document,
        string fileName,
        ESupportedLanguage language,
        string templateId)
    {
        var template = await _ruleRepository.GetTemplateAsync(templateId);
        if (template == null)
        {
            return new ValidationResult { FileName = fileName, TemplateId = templateId };
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var validator = _validators.FirstOrDefault(v => v.CanValidate(extension));
        if (validator == null)
        {
            // TODO: make this return more accurate?
            return new ValidationResult { FileName = fileName, TemplateId = templateId };
        }

        var result = await validator.ValidateAsync(document, template, language);
        result.FileName = fileName;
        result.TemplateId = templateId;

        return result;
    }
}

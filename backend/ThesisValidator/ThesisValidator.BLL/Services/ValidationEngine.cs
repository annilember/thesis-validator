using System.Net;
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
        string templateId,
        EThesisType thesisType,
        ESupportedLanguage curriculumLanguage,
        string? foreignTitle)
    {
        var template = await _ruleRepository.GetTemplateAsync(templateId);
        if (template == null)
        {
            throw new TemplateNotFoundException(templateId);
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var validator = _validators.FirstOrDefault(v => v.CanValidate(extension));
        if (validator == null)
        {
            throw new UnsupportedFormatException(extension);
        }

        var applicableRules = template.Rules.Where(r =>
            (r.ThesisType == null || r.ThesisType == thesisType) &&
            (r.Language == null || r.Language == language) &&
            (r.CurriculumLanguage == null || r.CurriculumLanguage == curriculumLanguage));

        var result = await validator.ValidateAsync(document, applicableRules);
        result.FileName = fileName;
        result.TemplateId = templateId;

        return result;
    }
}

using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Models;
using ThesisValidator.DAL;
using ThesisValidator.Domain;
using ThesisValidator.Domain.Enums;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Docx;

public class DocxValidator : IDocumentValidator
{
    private readonly IRuleEvaluator _ruleEvaluator;
    private readonly IDocumentParsingService _docxParsingService;

    public DocxValidator(IRuleEvaluator ruleEvaluator, IDocumentParsingService docxParsingService)
    {
        _ruleEvaluator = ruleEvaluator;
        _docxParsingService = docxParsingService;
    }

    public bool CanValidate(string fileExtension)
    {
        return fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ValidationResult> ValidateAsync(
        Stream document,
        ValidationTemplate template,
        ESupportedLanguage language)
    {
        var issues = new List<ValidationIssue>();

        foreach (var rule in template.Rules)
        {
            var issue = rule.Enabled
                ? await ValidateRuleAsync(document, rule, language)
                : ValidationIssue.CreateSkipped(rule.RuleId, "Reegel pole sisse lülitatud");

            issues.Add(issue);
        }

        return new ValidationResult { Issues = issues };
    }

    private Task<ValidationIssue> ValidateRuleAsync(Stream document, ValidationRule rule, ESupportedLanguage language)
    {
        return rule.Type switch
        {
            ERuleType.Numeric => ValidateNumericRuleAsync(document, (NumericRule)rule),
            _ => Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, "Reegli kontrollmehhanism on puudu"))
        };
    }

    private async Task<ValidationIssue> ValidateNumericRuleAsync(Stream document, NumericRule rule)
    {
        var actualValue = await GetNumericValueAsync(document, rule);

        if (actualValue == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, "Väärtust ei leitud dokumendist");
        }

        return _ruleEvaluator.EvaluateNumeric(rule, actualValue.Value);
    }

    private async Task<double?> GetNumericValueAsync(Stream document, NumericRule rule)
    {
        return (rule.Target, rule.Property) switch
        {
            ("page", "marginLeft") => await _docxParsingService.GetPageMarginLeftAsync(document, rule.Unit),
            _ => null
        };
    }
}

using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
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
    private readonly IDocumentParsingService<WordprocessingDocument> _docxParsingService;
    private readonly ILogger<DocxValidator> _logger;

    public DocxValidator(
        IRuleEvaluator ruleEvaluator,
        IDocumentParsingService<WordprocessingDocument> docxParsingService,
        ILogger<DocxValidator> logger)
    {
        _ruleEvaluator = ruleEvaluator;
        _docxParsingService = docxParsingService;
        _logger = logger;
    }

    public bool CanValidate(string fileExtension)
    {
        return fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ValidationResult> ValidateAsync(
        Stream document,
        IEnumerable<ValidationRule> rules)
    {
        using var wordDocument = WordprocessingDocument.Open(document, false);
        var issues = new List<ValidationIssue>();

        foreach (var rule in rules)
        {
            var issue = rule.Enabled
                ? await ValidateRuleAsync(wordDocument, rule)
                : ValidationIssue.CreateSkipped(rule.RuleId, "Reegel pole sisse lülitatud");

            issues.Add(issue);
        }

        return new ValidationResult { Issues = issues };
    }

    private Task<ValidationIssue> ValidateRuleAsync(
        WordprocessingDocument document,
        ValidationRule rule)
    {
        return rule switch
        {
            NumericRule numeric => ValidateNumericRuleAsync(document, numeric),
            BooleanRule boolean => ValidateBooleanRuleAsync(document, boolean),
            EnumRule enumRule => ValidateEnumRuleAsync(document, enumRule),
            RegexRule regex => ValidateRegexRuleAsync(document, regex),
            CountRule count => ValidateCountRuleAsync(document, count),
            OrderRule order => ValidateOrderRuleAsync(document, order),
            CrossReferenceRule crossRef => ValidateCrossReferenceRuleAsync(document, crossRef),
            _ => Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, "Reegli kontrollmehhanism on puudu"))
        };
    }

    private async Task<ValidationIssue> ValidateNumericRuleAsync(WordprocessingDocument document, NumericRule rule)
    {
        var actualValue = GetNumericValue(document, rule);

        if (actualValue == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, "Väärtust ei leitud dokumendist");
        }

        return _ruleEvaluator.EvaluateNumeric(rule, actualValue.Value);
    }

    private async Task<ValidationIssue> ValidateBooleanRuleAsync(WordprocessingDocument document, BooleanRule rule)
    {
        var actualValue = GetBooleanValue(document, rule);

        if (actualValue == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, "Väärtust ei leitud dokumendist");
        }

        return _ruleEvaluator.EvaluateBoolean(rule, actualValue.Value);
    }

    private Task<ValidationIssue> ValidateEnumRuleAsync(WordprocessingDocument document, EnumRule rule)
    {
        var actualValues = GetEnumValues(document, rule);

        if (actualValues == null)
        {
            return Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, "Väärtust ei leitud dokumendist"));
        }

        return Task.FromResult(_ruleEvaluator.EvaluateEnum(rule, actualValues));
    }

    private Task<ValidationIssue> ValidateRegexRuleAsync(WordprocessingDocument document, RegexRule rule)
    {
        var actualValues = _docxParsingService.GetParagraphTexts(document, rule.StyleFilters);

        if (actualValues.Count == 0)
        {
            return Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, "Ühtegi lõiku ei leitud"));
        }

        foreach (var value in actualValues)
        {
            _logger.LogDebug("Regex check - RuleId: {RuleId}, Text: {Text}, Pattern: {Pattern}",
                rule.RuleId, value, rule.Pattern);
        }

        return Task.FromResult(_ruleEvaluator.EvaluateRegex(rule, actualValues));
    }

    private Task<ValidationIssue> ValidateCountRuleAsync(WordprocessingDocument document, CountRule rule)
    {
        var actualCount = (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Section, ERuleProperty.ParagraphCount) => _docxParsingService.GetMinParagraphCountInSubsection(document),
            _ => (int?)null
        };

        if (actualCount == null)
        {
            return Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, "Väärtust ei leitud dokumendist"));
        }

        return Task.FromResult(_ruleEvaluator.EvaluateCount(rule, actualCount.Value));
    }

    private Task<ValidationIssue> ValidateOrderRuleAsync(WordprocessingDocument document, OrderRule rule)
    {
        var actualOrder = (rule.Target, rule.OrderType) switch
        {
            (ERuleTarget.Document, EOrderType.Fixed) => _docxParsingService.GetSectionTitles(document),
            _ => null
        };

        if (actualOrder == null)
        {
            return Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, "Väärtust ei leitud dokumendist"));
        }

        return Task.FromResult(_ruleEvaluator.EvaluateOrder(rule, actualOrder));
    }

    private Task<ValidationIssue> ValidateCrossReferenceRuleAsync(WordprocessingDocument document,
        CrossReferenceRule rule)
    {
        var result = (rule.Target, rule.ReferenceTarget) switch
        {
            (ERuleTarget.Document, EReferenceTarget.BodyText) => EvaluateGlossaryTermsInText(document, rule),
            _ => ValidationIssue.CreateSkipped(rule.RuleId, "Reegli kontrollmehhanism on puudu")
        };

        return Task.FromResult(result);
    }

    private double? GetNumericValue(WordprocessingDocument document, NumericRule rule)
    {
        return (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Page, ERuleProperty.MarginLeft) => _docxParsingService.GetPageMarginLeft(document, rule.Unit),
            _ => null
        };
    }

    private bool? GetBooleanValue(WordprocessingDocument document, BooleanRule rule)
    {
        return (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Paragraph, ERuleProperty.Bold) => _docxParsingService.GetParagraphBold(document, rule.StyleFilters),
            _ => null
        };
    }

    private List<string>? GetEnumValues(WordprocessingDocument document, EnumRule rule)
    {
        return (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Paragraph, ERuleProperty.Alignment) => _docxParsingService.GetParagraphAlignments(document, rule.StyleFilters),
            _ => null
        };
    }

    private ValidationIssue EvaluateGlossaryTermsInText(WordprocessingDocument document, CrossReferenceRule rule)
    {
        if (string.IsNullOrEmpty(rule.SectionTitle))
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, "Sektsiooni pealkiri puudub");
        }

        var terms = _docxParsingService.GetGlossaryTerms(document, rule.SectionTitle);
        var bodyText = _docxParsingService.GetBodyText(document);

        foreach (var term in terms)
        {
            _logger.LogDebug("Glossary term: {Term}, Found in text: {Found}", term,
                bodyText.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return _ruleEvaluator.EvaluateCrossReference(rule, terms, bodyText);
    }
}

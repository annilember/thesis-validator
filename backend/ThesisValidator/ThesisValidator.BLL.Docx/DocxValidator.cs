using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Models;
using ThesisValidator.BLL.Services;
using ThesisValidator.DAL;
using ThesisValidator.Domain;
using ThesisValidator.Domain.Enums;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Docx;

public class DocxValidator : DocumentValidatorBase<WordprocessingDocument>
{
    private readonly IDocumentParsingService<WordprocessingDocument> _docxParsingService;
    private readonly ILogger<DocxValidator> _logger;

    public DocxValidator(
        IRuleEvaluator ruleEvaluator,
        IDocumentParsingService<WordprocessingDocument> docxParsingService,
        ILogger<DocxValidator> logger) : base(ruleEvaluator)
    {
        _docxParsingService = docxParsingService;
        _logger = logger;
    }

    public override bool CanValidate(string fileExtension)
        => fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public override async Task<ValidationResult> ValidateAsync(Stream document, IEnumerable<ValidationRule> rules)
    {
        using var wordDocument = WordprocessingDocument.Open(document, false);
        return await ValidateRulesAsync(wordDocument, rules);
    }

    protected override async Task<ValidationIssue> ValidateNumericRuleAsync(WordprocessingDocument document,
        NumericRule rule)
    {
        var actualValue = GetNumericValue(document, rule);
        var actualValues = GetNumericValues(document, rule);

        if (actualValue == null && actualValues == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Väärtust ei leitud dokumendist");
        }

        if (actualValues != null)
        {
            if (actualValues.Count == 0)
            {
                return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Ühtegi lõiku ei leitud");
            }

            return RuleEvaluator.EvaluateNumeric(rule, actualValues);
        }

        return RuleEvaluator.EvaluateNumeric(rule, actualValue!.Value);
    }

    protected override async Task<ValidationIssue> ValidateBooleanRuleAsync(WordprocessingDocument document,
        BooleanRule rule)
    {
        var actualValue = GetBooleanValue(document, rule);

        if (actualValue == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Väärtust ei leitud dokumendist");
        }

        return RuleEvaluator.EvaluateBoolean(rule, actualValue.Value);
    }

    protected override Task<ValidationIssue> ValidateEnumRuleAsync(WordprocessingDocument document, EnumRule rule)
    {
        var actualValues = GetEnumValues(document, rule);

        if (actualValues == null)
        {
            return Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Väärtust ei leitud dokumendist"));
        }

        return Task.FromResult(RuleEvaluator.EvaluateEnum(rule, actualValues));
    }

    protected override Task<ValidationIssue> ValidateRegexRuleAsync(WordprocessingDocument document, RegexRule rule)
    {
        var actualValues = _docxParsingService.GetParagraphTexts(document, rule.StyleFilters);

        if (actualValues.Count == 0)
        {
            return Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Ühtegi lõiku ei leitud"));
        }

        foreach (var value in actualValues)
        {
            _logger.LogDebug("Regex check - RuleId: {RuleId}, Text: {Text}, Pattern: {Pattern}",
                rule.RuleId, value, rule.Pattern);
        }

        return Task.FromResult(RuleEvaluator.EvaluateRegex(rule, actualValues));
    }

    protected override Task<ValidationIssue> ValidateCountRuleAsync(WordprocessingDocument document, CountRule rule)
    {
        var actualCount = (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Section, ERuleProperty.ParagraphCount) =>
                _docxParsingService.GetMinParagraphCountInSubsection(document),
            _ => (int?)null
        };

        if (actualCount == null)
        {
            return Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Väärtust ei leitud dokumendist"));
        }

        return Task.FromResult(RuleEvaluator.EvaluateCount(rule, actualCount.Value));
    }

    protected override Task<ValidationIssue> ValidateOrderRuleAsync(WordprocessingDocument document, OrderRule rule)
    {
        var actualOrder = (rule.Target, rule.OrderType) switch
        {
            (ERuleTarget.Document, EOrderType.Fixed) => _docxParsingService.GetSectionTitles(document),
            _ => null
        };

        if (actualOrder == null)
        {
            return Task.FromResult(ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Väärtust ei leitud dokumendist"));
        }

        return Task.FromResult(RuleEvaluator.EvaluateOrder(rule, actualOrder));
    }

    protected override Task<ValidationIssue> ValidateCrossReferenceRuleAsync(WordprocessingDocument document,
        CrossReferenceRule rule)
    {
        var result = (rule.Target, rule.ReferenceTarget) switch
        {
            (ERuleTarget.Document, EReferenceTarget.BodyText) => EvaluateGlossaryTermsInText(document, rule),
            _ => ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Reegli kontrollmehhanism on puudu")
        };

        return Task.FromResult(result);
    }

    private double? GetNumericValue(WordprocessingDocument document, NumericRule rule)
    {
        return (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Page, ERuleProperty.MarginTop) => _docxParsingService.GetPageMarginTop(document, rule.Unit),
            (ERuleTarget.Page, ERuleProperty.MarginBottom) => _docxParsingService.GetPageMarginBottom(document,
                rule.Unit),
            (ERuleTarget.Page, ERuleProperty.MarginLeft) => _docxParsingService.GetPageMarginLeft(document, rule.Unit),
            (ERuleTarget.Page, ERuleProperty.MarginRight) =>
                _docxParsingService.GetPageMarginRight(document, rule.Unit),
            (ERuleTarget.Page, ERuleProperty.MarginFooter) => _docxParsingService.GetPageMarginFooter(document,
                rule.Unit),
            _ => null
        };
    }

    private List<double>? GetNumericValues(WordprocessingDocument document, NumericRule rule)
    {
        return (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Paragraph, ERuleProperty.FontSize) => _docxParsingService.GetParagraphFontSizes(document,
                rule.StyleFilters),
            _ => null
        };
    }

    private bool? GetBooleanValue(WordprocessingDocument document, BooleanRule rule)
    {
        return (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Paragraph, ERuleProperty.Bold) => _docxParsingService.GetParagraphBold(document,
                rule.StyleFilters),
            _ => null
        };
    }

    private List<string>? GetEnumValues(WordprocessingDocument document, EnumRule rule)
    {
        return (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Paragraph, ERuleProperty.Alignment) => _docxParsingService.GetParagraphAlignments(document,
                rule.StyleFilters),
            _ => null
        };
    }

    private ValidationIssue EvaluateGlossaryTermsInText(WordprocessingDocument document, CrossReferenceRule rule)
    {
        if (string.IsNullOrEmpty(rule.SectionTitle))
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message,"Sektsiooni pealkiri puudub");
        }

        var terms = _docxParsingService.GetGlossaryTerms(document, rule.SectionTitle);
        var bodyText = _docxParsingService.GetBodyText(document);

        foreach (var term in terms)
        {
            _logger.LogDebug("Glossary term: {Term}, Found in text: {Found}", term,
                bodyText.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return RuleEvaluator.EvaluateCrossReference(rule, terms, bodyText);
    }
}

using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Models;
using ThesisValidator.BLL.Services;
using ThesisValidator.DAL.Interfaces;
using ThesisValidator.Domain;
using ThesisValidator.Domain.Enums;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Docx;

public class DocxValidator : DocumentValidatorBase<WordprocessingDocument>
{
    private readonly IDocumentParsingService<WordprocessingDocument> _docxParsingService;
    private readonly ILanguageDetectionService _languageDetectionService;
    private readonly IDocumentRenderingService _renderingService;
    private readonly ILogger<DocxValidator> _logger;

    public DocxValidator(
        IRuleEvaluator ruleEvaluator,
        IDocumentParsingService<WordprocessingDocument> docxParsingService,
        ILanguageDetectionService languageDetectionService,
        IDocumentRenderingService renderingService,
        ILogger<DocxValidator> logger) : base(ruleEvaluator)
    {
        _docxParsingService = docxParsingService;
        _languageDetectionService = languageDetectionService;
        _renderingService = renderingService;
        _logger = logger;
    }

    public override bool CanValidate(string fileExtension)
        => fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public override async Task<ValidationResult> ValidateAsync(Stream document, IEnumerable<ValidationRule> rules)
    {
        using var memoryStream = new MemoryStream();
        await document.CopyToAsync(memoryStream);

        memoryStream.Position = 0;
        using var wordDocument = WordprocessingDocument.Open(memoryStream, false);

        memoryStream.Position = 0;
        return await ValidateRulesAsync(wordDocument, memoryStream, rules);
    }

    protected override async Task<ValidationIssue> ValidateNumericRuleAsync(
        WordprocessingDocument document,
        NumericRule rule)
    {
        var (actualValues, countContext) = (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Page, ERuleProperty.MarginTop) =>
                (_docxParsingService.GetPageMarginTop(document, rule.Unit), "vead loendatud sektsioonide kaupa"),
            (ERuleTarget.Page, ERuleProperty.MarginBottom) =>
                (_docxParsingService.GetPageMarginBottom(document, rule.Unit), "vead loendatud sektsioonide kaupa"),
            (ERuleTarget.Page, ERuleProperty.MarginLeft) =>
                (_docxParsingService.GetPageMarginLeft(document, rule.Unit), "vead loendatud sektsioonide kaupa"),
            (ERuleTarget.Page, ERuleProperty.MarginRight) =>
                (_docxParsingService.GetPageMarginRight(document, rule.Unit), "vead loendatud sektsioonide kaupa"),
            (ERuleTarget.Page, ERuleProperty.MarginFooter) =>
                (_docxParsingService.GetPageMarginFooter(document, rule.Unit), "vead loendatud sektsioonide kaupa"),
            (ERuleTarget.Paragraph, ERuleProperty.FontSize) =>
                (_docxParsingService.GetParagraphFontSizes(document, rule.StyleFilters, rule.FontFilters, rule.ExcludeFontFilters), "vead loendatud tekstijooksude kaupa"),
            (ERuleTarget.Table, ERuleProperty.FontSize) =>
                (_docxParsingService.GetTableCellFontSizes(document, rule.AfterSectionTitle, rule.BeforeSectionTitle), "vead loendatud tabeli ruutude kaupa"),
            _ => (null, null)
        };

        return RuleEvaluator.EvaluateNumeric(rule, actualValues, countContext);
    }

    protected override async Task<ValidationIssue> ValidateBooleanRuleAsync(WordprocessingDocument document,
        BooleanRule rule)
    {
        var actualValues = (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Paragraph, ERuleProperty.Bold) => _docxParsingService.GetParagraphBoldValues(document,
                rule.StyleFilters),
            _ => null
        };

        return RuleEvaluator.EvaluateBoolean(rule, actualValues);
    }

    protected override Task<ValidationIssue> ValidateEnumRuleAsync(WordprocessingDocument document, EnumRule rule)
    {
        var actualValues = (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Paragraph, ERuleProperty.Alignment) => _docxParsingService.GetParagraphAlignments(
                document,
                rule.StyleFilters,
                rule.ExcludeFontFilters,
                rule.AfterSectionTitle,
                rule.BeforeSectionTitle),
            _ => null
        };

        return Task.FromResult(RuleEvaluator.EvaluateEnum(rule, actualValues));
    }

    protected override Task<ValidationIssue> ValidateRegexRuleAsync(WordprocessingDocument document, RegexRule rule)
    {
        List<string> actualValues;

        if (rule.AfterSectionTitle != null || rule.BeforeSectionTitle != null)
        {
            actualValues = _docxParsingService.GetSectionTitles(
                document,
                startFromHeading: rule.AfterSectionTitle,
                endWithHeading: rule.BeforeSectionTitle);
        }
        else
        {
            actualValues = _docxParsingService.GetParagraphTexts(document, rule.StyleFilters);
        }

        return Task.FromResult(RuleEvaluator.EvaluateRegex(rule, actualValues));
    }

    protected override Task<ValidationIssue> ValidateCountRuleAsync(
        WordprocessingDocument document,
        Stream rawStream,
        CountRule rule)
    {
        var result = (rule.Target, rule.Property) switch
        {
            (ERuleTarget.Section, ERuleProperty.ParagraphCount) =>
                RuleEvaluator.EvaluateCount(rule, _docxParsingService.GetParagraphCountsPerSubsection(document)),
            (ERuleTarget.Document, ERuleProperty.PageCount) =>
                RuleEvaluator.EvaluateCount(rule, _renderingService.GetPageCount(
                    rawStream, rule.StartSectionTitle, rule.EndSectionTitle)),
            _ => RuleEvaluator.EvaluateUnknownRule(rule)
        };

        return Task.FromResult(result);
    }

    protected override Task<ValidationIssue> ValidateOrderRuleAsync(WordprocessingDocument document, OrderRule rule)
    {
        var actualOrder = (rule.Target, rule.OrderType) switch
        {
            (ERuleTarget.Document, EOrderType.Fixed) => _docxParsingService.GetSectionTitles(document),
            _ => null
        };

        return Task.FromResult(RuleEvaluator.EvaluateOrder(rule, actualOrder));
    }

    protected override Task<ValidationIssue> ValidateCrossReferenceRuleAsync(WordprocessingDocument document,
        CrossReferenceRule rule)
    {
        var result = (rule.Target, rule.ReferenceTarget) switch
        {
            (ERuleTarget.Document, EReferenceTarget.BodyText) => EvaluateGlossaryTermsInText(document, rule),
            _ => RuleEvaluator.EvaluateUnknownRule(rule)
        };

        return Task.FromResult(result);
    }

    protected override Task<ValidationIssue> ValidateLanguageRuleAsync(
        WordprocessingDocument document,
        LanguageRule rule)
    {
        List<ESupportedLanguage?>? detectedLanguages = null;

        if (string.IsNullOrEmpty(rule.SectionTitle))
        {
            return Task.FromResult(
                RuleEvaluator.EvaluateLanguage(rule, detectedLanguages, "Sektsiooni pealkiri puudub"));
        }

        var paragraphs = _docxParsingService.GetSectionParagraphs(document, rule.SectionTitle);

        if (paragraphs.Count == 0)
        {
            return Task.FromResult(RuleEvaluator.EvaluateLanguage(rule, detectedLanguages,
                "Sektsiooni teksti ei leitud"));
        }

        detectedLanguages = paragraphs
            .Select(p => _languageDetectionService.DetectLanguage(p))
            .ToList();

        return Task.FromResult(RuleEvaluator.EvaluateLanguage(rule, detectedLanguages));
    }

    private ValidationIssue EvaluateGlossaryTermsInText(WordprocessingDocument document, CrossReferenceRule rule)
    {
        List<string>? terms = null;
        string? bodyText = null;

        if (string.IsNullOrEmpty(rule.SectionTitle))
        {
            return RuleEvaluator.EvaluateCrossReference(
                rule, terms, bodyText, reason: "Kontrollitava sektsiooni pealkiri puudub");
        }

        terms = _docxParsingService.GetGlossaryTerms(document, rule.SectionTitle);
        bodyText = _docxParsingService.GetBodyText(document, rule.AfterSectionTitle, rule.BeforeSectionTitle);

        return RuleEvaluator.EvaluateCrossReference(rule, terms, bodyText, detailsLabel: "Töö põhitekstist mitte leitud mõisted");
    }
}

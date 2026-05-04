using ThesisValidator.Domain.Enums;
using ThesisValidator.Domain.Models;

namespace ThesisValidator.DAL.Interfaces;

public interface IDocumentParsingService<in TDocument>
{
    List<double> GetPageMarginTop(TDocument document, EUnit unit);
    List<double> GetPageMarginBottom(TDocument document, EUnit unit);
    List<double> GetPageMarginLeft(TDocument document, EUnit ruleUnit);
    List<double> GetPageMarginRight(TDocument document, EUnit unit);
    List<double> GetPageMarginFooter(TDocument document, EUnit unit);

    List<double> GetParagraphFontSizes(
        TDocument document,
        List<string>? styleFilters,
        List<string>? fontFilters,
        List<string>? excludeFontFilters);

    List<double> GetTableCellFontSizes(TDocument document, string? afterSectionTitle, string? beforeSectionTitle);
    List<bool> GetParagraphBoldValues(TDocument document, List<string>? styleFilters);

    List<string> GetParagraphAlignments(
        TDocument document,
        List<string>? styleFilters,
        List<string>? excludeFontFilters,
        string? afterSectionTitle = null,
        string? beforeSectionTitle = null);

    List<string> GetParagraphTexts(TDocument document, List<string>? styleFilters);
    List<ItemCount> GetParagraphCountsPerSubsection(TDocument document);
    List<string> GetSectionTitles(TDocument document, string? startFromHeading = null, string? endWithHeading = null);
    List<string> GetGlossaryTerms(TDocument document, string sectionTitle);
    string GetBodyText(TDocument document, string? afterSectionTitle = null, string? beforeSectionTitle = null);
    List<string> GetSectionParagraphs(TDocument document, string sectionTitle);
}

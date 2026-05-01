using ThesisValidator.Domain.Enums;

namespace ThesisValidator.DAL.Interfaces;

public interface IDocumentParsingService<in TDocument>
{
    List<double> GetPageMarginTop(TDocument document, EUnit unit);
    List<double> GetPageMarginBottom(TDocument document, EUnit unit);
    List<double> GetPageMarginLeft(TDocument document, EUnit ruleUnit);
    List<double> GetPageMarginRight(TDocument document, EUnit unit);
    List<double> GetPageMarginFooter(TDocument document, EUnit unit);
    List<double> GetParagraphFontSizes(TDocument document, List<string>? styleFilters, List<string>? fontFilters);
    List<double> GetTableCellFontSizes(TDocument document, string? afterSectionTitle, string? beforeSectionTitle);
    List<bool> GetParagraphBoldValues(TDocument document, List<string>? styleFilters);
    List<string> GetParagraphAlignments(TDocument document, List<string>? styleFilters);
    List<string> GetParagraphTexts(TDocument document, List<string>? styleFilters);
    int GetMinParagraphCountInSubsection(TDocument document);
    List<string> GetSectionTitles(TDocument document, string? startFromHeading = null, string? endWithHeading = null);
    List<string> GetGlossaryTerms(TDocument document, string sectionTitle);
    string GetBodyText(TDocument document);
    List<string> GetSectionParagraphs(TDocument document, string sectionTitle);
}

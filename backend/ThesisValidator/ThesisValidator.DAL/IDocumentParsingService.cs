using ThesisValidator.Domain.Enums;

namespace ThesisValidator.DAL;

public interface IDocumentParsingService<TDocument>
{
    double? GetPageMarginTop(TDocument document, EUnit unit);
    double? GetPageMarginBottom(TDocument document, EUnit unit);
    double? GetPageMarginLeft(TDocument document, EUnit ruleUnit);

    double? GetPageMarginRight(TDocument document, EUnit unit);
    double? GetPageMarginFooter(TDocument document, EUnit unit);
    bool? GetParagraphBold(TDocument document, List<string>? styleFilters);
    List<string> GetParagraphAlignments(TDocument document, List<string>? styleFilters);
    List<string> GetParagraphTexts(TDocument document, List<string>? styleFilters);
    int GetMinParagraphCountInSubsection(TDocument document);
    List<string> GetSectionTitles(TDocument document);
    List<string> GetGlossaryTerms(TDocument document, string sectionTitle);
    string GetBodyText(TDocument document);
}

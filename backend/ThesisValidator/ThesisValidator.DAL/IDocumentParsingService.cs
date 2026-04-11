using ThesisValidator.Domain.Enums;

namespace ThesisValidator.DAL;

public interface IDocumentParsingService<TDocument>
{
    double? GetPageMarginLeft(TDocument document, EUnit ruleUnit);
    bool? GetParagraphBold(TDocument document, List<string>? styleFilters);
    List<string> GetParagraphAlignments(TDocument document, List<string>? styleFilters);
    List<string> GetParagraphTexts(TDocument document, List<string>? styleFilters);
    int GetMinParagraphCountInSubsection(TDocument document);
    List<string> GetSectionTitles(TDocument document);
    List<string> GetGlossaryTerms(TDocument document, string sectionTitle);
    string GetBodyText(TDocument document);
}

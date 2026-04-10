using DocumentFormat.OpenXml.Packaging;
using ThesisValidator.Domain.Enums;

namespace ThesisValidator.DAL;

public interface IDocumentParsingService
{
    // TODO: kaota ära WordprocessingDocument mingi T vastu.
    double? GetPageMarginLeft(WordprocessingDocument document, EUnit ruleUnit);
    bool? GetParagraphBold(WordprocessingDocument document, List<string>? styleFilters);

    List<string> GetParagraphAlignments(WordprocessingDocument document, List<string>? styleFilters);

    List<string> GetParagraphTexts(WordprocessingDocument document, List<string>? styleFilters);

    int GetMinParagraphCountInSubsection(WordprocessingDocument document);

    List<string> GetSectionTitles(WordprocessingDocument document);

    List<string> GetGlossaryTerms(WordprocessingDocument document, string sectionTitle);
    string GetBodyText(WordprocessingDocument document);
}

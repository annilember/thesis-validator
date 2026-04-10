using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ThesisValidator.Domain.Enums;

namespace ThesisValidator.DAL.Docx;

public class DocxParsingService : IDocumentParsingService
{
    public Task<double?> GetPageMarginLeftAsync(Stream document, EUnit ruleUnit)
    {
        using var wordDocument = WordprocessingDocument.Open(document, false);
        var sectionProperties = wordDocument.MainDocumentPart?
            .Document?.Body?
            .GetFirstChild<SectionProperties>();

        var pageMargin = sectionProperties?.GetFirstChild<PageMargin>();

        if (pageMargin?.Left == null)
        {
            return Task.FromResult<double?>(null);
        }

        var marginValue = UnitConverter.TwipsToUnit(pageMargin.Left.Value, ruleUnit);

        Console.WriteLine($"Left margin raw: {pageMargin.Left.Value}, converted: {marginValue}");

        return Task.FromResult<double?>(marginValue);
    }
}

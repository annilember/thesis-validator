using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ThesisValidator.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ThesisValidator.DAL.Docx;

public class DocxParsingService : IDocumentParsingService<WordprocessingDocument>
{
    private readonly ILogger<DocxParsingService> _logger;

    public DocxParsingService(ILogger<DocxParsingService> logger)
    {
        _logger = logger;
    }

    public double? GetPageMarginLeft(WordprocessingDocument document, EUnit unit)
    {
        var pageMargin = document.MainDocumentPart?
            .Document?.Body?
            .GetFirstChild<SectionProperties>()?
            .GetFirstChild<PageMargin>();

        if (pageMargin?.Left == null)
            return null;

        var marginValue = UnitConverter.TwipsToUnit(pageMargin.Left.Value, unit);
        _logger.LogDebug("Left margin raw: {Raw}, converted: {Converted}", pageMargin.Left.Value, marginValue);
        return marginValue;
    }

    public bool? GetParagraphBold(WordprocessingDocument document, List<string>? styleFilters)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return null;
        }

        var paragraphs = body.Descendants<Paragraph>().AsEnumerable();

        if (styleFilters != null && styleFilters.Count > 0)
        {
            paragraphs = paragraphs.Where(p =>
                styleFilters.Contains(
                    p.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? string.Empty));
        }

        foreach (var paragraph in paragraphs)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            var isBoldFromRun = paragraph.Descendants<Run>()
                .Any(r => r.RunProperties?.Bold != null);

            var isBoldFromParagraphMark = paragraph.ParagraphProperties?
                .ParagraphMarkRunProperties?.GetFirstChild<Bold>() != null;

            var isBoldFromStyle = StyleResolver.ResolveBold(document, styleId) ?? false;

            var isBold = isBoldFromRun || isBoldFromParagraphMark || isBoldFromStyle;

            _logger.LogDebug("Paragraph style: {Style}, Bold: {Bold}", styleId, isBold);

            if (!isBold)
            {
                return false;
            }
        }

        return true;
    }

    public List<string> GetParagraphAlignments(WordprocessingDocument document, List<string>? styleFilters)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var paragraphs = body.Descendants<Paragraph>().AsEnumerable();

        if (styleFilters != null && styleFilters.Count > 0)
        {
            paragraphs = paragraphs.Where(p =>
                styleFilters.Contains(
                    p.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? string.Empty));
        }

        var alignments = new List<string>();

        foreach (var paragraph in paragraphs)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            var alignment = paragraph.ParagraphProperties?.Justification?.Val?.Value is { } val
                ? ValueMapper.MapAlignment(val)
                : StyleResolver.ResolveAlignment(document, styleId);

            if (alignment != null)
            {
                _logger.LogDebug("Paragraph style: {Style}, Alignment: {Alignment}", styleId, alignment);
                alignments.Add(alignment);
            }
        }

        return alignments;
    }

    public List<string> GetParagraphTexts(WordprocessingDocument document, List<string>? styleFilters)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var paragraphs = body.Descendants<Paragraph>().AsEnumerable();

        if (styleFilters != null && styleFilters.Count > 0)
        {
            paragraphs = paragraphs.Where(p =>
                styleFilters.Contains(
                    p.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? string.Empty));
        }

        return paragraphs
            .Select(p => p.InnerText)
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
    }

    public int GetMinParagraphCountInSubsection(WordprocessingDocument document)
    {
        //TODO: kontrolli loogika üle.
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
            return 0;

        var paragraphs = body.Descendants<Paragraph>().ToList();
        var headingStyles = new[] { "Heading2", "Heading3" };

        int minCount = int.MaxValue;
        int currentCount = 0;
        bool inSubsection = false;

        foreach (var paragraph in paragraphs)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            if (headingStyles.Contains(styleId))
            {
                if (inSubsection)
                    minCount = Math.Min(minCount, currentCount);

                currentCount = 0;
                inSubsection = true;
            }
            else if (inSubsection && styleId == "Normal")
            {
                currentCount++;
            }
        }

        if (inSubsection)
            minCount = Math.Min(minCount, currentCount);

        return minCount == int.MaxValue ? 0 : minCount;
    }

    public List<string> GetSectionTitles(WordprocessingDocument document)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        return body.Descendants<Paragraph>()
            .Where(p =>
            {
                var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                return styleId == DocxStyles.Heading1 || styleId == DocxStyles.HeadingUnnumbered;
            })
            .Select(p => p.InnerText.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
    }

    public List<string> GetGlossaryTerms(WordprocessingDocument document, string sectionTitle)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
            return [];

        var elements = body.ChildElements.ToList();
        bool foundHeading = false;

        _logger.LogDebug("Looking for section: {Title}", sectionTitle);

        foreach (var element in elements)
        {
            if (element is Paragraph paragraph)
            {
                var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                var text = paragraph.InnerText.Trim();
                _logger.LogDebug("Element: Paragraph, Style: {Style}, Text: {Text}", styleId, text);

                if ((styleId == DocxStyles.HeadingUnnumbered || styleId == DocxStyles.Heading1 ||
                     styleId == DocxStyles.Heading) && text == sectionTitle)
                {
                    foundHeading = true;
                    continue;
                }
            }
            else if (element is Table)
            {
                _logger.LogDebug("Element: Table");
            }

            if (foundHeading && element is Table table)
            {
                return table.Descendants<TableRow>()
                    .Select(row => row.Descendants<TableCell>().FirstOrDefault()?.InnerText.Trim() ?? string.Empty)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }
        }

        return [];
    }

    public string GetBodyText(WordprocessingDocument document)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return string.Empty;
        }

        return string.Join(" ", body.Descendants<Paragraph>()
            .Where(p =>
            {
                var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                return styleId == DocxStyles.Normal || styleId == null;
            })
            .Select(p => p.InnerText));
    }
}

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ThesisValidator.Domain.Enums;
using Microsoft.Extensions.Logging;
using ThesisValidator.DAL.Interfaces;

namespace ThesisValidator.DAL.Docx;

public class DocxParsingService : IDocumentParsingService<WordprocessingDocument>
{
    private readonly ILogger<DocxParsingService> _logger;

    public DocxParsingService(ILogger<DocxParsingService> logger)
    {
        _logger = logger;
    }

    public double? GetPageMarginLeft(WordprocessingDocument document, EUnit unit) =>
        GetPageMargin(document, unit, m => m.Left?.Value);

    public double? GetPageMarginRight(WordprocessingDocument document, EUnit unit) =>
        GetPageMargin(document, unit, m => m.Right?.Value);

    public double? GetPageMarginTop(WordprocessingDocument document, EUnit unit) =>
        GetPageMargin(document, unit, m => m.Top?.Value);

    public double? GetPageMarginBottom(WordprocessingDocument document, EUnit unit) =>
        GetPageMargin(document, unit, m => m.Bottom?.Value);

    public double? GetPageMarginFooter(WordprocessingDocument document, EUnit unit) =>
        GetPageMargin(document, unit, m => m.Footer?.Value);

    private double? GetPageMargin(WordprocessingDocument document, EUnit unit, Func<PageMargin, long?> selector)
    {
        // TODO: kontrolli kõiki sektsioone
        var pageMargin = document.MainDocumentPart?.Document?.Body?
            .GetFirstChild<SectionProperties>()?
            .GetFirstChild<PageMargin>();

        if (pageMargin == null)
        {
            return null;
        }

        var value = selector(pageMargin);
        if (value == null)
        {
            return null;
        }

        return UnitConverter.TwipsToUnit(value.Value, unit);
    }

    public List<double> GetParagraphFontSizes(
        WordprocessingDocument document,
        List<string>? styleFilters,
        List<string>? fontFilters)
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
            {
                var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                return styleFilters.Contains(styleId ?? string.Empty) ||
                       (styleId == null && styleFilters.Contains(DocxStyles.Normal));
            });
        }

        if (fontFilters != null && fontFilters.Count > 0)
        {
            paragraphs = paragraphs.Where(p =>
            {
                var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                var font = StyleResolver.ResolveFont(document, styleId);
                return font != null && fontFilters.Contains(font);
            });
        }

        var fontSizes = new List<double>();

        foreach (var paragraph in paragraphs)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            // WordprocessingML paragraphs without an explicit style inherit from Normal by default
            var effectiveStyleId = styleId ?? DocxStyles.Normal;
            double? halfPoints;

            if (fontFilters != null && fontFilters.Count > 0)
            {
                // In case of fontFilters also check fonts on run level
                var runVal = paragraph.Descendants<Run>()
                    .Select(r => r.RunProperties?.FontSize?.Val?.Value)
                    .FirstOrDefault(v => v != null);

                double? halfPointsFromRun = null;
                if (runVal != null && double.TryParse(runVal, out var runResult))
                {
                    halfPointsFromRun = runResult;
                }

                halfPoints = halfPointsFromRun ?? StyleResolver.ResolveFontSize(document, effectiveStyleId);
            }
            else
            {
                halfPoints = StyleResolver.ResolveFontSize(document, effectiveStyleId);
            }

            if (halfPoints != null)
            {
                fontSizes.Add(UnitConverter.HalfPointsToPt((int)halfPoints));
            }
        }

        return fontSizes;
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

    public List<string> GetSectionTitles(
        WordprocessingDocument document,
        string? startFromHeading = null,
        string? endWithHeading = null)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var allTitles = body.Descendants<Paragraph>()
            .Where(p => DocxStyles.Level1Headings.Contains(
                p.ParagraphProperties?.ParagraphStyleId?.Val?.Value))
            .Select(p => p.InnerText.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        var startIndex = startFromHeading != null
            ? allTitles.IndexOf(startFromHeading) + 1
            : 0;

        var endIndex = endWithHeading != null
            ? allTitles.IndexOf(endWithHeading)
            : allTitles.Count;

        if (startIndex < 0 || endIndex < 0 || startIndex > endIndex)
        {
            return [];
        }

        return allTitles.Skip(startIndex).Take(endIndex - startIndex).ToList();
    }

    public List<string> GetGlossaryTerms(WordprocessingDocument document, string sectionTitle)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var elements = body.ChildElements.ToList();
        var foundHeading = false;

        foreach (var element in elements)
        {
            if (element is Paragraph paragraph)
            {
                var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                var text = paragraph.InnerText.Trim();

                if ((styleId == DocxStyles.HeadingUnnumbered || styleId == DocxStyles.Heading1 ||
                     styleId == DocxStyles.Heading) && text == sectionTitle)
                {
                    foundHeading = true;
                    continue;
                }
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

    public List<string> GetSectionParagraphs(WordprocessingDocument document, string sectionTitle)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var elements = body.ChildElements.ToList();
        var foundHeading = false;
        var paragraphs = new List<string>();

        foreach (var element in elements)
        {
            if (element is Paragraph paragraph)
            {
                var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                var text = paragraph.InnerText.Trim();

                if (DocxStyles.AllHeadings.Contains(styleId) && text == sectionTitle)
                {
                    foundHeading = true;
                    continue;
                }

                if (foundHeading && DocxStyles.AllHeadings.Contains(styleId))
                {
                    break;
                }

                if (foundHeading && !string.IsNullOrEmpty(text))
                {
                    paragraphs.Add(text);
                }
            }
        }

        return paragraphs;
    }
}

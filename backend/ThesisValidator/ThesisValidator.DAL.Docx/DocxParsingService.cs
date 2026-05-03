using DocumentFormat.OpenXml;
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

    public List<double> GetPageMarginLeft(WordprocessingDocument document, EUnit unit) =>
        GetPageMargins(document, unit, m => m.Left?.Value);

    public List<double> GetPageMarginRight(WordprocessingDocument document, EUnit unit) =>
        GetPageMargins(document, unit, m => m.Right?.Value);

    public List<double> GetPageMarginTop(WordprocessingDocument document, EUnit unit) =>
        GetPageMargins(document, unit, m => m.Top?.Value);

    public List<double> GetPageMarginBottom(WordprocessingDocument document, EUnit unit) =>
        GetPageMargins(document, unit, m => m.Bottom?.Value);

    public List<double> GetPageMarginFooter(WordprocessingDocument document, EUnit unit) =>
        GetPageMargins(document, unit, m => m.Footer?.Value);

    private List<double> GetPageMargins(WordprocessingDocument document, EUnit unit, Func<PageMargin, long?> selector)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var results = new List<double>();
        var sectionProperties = body.Descendants<SectionProperties>().ToList();

        foreach (var section in sectionProperties)
        {
            var pageMargin = section.GetFirstChild<PageMargin>();
            if (pageMargin == null)
            {
                continue;
            }

            var value = selector(pageMargin);
            if (value != null)
            {
                results.Add(UnitConverter.TwipsToUnit(value.Value, unit));
            }
        }

        return results;
    }

    public List<double> GetParagraphFontSizes(
        WordprocessingDocument document,
        List<string>? styleFilters,
        List<string>? fontFilters)
    {
        if (styleFilters != null && styleFilters.Count > 0)
        {
            return GetFontSizesByStyle(document, styleFilters);
        }

        if (fontFilters != null && fontFilters.Count > 0)
        {
            return GetFontSizesByFont(document, fontFilters);
        }

        return [];
    }

    public List<double> GetTableCellFontSizes(
        WordprocessingDocument document,
        string? afterSectionTitle,
        string? beforeSectionTitle)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var elementsInScope = GetElementsInScope(body, afterSectionTitle, beforeSectionTitle);
        var fontSizes = new List<double>();

        foreach (var element in elementsInScope)
        {
            if (element is not Table table)
            {
                continue;
            }

            foreach (var cell in table.Descendants<TableCell>())
            {
                foreach (var paragraph in cell.Descendants<Paragraph>())
                {
                    var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                    var effectiveStyleId = styleId ?? DocxStyles.Normal;

                    var runSizes = paragraph.Descendants<Run>()
                        .Select(r => r.RunProperties?.FontSize?.Val?.Value)
                        .Where(v => v != null)
                        .Select(v => double.TryParse(v, out var parsed) ? parsed : (double?)null)
                        .Where(v => v != null)
                        .Select(v => UnitConverter.HalfPointsToPt((int)v!.Value))
                        .ToList();

                    if (runSizes.Count > 0)
                    {
                        fontSizes.AddRange(runSizes);
                    }
                    else
                    {
                        var fromStyle = StyleResolver.ResolveFontSize(document, effectiveStyleId);
                        if (fromStyle != null)
                        {
                            fontSizes.Add(UnitConverter.HalfPointsToPt((int)fromStyle));
                        }
                    }
                }
            }
        }

        return fontSizes;
    }

    public List<bool> GetParagraphBoldValues(WordprocessingDocument document, List<string>? styleFilters)
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

        var results = new List<bool>();

        foreach (var paragraph in paragraphs)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            var contentRuns = paragraph.Descendants<Run>()
                .Where(r => !string.IsNullOrEmpty(r.InnerText))
                .Where(r => !r.Descendants<FieldChar>().Any())
                .Where(r => !r.Descendants<FieldCode>().Any())
                .ToList();

            bool isBold;

            if (contentRuns.Count > 0)
            {
                var isBoldFromStyle = StyleResolver.ResolveBold(document, styleId) ?? false;

                isBold = contentRuns.All(r => // All runs need to be bold
                    r.RunProperties?.Bold == null
                        ? isBoldFromStyle
                        : r.RunProperties.Bold.Val?.Value != false);
            }
            else
            {
                var isBoldFromParagraphMark = paragraph.ParagraphProperties?
                                                  .ParagraphMarkRunProperties?.GetFirstChild<Bold>() is Bold bold &&
                                              bold.Val?.Value != false;

                var isBoldFromStyle = StyleResolver.ResolveBold(document, styleId) ?? false;

                isBold = isBoldFromParagraphMark || isBoldFromStyle;
            }

            results.Add(isBold);
        }

        return results;
    }

    public List<string> GetParagraphAlignments(
        WordprocessingDocument document,
        List<string>? styleFilters,
        List<string>? excludeFontFilters,
        string? afterSectionTitle = null,
        string? beforeSectionTitle = null)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        IEnumerable<Paragraph> paragraphs;

        if (afterSectionTitle != null || beforeSectionTitle != null)
        {
            var elements = GetElementsInScope(body, afterSectionTitle, beforeSectionTitle);
            paragraphs = elements.OfType<Paragraph>();
        }
        else
        {
            paragraphs = body.Descendants<Paragraph>();
        }

        paragraphs = paragraphs.Where(p =>
        {
            var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            if (styleFilters != null && styleFilters.Count > 0)
            {
                var matchesStyle = styleFilters.Contains(styleId ?? string.Empty) ||
                                   (styleId == null && styleFilters.Contains(DocxStyles.Normal));
                if (!matchesStyle)
                {
                    return false;
                }
            }

            if (excludeFontFilters != null && excludeFontFilters.Count > 0)
            {
                var fontFromRun = p.Descendants<Run>()
                    .Select(r => r.RunProperties?.RunFonts?.Ascii?.Value)
                    .FirstOrDefault(f => f != null);
                var fontFromStyle = StyleResolver.ResolveFont(document, styleId);
                var font = fontFromRun ?? fontFromStyle;
                if (font != null && excludeFontFilters.Contains(font))
                {
                    return false;
                }
            }

            return true;
        });

        var alignments = new List<string>();

        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph.InnerText))
            {
                continue;
            }

            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            var alignment = paragraph.ParagraphProperties?.Justification?.Val?.Value is { } val
                ? ValueMapper.MapAlignment(val)
                : StyleResolver.ResolveAlignment(document, styleId) ?? "left";

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

    public List<int> GetParagraphCountsPerSubsection(WordprocessingDocument document)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var counts = new List<int>();
        var currentCount = 0;
        var inSubsection = false;

        foreach (var element in body.ChildElements)
        {
            if (element is Paragraph paragraph)
            {
                var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

                if (DocxStyles.SubHeadings.Contains(styleId))
                {
                    if (inSubsection)
                    {
                        counts.Add(currentCount);
                    }

                    currentCount = 0;
                    inSubsection = true;
                }
                else if (DocxStyles.Level1Headings.Contains(styleId))
                {
                    if (inSubsection)
                    {
                        counts.Add(currentCount);
                    }

                    currentCount = 0;
                    inSubsection = false;
                }
                else if (inSubsection && (styleId == DocxStyles.Normal || styleId == null))
                {
                    if (!string.IsNullOrWhiteSpace(paragraph.InnerText))
                    {
                        currentCount++;
                    }
                }
            }
            else if (inSubsection && element is Table)
            {
                currentCount++;
            }
        }

        if (inSubsection)
        {
            counts.Add(currentCount);
        }

        return counts;
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

        var allTitles = new List<string>();

        if (startFromHeading == null)
        {
            var firstNonEmpty = body.ChildElements
                .FirstOrDefault(e => e is Table ||
                                     (e is Paragraph p && !string.IsNullOrWhiteSpace(p.InnerText)));
            if (firstNonEmpty is Table)
            {
                allTitles.Add("Tiitelleht");
            }
        }

        allTitles.AddRange(body.Descendants<Paragraph>()
            .Where(p => DocxStyles.Level1Headings.Contains(
                p.ParagraphProperties?.ParagraphStyleId?.Val?.Value))
            .Select(p => p.InnerText.Trim())
            .Where(t => !string.IsNullOrEmpty(t)));

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

        var elements = GetElementsInScope(body, afterSectionTitle: sectionTitle, beforeSectionTitle: null);

        foreach (var element in elements)
        {
            if (element is Table table)
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

        var elements = GetElementsInScope(body, afterSectionTitle: sectionTitle, beforeSectionTitle: null);
        var paragraphs = new List<string>();

        foreach (var element in elements)
        {
            if (element is Paragraph p)
            {
                if (DocxStyles.AllHeadings.Contains(p.ParagraphProperties?.ParagraphStyleId?.Val?.Value))
                {
                    break;
                }

                var text = p.InnerText.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    paragraphs.Add(text);
                }
            }
        }

        return paragraphs;
    }

    private List<double> GetFontSizesByStyle(WordprocessingDocument document, List<string> styleFilters)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var paragraphs = body.Descendants<Paragraph>().AsEnumerable();
        paragraphs = paragraphs.Where(p =>
        {
            var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
            return styleFilters.Contains(styleId ?? string.Empty) ||
                   (styleId == null && styleFilters.Contains(DocxStyles.Normal));
        });

        var fontSizes = new List<double>();

        foreach (var paragraph in paragraphs)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            // WordprocessingML paragraphs without an explicit style inherit from Normal by default
            var effectiveStyleId = styleId ?? DocxStyles.Normal;

            // Do not check run level for Normal text
            if (effectiveStyleId != DocxStyles.Normal)
            {
                var runSizes = paragraph.Descendants<Run>()
                    .Where(r => !string.IsNullOrEmpty(r.InnerText))
                    .Where(r => !r.Descendants<FieldChar>().Any())
                    .Where(r => !r.Descendants<FieldCode>().Any())
                    .Select(r => r.RunProperties?.FontSize?.Val?.Value)
                    .Where(v => v != null)
                    .Select(v => double.TryParse(v, out var parsed) ? parsed : (double?)null)
                    .Where(v => v != null)
                    .Select(v => UnitConverter.HalfPointsToPt((int)v!.Value))
                    .ToList();

                if (runSizes.Count > 0)
                {
                    fontSizes.AddRange(runSizes);
                    continue;
                }
            }

            var fromStyle = StyleResolver.ResolveFontSize(document, effectiveStyleId);
            if (fromStyle != null)
            {
                fontSizes.Add(UnitConverter.HalfPointsToPt((int)fromStyle));
            }
        }

        return fontSizes;
    }

    private List<double> GetFontSizesByFont(WordprocessingDocument document, List<string> fontFilters)
    {
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return [];
        }

        var paragraphs = body.Descendants<Paragraph>().AsEnumerable();

        paragraphs = paragraphs.Where(p =>
        {
            var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
            var fontFromStyle = StyleResolver.ResolveFont(document, styleId);

            // TODO: Currently only the first run's font is checked. For more accurate
            // validation, all runs should be checked as a paragraph may contain mixed fonts.
            var fontFromRun = p.Descendants<Run>()
                .Select(r => r.RunProperties?.RunFonts?.Ascii?.Value)
                .FirstOrDefault(f => f != null);

            var font = fontFromRun ?? fontFromStyle;
            return font != null && fontFilters.Contains(font);
        });

        var fontSizes = new List<double>();

        foreach (var paragraph in paragraphs)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            // WordprocessingML paragraphs without an explicit style inherit from Normal by default
            var effectiveStyleId = styleId ?? DocxStyles.Normal;

            // Check all runs in a paragraph
            var runSizes = paragraph.Descendants<Run>()
                .Select(r => r.RunProperties?.FontSize?.Val?.Value)
                .Where(v => v != null)
                .Select(v => double.TryParse(v, out var parsed) ? parsed : (double?)null)
                .Where(v => v != null)
                .Select(v => UnitConverter.HalfPointsToPt((int)v!.Value))
                .ToList();

            if (runSizes.Count > 0)
            {
                fontSizes.AddRange(runSizes);
            }
            else
            {
                var fromStyle = StyleResolver.ResolveFontSize(document, effectiveStyleId);
                if (fromStyle != null)
                {
                    fontSizes.Add(UnitConverter.HalfPointsToPt((int)fromStyle));
                }
            }
        }

        return fontSizes;
    }

    private List<OpenXmlElement> GetElementsInScope(
        Body body,
        string? afterSectionTitle,
        string? beforeSectionTitle)
    {
        var result = new List<OpenXmlElement>();
        var inScope = afterSectionTitle == null;

        foreach (var element in body.ChildElements)
        {
            if (element is Paragraph p)
            {
                var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                var text = p.InnerText.Trim();

                if (!inScope && DocxStyles.Level1Headings.Contains(styleId) && text == afterSectionTitle)
                {
                    inScope = true;

                    continue;
                }

                if (inScope && beforeSectionTitle != null && DocxStyles.Level1Headings.Contains(styleId)
                    && (text == beforeSectionTitle || text.StartsWith(beforeSectionTitle + " ")))
                {
                    break;
                }
            }

            if (inScope)
            {
                result.Add(element);
            }
        }

        return result;
    }
}

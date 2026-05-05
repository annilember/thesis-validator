using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ThesisValidator.DAL.Docx;

public static class StyleResolver
{
    public static double? ResolveFontSize(WordprocessingDocument document, string? styleId) =>
        ResolveProperty<double>(
            document,
            styleId,
            s => s.StyleRunProperties?.FontSize?.Val?.Value is { } v && double.TryParse(v, out var result)
                ? result
                : null,
            d => d.RunPropertiesDefault?.RunPropertiesBaseStyle?.FontSize?.Val?.Value is { } v &&
                 double.TryParse(v, out var result)
                ? result
                : null);

    public static string? ResolveFont(WordprocessingDocument document, string? styleId) =>
        ResolvePropertyClass<string>(
            document,
            styleId,
            s => s.StyleRunProperties?.RunFonts?.Ascii?.Value,
            d => d.RunPropertiesDefault?.RunPropertiesBaseStyle?.RunFonts?.Ascii?.Value);

    public static bool? ResolveBold(WordprocessingDocument document, string? styleId) =>
        ResolveProperty(
            document,
            styleId,
            s => s.StyleRunProperties?.Bold != null ? (bool?)true : null,
            d => d.RunPropertiesDefault?.RunPropertiesBaseStyle?.Bold != null ? (bool?)true : null);

    public static string? ResolveAlignment(WordprocessingDocument document, string? styleId)
    {
        if (styleId == null)
        {
            // Try Normal style first
            var normalAlignment = ResolveAlignment(document, DocxStyles.Normal);
            if (normalAlignment != null)
            {
                return normalAlignment;
            }

            // Fallback to DocDefaults
            var docDefaults = document.MainDocumentPart?.StyleDefinitionsPart?.Styles?.DocDefaults;
            var defaultAlignment = docDefaults?.ParagraphPropertiesDefault?
                .ParagraphPropertiesBaseStyle?.Justification?.Val?.Value;
            return defaultAlignment != null ? ValueMapper.MapAlignment(defaultAlignment) : null;
        }

        var style = GetStyle(document, styleId);
        if (style == null)
        {
            return null;
        }

        var alignment = ValueMapper.MapAlignment(style.StyleParagraphProperties?.Justification?.Val?.Value);
        if (alignment != null)
        {
            return alignment;
        }

        return ResolveAlignment(document, style.BasedOn?.Val?.Value);
    }

    private static T? ResolveProperty<T>(
        WordprocessingDocument document,
        string? styleId,
        Func<Style, T?> propertySelector,
        Func<DocDefaults, T?>? docDefaultSelector = null) where T : struct
    {
        if (styleId != null)
        {
            var style = GetStyle(document, styleId);
            if (style != null)
            {
                var value = propertySelector(style);
                if (value != null)
                {
                    return value;
                }

                var fromBase = ResolveProperty(document, style.BasedOn?.Val?.Value, propertySelector,
                    docDefaultSelector);
                if (fromBase != null)
                {
                    return fromBase;
                }
            }
        }

        // DocDefaults
        if (docDefaultSelector != null)
        {
            var docDefaults = document.MainDocumentPart?.StyleDefinitionsPart?.Styles?.DocDefaults;
            if (docDefaults != null)
            {
                return docDefaultSelector(docDefaults);
            }
        }

        return null;
    }

    private static T? ResolvePropertyClass<T>(
        WordprocessingDocument document,
        string? styleId,
        Func<Style, T?> propertySelector,
        Func<DocDefaults, T?>? docDefaultSelector = null) where T : class
    {
        if (styleId != null)
        {
            var style = GetStyle(document, styleId);
            if (style != null)
            {
                var value = propertySelector(style);
                if (value != null)
                {
                    return value;
                }

                var fromBase = ResolvePropertyClass(document, style.BasedOn?.Val?.Value, propertySelector,
                    docDefaultSelector);
                if (fromBase != null)
                {
                    return fromBase;
                }
            }
        }

        if (docDefaultSelector != null)
        {
            var docDefaults = document.MainDocumentPart?.StyleDefinitionsPart?.Styles?.DocDefaults;
            if (docDefaults != null)
            {
                return docDefaultSelector(docDefaults);
            }
        }

        return null;
    }

    private static Style? GetStyle(WordprocessingDocument document, string styleId)
    {
        return document.MainDocumentPart?.StyleDefinitionsPart?.Styles?
            .Descendants<Style>()
            .FirstOrDefault(s => s.StyleId?.Value == styleId);
    }
}

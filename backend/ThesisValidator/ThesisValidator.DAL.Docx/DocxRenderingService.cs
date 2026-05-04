using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using ThesisValidator.DAL.Interfaces;
using ThesisValidator.Domain.Models;

namespace ThesisValidator.DAL.Docx;

public class DocxRenderingService : IDocumentRenderingService
{
    public ItemCount GetPageCount(Stream document, string? startTitle, string? endTitle)
    {
        document.Position = 0;
        using var wordDocument = new WordDocument(document, FormatType.Docx);

        if (endTitle != null)
        {
            var endSectionIndex = -1;
            for (int i = 0; i < wordDocument.Sections.Count; i++)
            {
                var section = wordDocument.Sections[i];
                var firstPara = section?.Body.ChildEntities.OfType<WParagraph>().FirstOrDefault();
                if (firstPara != null &&
                    DocxStyles.Level1HeadingsSyncfusion.Contains(firstPara.StyleName) &&
                    firstPara.Text.Trim() == endTitle)
                {
                    endSectionIndex = i;
                    break;
                }
            }

            if (endSectionIndex != -1)
            {
                for (int i = wordDocument.Sections.Count - 1; i > endSectionIndex; i--)
                {
                    wordDocument.Sections.Remove(wordDocument.Sections[i]);
                }
            }
        }

        if (startTitle != null)
        {
            var startSectionIndex = -1;
            for (int i = 0; i < wordDocument.Sections.Count; i++)
            {
                var section = wordDocument.Sections[i];
                var firstPara = section?.Body.ChildEntities.OfType<WParagraph>().FirstOrDefault();
                if (firstPara != null &&
                    DocxStyles.Level1HeadingsSyncfusion.Contains(firstPara.StyleName) &&
                    firstPara.Text.Trim() == startTitle)
                {
                    startSectionIndex = i;
                    break;
                }
            }

            if (startSectionIndex != -1)
            {
                for (int i = startSectionIndex - 1; i >= 0; i--)
                {
                    wordDocument.Sections.Remove(wordDocument.Sections[i]);
                }
            }
        }

        using var renderer = new DocIORenderer();
        using var pdfDocument = renderer.ConvertToPDF(wordDocument);

        return new ItemCount(null, pdfDocument.Pages.Count);
    }
}

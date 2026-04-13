namespace ThesisValidator.DAL.Interfaces;

public interface IDocumentRenderingService
{
    int GetMainContentPageCount(Stream document, string startTitle, string endTitle);
}

namespace ThesisValidator.DAL.Interfaces;

public interface IDocumentRenderingService
{
    int GetPageCount(Stream document, string? startTitle, string? endTitle);
}

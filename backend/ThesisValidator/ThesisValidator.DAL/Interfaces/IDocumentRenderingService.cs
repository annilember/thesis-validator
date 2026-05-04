using ThesisValidator.Domain.Models;

namespace ThesisValidator.DAL.Interfaces;

public interface IDocumentRenderingService
{
    ItemCount GetPageCount(Stream document, string? startTitle, string? endTitle);
}

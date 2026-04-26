using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Interfaces;

public interface ILanguageDetectionService
{
    ESupportedLanguage? DetectLanguage(string text);
}

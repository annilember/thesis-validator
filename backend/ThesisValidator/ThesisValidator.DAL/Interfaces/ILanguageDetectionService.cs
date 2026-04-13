using ThesisValidator.Domain.Enums;

namespace ThesisValidator.DAL.Interfaces;

public interface ILanguageDetectionService
{
    ESupportedLanguage? DetectLanguage(string text);
}

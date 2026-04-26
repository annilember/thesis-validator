using Lingua;
using ThesisValidator.BLL.Interfaces;
using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Services;

public class LanguageDetectionService : ILanguageDetectionService
{
    private readonly LanguageDetector _detector;

    public LanguageDetectionService()
    {
        _detector = LanguageDetectorBuilder
            .FromLanguages(Language.Estonian, Language.English)
            .Build();
    }

    public ESupportedLanguage? DetectLanguage(string text)
    {
        var detected = _detector.DetectLanguageOf(text);

        return detected switch
        {
            Language.Estonian => ESupportedLanguage.Et,
            Language.English => ESupportedLanguage.En,
            _ => null
        };
    }
}

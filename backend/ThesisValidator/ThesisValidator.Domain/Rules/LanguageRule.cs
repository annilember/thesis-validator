using ThesisValidator.Domain.Enums;

namespace ThesisValidator.Domain.Rules;

public class LanguageRule : ValidationRule
{
    public ESupportedLanguage ExpectedLanguage { get; set; }
}

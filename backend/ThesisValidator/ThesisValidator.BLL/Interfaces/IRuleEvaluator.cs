using ThesisValidator.BLL.Models;
using ThesisValidator.Domain.Enums;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Interfaces;

public interface IRuleEvaluator
{
    ValidationIssue EvaluateNumeric(NumericRule rule, double actualValue);
    ValidationIssue EvaluateNumeric(NumericRule rule, List<double> actualValues);

    ValidationIssue EvaluateBoolean(BooleanRule rule, bool actualValue);

    ValidationIssue EvaluateEnum(EnumRule rule, List<string> actualValues);

    ValidationIssue EvaluateRegex(RegexRule rule, List<string> actualValues);

    ValidationIssue EvaluateCount(CountRule rule, int actualCount);

    ValidationIssue EvaluateOrder(OrderRule rule, List<string> actualOrder);

    ValidationIssue EvaluateCrossReference(CrossReferenceRule rule, List<string> terms, string bodyText);

    ValidationIssue EvaluateLanguage(LanguageRule rule, ESupportedLanguage? detectedLanguage);

    ValidationIssue EvaluateLanguage(LanguageRule rule, List<ESupportedLanguage?> detectedLanguages);
}

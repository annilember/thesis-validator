using ThesisValidator.BLL.Models;
using ThesisValidator.Domain;
using ThesisValidator.Domain.Enums;
using ThesisValidator.Domain.Models;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Interfaces;

public interface IRuleEvaluator
{
    ValidationIssue EvaluateDisabled(ValidationRule rule);

    ValidationIssue EvaluateUnknownRuleType(ValidationRule rule);

    ValidationIssue EvaluateUnknownRule(ValidationRule rule);

    ValidationIssue EvaluateNumeric(NumericRule rule, List<double>? actualValues, string? countContext);

    ValidationIssue EvaluateBoolean(BooleanRule rule, List<bool>? actualValues);

    ValidationIssue EvaluateEnum(EnumRule rule, List<string>? actualValues);

    ValidationIssue EvaluateRegex(RegexRule rule, List<string>? actualValues, string? reason = null);

    ValidationIssue EvaluateCount(CountRule rule, ItemCount? actualCount);

    ValidationIssue EvaluateCount(CountRule rule, List<ItemCount>? actualCounts);

    ValidationIssue EvaluateOrder(OrderRule rule, List<string>? actualOrder);

    ValidationIssue EvaluateCrossReference(
        CrossReferenceRule rule,
        List<string>? terms,
        string? bodyText,
        string? reason = null,
        string? detailsLabel = null);

    ValidationIssue EvaluateLanguage(
        LanguageRule rule,
        List<ESupportedLanguage?>? detectedLanguages,
        string? reason = null);
}

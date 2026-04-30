using System.Text.RegularExpressions;
using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Models;
using ThesisValidator.Domain;
using ThesisValidator.Domain.Enums;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Services;

public class RuleEvaluator : IRuleEvaluator
{
    public ValidationIssue EvaluateDisabled(ValidationRule rule) =>
        ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Reegel pole sisse lülitatud");

    public ValidationIssue EvaluateUnknownRuleType(ValidationRule rule) =>
        ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Tundmatu reegli tüüp");

    public ValidationIssue EvaluateUnknownRule(ValidationRule rule) =>
        ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Reegli kontrollmehhanism on puudu");

    public ValidationIssue EvaluateNumeric(NumericRule rule, List<double>? actualValues)
    {
        if (actualValues == null || actualValues.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Ühtegi väärtust ei leitud");
        }

        var failedValues = actualValues
            .Where(v => Math.Abs(v - rule.ExpectedValue) > rule.Tolerance)
            .ToList();

        if (failedValues.Count == 0)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        var distinctFailed = failedValues.Distinct().ToList();
        var unit = rule.Unit.ToString().ToLower();
        var failedStr = string.Join(", ", distinctFailed.Select(v => $"{v:F2}{unit}"));
        var details = $"Leitud {failedValues.Count} viga ({distinctFailed.Count} erinevat väärtust: {failedStr}), oodatav: {rule.ExpectedValue}{unit}";

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity, details);
    }

    public ValidationIssue EvaluateBoolean(BooleanRule rule, bool? actualValue)
    {
        if (actualValue == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        return Evaluate(rule, actualValue == rule.ExpectedValue);
    }

    public ValidationIssue EvaluateEnum(EnumRule rule, List<string>? actualValues)
    {
        if (actualValues == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        var allMatch = actualValues.All(v => rule.AllowedValues.Contains(v));

        return Evaluate(rule, allMatch);
    }

    public ValidationIssue EvaluateRegex(RegexRule rule, List<string>? actualValues, string? reason = null)
    {
        if (actualValues == null || actualValues.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, reason ?? "Ühtegi lõiku ei leitud");
        }

        var allMatch = actualValues.All(v => Regex.IsMatch(v, rule.Pattern));

        return Evaluate(rule, allMatch);
    }

    public ValidationIssue EvaluateCount(CountRule rule, int? actualCount)
    {
        if (actualCount == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        var countIsInBounds = actualCount >= rule.MinValue && (rule.MaxValue == null || actualCount <= rule.MaxValue);

        return Evaluate(rule, countIsInBounds);
    }

    public ValidationIssue EvaluateOrder(OrderRule rule, List<string>? actualOrder)
    {
        if (actualOrder == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        if (rule.OrderType == EOrderType.Fixed)
        {
            if (rule.ExpectedOrder == null)
            {
                return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Oodatav järjekord puudub");
            }

            var expectedInActual = rule.ExpectedOrder
                .Where(actualOrder.Contains)
                .ToList();

            var actualFiltered = actualOrder
                .Where(a => rule.ExpectedOrder.Contains(a))
                .ToList();

            if (expectedInActual.SequenceEqual(actualFiltered))
            {
                return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
            }

            return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
        }

        return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Reegli tüüp pole toetatud");
    }

    public ValidationIssue EvaluateCrossReference(
        CrossReferenceRule rule,
        List<string>? terms,
        string? bodyText,
        string? reason = null)
    {
        if (reason != null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, reason);
        }

        if (terms == null || terms.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Kontrollitav loend on tühi");
        }

        if (bodyText == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Dokumendi sisu kättesaamine ebaõnnestus");
        }

        var allFound = terms.All(term => bodyText.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (allFound)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }

    public ValidationIssue EvaluateLanguage(
        LanguageRule rule,
        List<ESupportedLanguage?>? detectedLanguages,
        string? reason = null)
    {
        if (reason != null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, reason);
        }

        var validLanguages = detectedLanguages?.Where(l => l != null).ToList();

        if (validLanguages == null || validLanguages.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Keelt ei õnnestunud tuvastada");
        }

        var allSame = validLanguages.Distinct().Count() == 1;

        if (!allSame)
        {
            return ValidationIssue.CreateFailed(rule.RuleId, "Sektsioon sisaldab mitmes keeles teksti", rule.Severity);
        }

        return Evaluate(rule, validLanguages.First() == rule.ExpectedLanguage);
    }

    private static ValidationIssue Evaluate(ValidationRule rule, bool passed)
    {
        return passed
            ? ValidationIssue.CreatePassed(rule.RuleId, rule.Description)
            : ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }
}

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
        var details =
            $"Leitud {failedValues.Count} viga ({distinctFailed.Count} erinevat väärtust: {failedStr}), oodatav: {rule.ExpectedValue}{unit}";

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity, details);
    }

    public ValidationIssue EvaluateBoolean(BooleanRule rule, List<bool>? actualValues)
    {
        if (actualValues == null || actualValues.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        var violations = actualValues.Count(v => v != rule.ExpectedValue);

        if (violations == 0)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(
            rule.RuleId,
            rule.Message,
            rule.Severity,
            $"Leitud {violations} viga");
    }

    public ValidationIssue EvaluateEnum(EnumRule rule, List<string>? actualValues)
    {
        if (actualValues == null || actualValues.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        var violations = actualValues.Where(v => !rule.AllowedValues.Contains(v)).ToList();

        if (violations.Count == 0)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        var distinctViolations = violations.Distinct().ToList();
        var violationsStr = string.Join(", ", distinctViolations);
        var expectedStr = string.Join(", ", rule.AllowedValues);
        var details =
            $"Leitud {violations.Count} viga ({distinctViolations.Count} erinevat väärtust: {violationsStr}), oodatav: {expectedStr}";

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity, details);
    }

    public ValidationIssue EvaluateRegex(RegexRule rule, List<string>? actualValues, string? reason = null)
    {
        if (actualValues == null || actualValues.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, reason ?? "Ühtegi lõiku ei leitud");
        }

        var violations = actualValues
            .Where(v => !Regex.IsMatch(v, rule.Pattern))
            .ToList();

        if (violations.Count == 0)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
            $"Vale formaat leitud: {string.Join(", ", violations)}");
    }

    public ValidationIssue EvaluateCount(CountRule rule, int? actualCount)
    {
        if (actualCount == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        var countIsInBounds = actualCount >= rule.MinValue && (rule.MaxValue == null || actualCount <= rule.MaxValue);

        if (countIsInBounds)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        var expected = rule.MaxValue != null
            ? $"{rule.MinValue}-{rule.MaxValue}"
            : $"vähemalt {rule.MinValue}";
        var unitStr = rule.Unit != null ? $" {rule.Unit.Value.ToDisplayString()}" : "";

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
            $"Leitud {actualCount}{unitStr}, oodatav: {expected}{unitStr}");
    }

    public ValidationIssue EvaluateCount(CountRule rule, List<int>? actualCounts)
    {
        if (actualCounts == null || actualCounts.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        var violations = actualCounts.Count(c => c < rule.MinValue);

        if (violations == 0)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
            $"Leitud {violations} alampeatükki, milles on vähem kui {rule.MinValue} lõiku");
    }

    public ValidationIssue EvaluateOrder(OrderRule rule, List<string>? actualOrder)
    {
        if (actualOrder == null || actualOrder.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Väärtust ei leitud dokumendist");
        }

        if (rule.OrderType != EOrderType.Fixed)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Reegli tüüp pole toetatud");
        }

        if (rule.ExpectedOrder == null)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, rule.Message, "Oodatav järjekord pole defineeritud");
        }

        bool Matches(string actual, string expected) =>
            actual == expected || actual.StartsWith(expected + " ");

        var missingRequired = rule.ExpectedOrder
            .Where(e => rule.OptionalOrderItems == null || !rule.OptionalOrderItems.Any(o => Matches(e, o)))
            .Where(e => !actualOrder.Any(a => Matches(a, e)))
            .ToList();

        if (missingRequired.Count > 0)
        {
            return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
                $"Puuduvad kohustuslikud osad: {string.Join(", ", missingRequired)}");
        }

        if (rule.AllowUnknownBetween != null)
        {
            var startIndex = actualOrder.FindIndex(a => Matches(a, rule.AllowUnknownBetween[0]));
            var endIndex = actualOrder.FindIndex(a => Matches(a, rule.AllowUnknownBetween[1]));

            var illegalUnknowns = actualOrder
                .Select((a, i) => (a, i))
                .Where(x => !rule.ExpectedOrder.Any(e => Matches(x.a, e)))
                .Where(x => !(x.i > startIndex && x.i < endIndex))
                .Select(x => x.a)
                .ToList();

            if (illegalUnknowns.Count > 0)
            {
                var allowStart = rule.AllowUnknownBetween[0];
                var unknownInScope = actualOrder
                    .Where(a => !rule.ExpectedOrder.Any(e => Matches(a, e)))
                    .ToList();
                var expectedWithUnknowns = new List<string>();

                foreach (var item in rule.ExpectedOrder)
                {
                    var actualMatches = actualOrder
                        .Where(a => Matches(a, item))
                        .ToList();

                    if (actualMatches.Count > 0)
                    {
                        expectedWithUnknowns.AddRange(actualMatches);
                    }

                    if (Matches(item, allowStart))
                    {
                        expectedWithUnknowns.AddRange(unknownInScope);
                    }
                }

                return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
                    $"Oodatud osade järjekord: {string.Join(" → ", expectedWithUnknowns)}");
            }
        }

        var actualFiltered = actualOrder
            .Where(a => rule.ExpectedOrder.Any(e => Matches(a, e)))
            .ToList();

        var expectedFiltered = rule.ExpectedOrder
            .SelectMany(e => actualOrder
                .Where(a => Matches(a, e))
                .Where(a =>
                {
                    var hasExactMatch = rule.ExpectedOrder.Any(exact => exact != e && exact == a);
                    if (hasExactMatch)
                    {
                        return e == a;
                    }

                    return true;
                }))
            .ToList();

        if (expectedFiltered.SequenceEqual(actualFiltered))
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        var expectedStr = string.Join(" → ", expectedFiltered);
        var actualStr = string.Join(" → ", actualFiltered);

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
            $"Leitud järjekord: {actualStr}, oodatav: {expectedStr}");
    }

    public ValidationIssue EvaluateCrossReference(
        CrossReferenceRule rule,
        List<string>? terms,
        string? bodyText,
        string? reason = null,
        string? detailsLabel = null)
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

        var violations = terms
            .Where(term => !Regex.IsMatch(bodyText,
                $@"(?<![a-zA-ZäöüõÄÖÜÕ0-9]){Regex.Escape(term)}(?![a-zA-ZäöüõÄÖÜÕ0-9])",
                RegexOptions.IgnoreCase))
            .ToList();

        if (violations.Count == 0)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        var label = detailsLabel ?? "Tuvastatud veakohad";

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
            $"{label}: {string.Join(", ", violations)}");
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
            var detectedStr = string.Join(", ", validLanguages
                .Distinct()
                .Select(l => l?.ToString().ToLower() ?? "tundmatu"));
            return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
                $"Tuvastati mitu erinevat keelt: {detectedStr}");
        }

        var detectedLanguage = validLanguages.First();
        if (detectedLanguage != rule.ExpectedLanguage)
        {
            return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity,
                $"Tuvastatud keel: {detectedLanguage?.ToString().ToLower()}, oodatav: {rule.ExpectedLanguage.ToString().ToLower()}");
        }

        return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
    }
}

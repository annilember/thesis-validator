using System.Text.RegularExpressions;
using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Models;
using ThesisValidator.Domain.Enums;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Services;

public class RuleEvaluator : IRuleEvaluator
{
    //TODO: tee ühine meetod kui saab.
    public ValidationIssue EvaluateNumeric(NumericRule rule, double actualValue)
    {
        var difference = Math.Abs(actualValue - rule.ExpectedValue);

        if (difference <= rule.Tolerance)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }

    public ValidationIssue EvaluateBoolean(BooleanRule rule, bool actualValue)
    {
        if (actualValue == rule.ExpectedValue)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }

    public ValidationIssue EvaluateEnum(EnumRule rule, List<string> actualValues)
    {
        var allMatch = actualValues.All(v => rule.AllowedValues.Contains(v));

        if (allMatch)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }

    public ValidationIssue EvaluateRegex(RegexRule rule, List<string> actualValues)
    {
        var allMatch = actualValues.All(v => Regex.IsMatch(v, rule.Pattern));

        if (allMatch)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }

    public ValidationIssue EvaluateCount(CountRule rule, int actualCount)
    {
        if (actualCount >= rule.MinValue && (rule.MaxValue == null || actualCount <= rule.MaxValue))
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }

    public ValidationIssue EvaluateOrder(OrderRule rule, List<string> actualOrder)
    {
        if (rule.OrderType == EOrderType.Fixed)
        {
            if (rule.ExpectedOrder == null)
            {
                return ValidationIssue.CreateSkipped(rule.RuleId, "Oodatav järjekord puudub");
            }

            var expectedInActual = rule.ExpectedOrder
                .Where(e => actualOrder.Contains(e))
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

        return ValidationIssue.CreateSkipped(rule.RuleId, "Reegli tüüp pole toetatud");
    }

    public ValidationIssue EvaluateCrossReference(CrossReferenceRule rule, List<string> terms, string bodyText)
    {
        if (terms.Count == 0)
        {
            return ValidationIssue.CreateSkipped(rule.RuleId, "Lühendite sõnastik on tühi");
        }

        var allFound = terms.All(term => bodyText.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (allFound)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }
}

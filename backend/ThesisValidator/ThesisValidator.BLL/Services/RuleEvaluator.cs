using ThesisValidator.BLL.Interfaces;
using ThesisValidator.BLL.Models;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Services;

public class RuleEvaluator : IRuleEvaluator
{
    public ValidationIssue EvaluateNumeric(NumericRule rule, double actualValue)
    {
        var difference = Math.Abs(actualValue - rule.ExpectedValue);

        if (difference <= rule.Tolerance)
        {
            return ValidationIssue.CreatePassed(rule.RuleId, rule.Description);
        }

        return ValidationIssue.CreateFailed(rule.RuleId, rule.Message, rule.Severity);
    }
}

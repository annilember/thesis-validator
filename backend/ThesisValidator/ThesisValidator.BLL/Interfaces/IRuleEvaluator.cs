using ThesisValidator.BLL.Models;
using ThesisValidator.Domain.Rules;

namespace ThesisValidator.BLL.Interfaces;

public interface IRuleEvaluator
{
    ValidationIssue EvaluateNumeric(NumericRule rule, double actualValue);
}

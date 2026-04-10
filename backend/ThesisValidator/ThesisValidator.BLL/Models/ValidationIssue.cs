using ThesisValidator.Domain.Enums;

namespace ThesisValidator.BLL.Models;

public class ValidationIssue
{
    public string RuleId { get; set; } = string.Empty;

    public bool Passed { get; set; }

    public bool Skipped { get; set; }

    public string Message { get; set; } = string.Empty;

    public ERuleSeverity? Severity { get; set; }

    public static ValidationIssue CreateSkipped(string ruleId, string reason) => new()
    {
        RuleId = ruleId,
        Skipped = true,
        Message = reason
    };

    public static ValidationIssue CreatePassed(string ruleId, string description) => new()
    {
        RuleId = ruleId,
        Passed = true,
        Message = description
    };

    public static ValidationIssue CreateFailed(string ruleId, string message, ERuleSeverity severity) => new()
    {
        RuleId = ruleId,
        Passed = false,
        Severity = severity,
        Message = message
    };
}

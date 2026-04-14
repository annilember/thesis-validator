export interface IValidationIssue {
  ruleId: string
  passed: boolean
  skipped: boolean
  message: string
  severity: string | null
}

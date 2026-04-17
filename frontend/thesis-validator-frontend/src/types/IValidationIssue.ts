export interface IValidationIssue {
  ruleId: string
  passed: boolean
  skipped: boolean
  message: string
  details?: string
  severity: string | null
}

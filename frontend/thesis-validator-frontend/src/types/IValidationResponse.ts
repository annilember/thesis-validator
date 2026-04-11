import type { IValidationIssue } from "./IValidationIssue"

export interface IValidationResponse {
  templateId: string
  fileName: string
  issues: IValidationIssue[]
  isValid: boolean
}

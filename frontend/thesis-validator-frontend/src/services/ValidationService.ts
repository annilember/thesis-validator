import type { IResponseObject } from "@/types/IResponseObject";
import { BaseService } from "./BaseService";
import type { IValidationResponse } from "@/types/IValidationResponse";

export class ValidationService extends BaseService {

  private readonly endpointPath: string = 'validate';

  private readonly options = {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  }

  async validateAsync(
    file: FormData,
    language: string = 'et',
    templateId: string = 'taltech-it',
    thesisType: string = 'bachelor',
    curriculumLanguage: string = 'et',
    foreignTitle: string = ''
  ): Promise<IResponseObject<IValidationResponse>> {
    try {
      file.append('language', language)
      file.append('templateId', templateId)
      file.append('thesisType', thesisType)
      file.append('curriculumLanguage', curriculumLanguage)
      if (foreignTitle) file.append('foreignTitle', foreignTitle)

      const response = await BaseService.axios.post<IValidationResponse>(
        this.endpointPath,
        file,
        this.options
      )

      if (response.status <= 300) {
        return { data: response.data }
      }
      return {
        errors: [(response.status.toString() + ' ' + response.statusText).trim()]
      }
    } catch (error) {
      return BaseService.handleError(error as Error)
    }
  }
}

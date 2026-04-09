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

  async validateAsync(file: FormData): Promise<IResponseObject<IValidationResponse>> {
    try {
      const response = await BaseService.axios.post<IValidationResponse>(
        this.endpointPath,
        file,
        this.options
      );

      console.log('request response', response);

      if (response.status <= 300) {
        return { data: response.data };
      }
      return {
        errors: [(response.status.toString() + ' ' + response.statusText).trim()]
      };
    } catch (error) {
      return BaseService.handleError(error as Error);
    }
  }
}

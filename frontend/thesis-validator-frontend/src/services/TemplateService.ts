import type { IResponseObject } from "@/types/IResponseObject";
import { BaseService } from "./BaseService";
import type { ITemplateDto } from "@/types/ITemplateDto";

export class TemplateService extends BaseService {

  private readonly endpointPath: string = 'templates';

  async getTemplatesAsync(): Promise<IResponseObject<ITemplateDto[]>> {
    try {
      const response = await BaseService.axios.get<ITemplateDto[]>(this.endpointPath);
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

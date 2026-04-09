export interface IResponseObject<TResponse> {
  errors?: string[]
  data?: TResponse
}

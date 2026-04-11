import Axios from 'axios';

export abstract class BaseService {

  protected static axios = Axios.create(
    {
      baseURL: import.meta.env.VITE_API_URL,
      headers: {
        common: {
          accept: 'application/json'
        }
      }
    }
  )

  protected static handleError(error: unknown): { errors: string[] } {
    if (Axios.isAxiosError(error) && error.response?.data?.error) {
      return { errors: [error.response.data.error] }
    }
    return { errors: ['Valideerimine ebaõnnestus. Palun proovi uuesti.'] }
  }
}

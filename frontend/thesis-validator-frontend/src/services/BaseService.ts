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

  protected static handleError(error: Error) {
    console.log('error: ', error.message);
    return {
      errors: [JSON.stringify(error)]
    };
  }
}

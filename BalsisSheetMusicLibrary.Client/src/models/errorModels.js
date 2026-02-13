export class ResponseError extends Error {
  constructor(err, fallbackMessage) {
    if (err.response?.data?.errors) {
      //Assuming .net core SerializableError
      const errorMessages = [];
      for (const key in err.response.data.errors) {
        if (
          Object.prototype.hasOwnProperty.call(err.response.data.errors, key)
        ) {
          errorMessages.push(...err.response.data.errors[key]);
        }
      }
      super(errorMessages.join("\n"));
    } else if (err.response?.data?.title) {
      //Assuming .net core SerializableError with no errors
      super(err.response.data.title);
    } else if (err.response?.data) {
      if (err.response.data.message) {
        //Assuming ExceptionHandlingMiddleware response
        super(err.response.data.message);
      } else {
        // Probably a string message
        super(err.response.data);
      }
    } else {
      super(err.message || fallbackMessage);
    }
    this.name = "ResponseError";
    this.cause = err;
  }
}

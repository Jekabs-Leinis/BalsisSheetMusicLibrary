export class ResponseError extends Error {
  constructor(err, fallbackMessage) {
    if (err.response?.data?.errors) {
      //Assuming .net core style error response
      const errorMessages = [];
      for (const key in err.response.data.errors) {
        if (
          Object.prototype.hasOwnProperty.call(err.response.data.errors, key)
        ) {
          errorMessages.push(...err.response.data.errors[key]);
        }
      }
      super(errorMessages.join("\n"));
    } else if (err.response.data.title) {
      //Assuming .net core style response
      super(err.response.data.title);
    } else {
      super(err.message || fallbackMessage);
    }
    this.name = "ResponseError";
    this.cause = err;
  }
}

export class BaseDto {
  constructor(props = {}) {
    Object.assign(this, props);
  }
  
  static fromResponse(response) {
    return new this(response.data);
  }

  /** @type {Object|Array|String|Number|Boolean} */
  data;
  /** @type {boolean} */
  success;
  /** @type {string} */
  message;
}
export class User {
    constructor(props = {}) {
        Object.assign(this, props)
    }

    /** @type {string} */
    userName;
    /** @type {boolean} */
    isAdmin = false;
}
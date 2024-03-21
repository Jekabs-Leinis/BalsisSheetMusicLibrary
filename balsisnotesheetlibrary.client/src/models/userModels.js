export class User {
    constructor(props = {}) {
        Object.assign(this, props)
    }

    id;
    name;
    isAdmin = false;
}
export class User {
    constructor(props = {}) {
        Object.assign(this, props)
    }
    
    userName;
    isAdmin = false;
}
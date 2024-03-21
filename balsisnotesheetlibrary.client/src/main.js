import './assets/main.css'

// Mandatory import to initialize bs components
// eslint-disable-next-line no-unused-vars
import * as bootstrap from 'bootstrap'

import axios from 'axios'

// Read the CSRF token from the cookie
const csrfToken = document.cookie
    .split('; ')
    .find(row => row.startsWith('XSRF-TOKEN='))

// If the token is found, set it in the axios headers
if (csrfToken) {
    axios.defaults.headers.common['X-XSRF-TOKEN'] = csrfToken.split('=')[1]
}

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import router from '@/router/routes'

import { useUserStore } from '@/stores/userStore'

import vLoading from './directives/vLoading'

import App from './App.vue'

const app = createApp(App)

app.use(createPinia())

const userStore = useUserStore()

router.beforeEach((to) => {
    if (userStore.isLoggedIn || to.name === "login") {
        return;
    }

    return { name: 'login' };
})

app.use(router)

app.directive('loading', vLoading)

app.mount('#app')

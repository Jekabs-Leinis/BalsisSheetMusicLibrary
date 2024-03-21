import './assets/main.css'

// Mandatory import to initialize bs components
// eslint-disable-next-line no-unused-vars
import * as bootstrap from 'bootstrap'

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

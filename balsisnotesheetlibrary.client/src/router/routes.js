import { createRouter, createWebHistory } from 'vue-router';
import LoginView from '@/views/LoginView.vue';
import SheetList from '@/views/SheetList.vue';

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/',
            name: 'SheetList',
            component: SheetList
        },
        {
            path: '/login',
            name: 'Login',
            component: LoginView
        },
        {
            // Catch-all redirect for 404, this has to stay last in array
            path: '/:pathMatch(.*)*',
            name: 'not-found',
            redirect: '/',
        },
    ]
})

export default router

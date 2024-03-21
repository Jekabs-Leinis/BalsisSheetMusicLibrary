import { createRouter, createWebHistory } from 'vue-router';
import LoginView from '@/views/LoginView.vue';
import ChartList from '@/views/ChartList.vue';

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/',
            name: 'chartList',
            component: ChartList
        },
        {
            path: '/login',
            name: 'login',
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

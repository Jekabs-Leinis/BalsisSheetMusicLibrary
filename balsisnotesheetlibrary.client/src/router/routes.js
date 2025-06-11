import { createRouter, createWebHistory } from 'vue-router';

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/',
            name: 'SheetList',
            component: () => import('@/views/SheetList.vue')
        },
        {
            path: '/login',
            name: 'Login',
            component: () => import('@/views/LoginView.vue')
        },
        {
          path: '/admin/sheets',
          name: 'Admin',
          component: () => import('@/views/AdminSheetListView.vue')
        },
        {
            path: '/admin/edit-set-lists',
            name: 'EditSetLists',
            component: () => import('@/views/EditSetListsView.vue')
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

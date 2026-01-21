import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Welcome.vue'),
  },
  {
    path: '/signin-oidc',
    name: 'SigninCallback',
    component: () => import('../views/SigninCallback.vue'),
  },
  {
    path: '/my-account',
    name: 'MyAccount',
    component: () => import('../views/MyAccount.vue'),
  },
  {
    path: '/management',
    name: 'Management',
    component: () => import('../views/Placeholder.vue'),
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

export default router;

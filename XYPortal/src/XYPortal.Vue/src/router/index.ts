import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import Home from '../views/Home.vue';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: Home,
  },
  {
    path: '/signin-oidc',
    name: 'SigninCallback',
    component: () => import('../views/SigninCallback.vue'),
  },
  {
    path: '/nav1',
    name: 'Nav1',
    component: () => import('../views/Placeholder.vue'),
  },
  {
    path: '/nav2',
    name: 'Nav2',
    component: () => import('../views/Placeholder.vue'),
  },
  {
    path: '/nav3',
    name: 'Nav3',
    component: () => import('../views/Placeholder.vue'),
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

export default router;

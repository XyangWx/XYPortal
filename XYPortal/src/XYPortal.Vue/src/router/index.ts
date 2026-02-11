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
    redirect: '/management/identity',
    children: [
      {
        path: 'identity',
        name: 'Identity',
        redirect: '/management/identity/roles',
        children: [
          {
            path: 'roles',
            name: 'Roles',
            component: () => import('../views/Roles.vue'),
          },
          {
            path: 'users',
            name: 'Users',
            component: () => import('../views/Users.vue'),
          },
        ],
      },
      {
        path: 'settings',
        name: 'Settings',
        component: () => import('../views/Settings.vue'),
      },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

export default router;

import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue'),
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
    path: '/linkboard',
    name: 'LinkBoard',
    redirect: '/linkboard/categories',
    children: [
      {
        path: 'categories',
        name: 'LinkBoardCategories',
        component: () => import('../views/LinkBoardCategories.vue'),
      },
      {
        path: 'links',
        name: 'LinkBoardLinks',
        component: () => import('../views/LinkBoardLinks.vue'),
      },
    ],
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
      {
        path: 'openiddict',
        name: 'OpenIddict',
        redirect: '/management/openiddict/applications',
        children: [
          {
            path: 'applications',
            name: 'OpenIddictApplications',
            component: () => import('../views/OpenIddictApplications.vue'),
          },
          {
            path: 'scopes',
            name: 'OpenIddictScopes',
            component: () => import('../views/OpenIddictScopes.vue'),
          },
        ],
      },
      {
        path: 'linkboard-review',
        name: 'LinkBoardReview',
        redirect: '/management/linkboard-review/categories',
        children: [
          {
            path: 'categories',
            name: 'LinkBoardCategoryReview',
            component: () => import('../views/LinkBoardCategoryReview.vue'),
          },
          {
            path: 'links',
            name: 'LinkBoardLinkReview',
            component: () => import('../views/LinkBoardLinkReview.vue'),
          },
        ],
      },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

export default router;

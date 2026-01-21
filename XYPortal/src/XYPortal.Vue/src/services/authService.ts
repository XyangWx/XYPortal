import { UserManager, WebStorageStateStore, User } from 'oidc-client-ts';
import { reactive } from 'vue';

const authConfig = {
  authority: import.meta.env.VITE_AUTH_SERVER_URL,
  client_id: 'XYPortal_Vue',
  redirect_uri: `${window.location.origin}/signin-oidc`,
  post_logout_redirect_uri: `${window.location.origin}/`,
  response_type: 'code',
  scope: 'openid profile roles email phone XYPortal',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
};

const userManager = new UserManager(authConfig);

export const authState = reactive({
  user: null as User | null,
  isAuthenticated: false,
});

export const authService = {
  async getUser(): Promise<User | null> {
    const user = await userManager.getUser();
    authState.user = user;
    authState.isAuthenticated = !!user;
    return user;
  },

  login(): Promise<void> {
    return userManager.signinRedirect();
  },

  async logout(): Promise<void> {
    await userManager.signoutRedirect();
    authState.user = null;
    authState.isAuthenticated = false;
  },

  async signinCallback(): Promise<User> {
    const user = await userManager.signinCallback();
    authState.user = user;
    authState.isAuthenticated = !!user;
    return user;
  },
};


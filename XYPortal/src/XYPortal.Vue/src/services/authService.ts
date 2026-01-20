import { UserManager, WebStorageStateStore, User } from 'oidc-client-ts';

const authConfig = {
  authority: import.meta.env.VITE_AUTH_SERVER_URL,
  client_id: 'XYPortal_Vue',
  redirect_uri: `${window.location.origin}/signin-oidc`,
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  scope: 'openid profile roles email phone XYPortal',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
};

const userManager = new UserManager(authConfig);

export const authService = {
  getUser(): Promise<User | null> {
    return userManager.getUser();
  },

  login(): Promise<void> {
    return userManager.signinRedirect();
  },

  logout(): Promise<void> {
    return userManager.signoutRedirect();
  },

  signinCallback(): Promise<User> {
    return userManager.signinCallback();
  },
};

import { UserManager, WebStorageStateStore, User } from 'oidc-client-ts';
import { reactive } from 'vue';
import axios from 'axios';

const authConfig = {
  authority: import.meta.env.VITE_AUTH_SERVER_URL,
  client_id: 'XYPortal_Vue',
  redirect_uri: `${window.location.origin}/signin-oidc`,
  post_logout_redirect_uri: `${window.location.origin}/`,
  response_type: 'code',
  scope: 'openid profile roles email phone XYPortal',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  automaticSilentRenew: true,         // 启用 access token 自动刷新
  silent_redirect_uri: `${window.location.origin}/silent-renew.html`,
  silentRequestTimeoutInSeconds: 10,  // silent renew 请求超时时间
  monitorAccessTokenExpiry: true,     // 监控 access token 过期
};

const userManager = new UserManager(authConfig);

// 监听 token 刷新事件
userManager.events.addAccessTokenExpiring(() => {
  console.log('[Auth] Access token expiring, renew in progress...');
});

userManager.events.addAccessTokenExpired(() => {
  console.log('[Auth] Access token expired, user will be logged out');
  // token 过期后尝试静默刷新，失败则跳转登录
  userManager.signinSilent().catch(() => {
    userManager.signinRedirect();
  });
});

userManager.events.addUserExpired(() => {
  console.log('[Auth] User session expired');
  userManager.signinRedirect();
});

userManager.events.addUserLoaded((user) => {
  console.log('[Auth] User loaded:', user?.profile?.sub);
});

// 启动静默刷新服务
userManager.startSilentRenew();

const updateAuthState = (user: User | null) => {
  authState.user = user;
  authState.isAuthenticated = !!user;
  if (user) {
    // 初始从 Token 获取一个保底名称
    const fn = (user.profile?.family_name as string) || '';
    const gn = (user.profile?.given_name as string) || '';
    if (fn || gn) {
      authState.displayName = `${fn}${gn}`;
    } else {
      authState.displayName = (user.profile?.name as string) || (user.profile?.preferred_username as string) || 'User';
    }
  } else {
    authState.displayName = '';
  }
};

export const authState = reactive({
  user: null as User | null,
  isAuthenticated: false,
  displayName: '',
});

export const authService = {
  async getUser(): Promise<User | null> {
    const user = await userManager.getUser();
    updateAuthState(user);
    if (user) {
      // 异步获取最新的 Profile 以纠正显示名称
      this.getProfile().catch(() => {});
    }
    return user;
  },

  login(): Promise<void> {
    return userManager.signinRedirect();
  },

  async logout(): Promise<void> {
    await userManager.signoutRedirect();
    updateAuthState(null);
  },

  async signinCallback(): Promise<User | null> {
    const user = await userManager.signinCallback();
    updateAuthState(user ?? null);
    // 登录成功后立即同步 Profile
    await this.getProfile().catch(() => {});
    return user;
  },

  async getProfile(): Promise<any> {
    const user = await userManager.getUser(); // 直接从底层的 userManager 获取，不调用 this.getUser()
    if (!user) return null;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/account/my-profile`, {
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });
    
    // 同步更新显示名称
    const profile = response.data;
    if (profile) {
      authState.displayName = `${profile.surname || ''}${profile.name || ''}`;
    }
    
    return profile;
  },

  async updateProfile(payload: any): Promise<any> {
    const user = await userManager.getUser(); // 直接获取，不调用副作用函数
    if (!user) return null;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.put(`${baseUrl}/api/account/my-profile`, payload, {
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    // 立即更新全局显示名称
    authState.displayName = `${payload.surname || ''}${payload.name || ''}`;
    
    return response.data;
  },

  async changePassword(payload: any): Promise<any> {
    const user = await userManager.getUser(); // 直接获取
    if (!user) return null;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.post(`${baseUrl}/api/account/my-profile/change-password`, payload, {
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });
    return response.data;
  },
};


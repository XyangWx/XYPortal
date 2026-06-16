import { UserManager, WebStorageStateStore, User } from 'oidc-client-ts';
import { reactive } from 'vue';
import axios from 'axios';

const DEBUG = import.meta.env.VITE_AUTH_DEBUG === 'true';

const log = {
  debug: (...args: any[]) => { if (DEBUG) console.log('[Auth:debug]', ...args); },
  info: (...args: any[]) => { if (DEBUG) console.info('[Auth:debug]', ...args); },
  warn: (...args: any[]) => { if (DEBUG) console.warn('[Auth:debug]', ...args); },
  error: (...args: any[]) => { if (DEBUG) console.error('[Auth:debug]', ...args); },
};

const authConfig = {
  authority: import.meta.env.VITE_AUTH_SERVER_URL,
  client_id: 'XYPortal_Vue',
  redirect_uri: `${window.location.origin}/signin-oidc`,
  post_logout_redirect_uri: `${window.location.origin}/`,
  response_type: 'code',
  scope: 'openid profile roles email phone offline_access XYPortal',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  automaticSilentRenew: true,         // 启用 access token 自动刷新
  silent_redirect_uri: `${window.location.origin}/silent-renew.html`,
  silentRequestTimeoutInSeconds: 10,  // silent renew 请求超时时间
  monitorAccessTokenExpiry: true,     // 监控 access token 过期
};

const userManager = new UserManager(authConfig);

if (DEBUG) {
  log.info('Auth service initialized, debug mode enabled');
  log.info('Auth server:', import.meta.env.VITE_AUTH_SERVER_URL);
}

// 监听 token 刷新事件
userManager.events.addAccessTokenExpiring(() => {
  log.warn('[Access Token 即将过期] 开始自动刷新...');
  userManager.signinSilent().then((user) => {
    log.info('[Access Token 刷新成功] expires_at:', user?.expires_at);
  }).catch((err) => {
    log.error('[Access Token 刷新失败]', err);
  });
});

userManager.events.addAccessTokenExpired(() => {
  log.warn('[Access Token 已过期] 尝试静默刷新，失败则清除本地状态');
  userManager.signinSilent().then((user) => {
    log.info('[Access Token 刷新成功]', user?.expires_at);
  }).catch((err) => {
    log.error('[Access Token 刷新失败，清除本地状态]', err);
    userManager.removeUser().then(() => {
      updateAuthState(null);
      log.info('[已清除本地用户状态]');
    });
  });
});

userManager.events.addUserUnloaded(() => {
  log.warn('[User 已从存储移除] 会话过期');
  // 不再跳转到登录页，仅清除本地状态，用户可继续在主页以未登录状态浏览
  updateAuthState(null);
  log.info('[已清除本地用户状态]');
});

userManager.events.addUserLoaded((user) => {
  log.info('[User 加载成功]', {
    sub: user?.profile?.sub,
    preferred_username: user?.profile?.preferred_username,
    expires_at: user?.expires_at,
    scopes: user?.scope,
  });
});

if (typeof (userManager.events as any).addSilentRenewError === 'function') {
  (userManager.events as any).addSilentRenewError((err: any) => {
    log.error('[Silent Renew 错误]', err);
  });
}

// 启动静默刷新服务
if (DEBUG) log.info('[启动 Silent Renew]');
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
    updateAuthState(user ?? null);
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
    log.info('[Logout] 正在注销...');
    userManager.stopSilentRenew();
    await userManager.removeUser();
    updateAuthState(null);
    log.info('[Logout] 本地状态已清除，跳转 IdP 注销');
    await userManager.signoutRedirect();
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


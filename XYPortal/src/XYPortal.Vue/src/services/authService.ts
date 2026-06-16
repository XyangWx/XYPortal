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

/** 取 access_token 前 N 字符用于日志对比(token 太长,只看签名前缀). */
const tokenFingerprint = (token: string | undefined | null): string => {
  if (!token) return '<none>';
  return token.length <= 8 ? token : `${token.slice(0, 8)}…(${token.length})`;
};

/** 秒数转人类可读 "Xm Ys" / "Xs". */
const fmtRemaining = (seconds: number): string => {
  if (seconds < 0) return `${seconds}s (already expired)`;
  const m = Math.floor(seconds / 60);
  const s = Math.floor(seconds % 60);
  return m > 0 ? `${m}m${s}s` : `${s}s`;
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
  const user = userManager.getUser();
  const nowSec = Math.floor(Date.now() / 1000);
  const expiresAt = (user as any)?.expires_at as number | undefined;
  const remaining = expiresAt ? expiresAt - nowSec : undefined;
  log.warn('[Access Token 即将过期] 开始自动刷新...', {
    now: new Date(nowSec * 1000).toISOString(),
    expires_at: expiresAt ? new Date(expiresAt * 1000).toISOString() : '<unknown>',
    remaining: remaining !== undefined ? fmtRemaining(remaining) : '<unknown>',
    current_access_token: tokenFingerprint((user as any)?.access_token),
    silent_redirect_uri: authConfig.silent_redirect_uri,
    silent_timeout_s: authConfig.silentRequestTimeoutInSeconds,
  });
  void runSilentRenew();
});

// 静默续期执行器:统一加计时 + 错误追踪,被 addAccessTokenExpiring / addAccessTokenExpired 共用
async function runSilentRenew(): Promise<void> {
  const startedAt = Date.now();
  log.info('[Silent Renew] 触发 signinSilent(),等待 iframe 完成...', {
    started_at: new Date(startedAt).toISOString(),
  });
  try {
    const user = await userManager.signinSilent();
    const elapsed = Date.now() - startedAt;
    if (!user) {
      // oidc-client-ts 在 iframe 拿不到新 token 时会 resolve(null) 而不是 reject
      log.warn('[Silent Renew] signinSilent() resolve(null),未拿到新 user', {
        elapsed_ms: elapsed,
      });
      return;
    }
    const newFp = tokenFingerprint((user as any).access_token);
    const oldFp = tokenFingerprint((userManager.getUser() as any)?.access_token);
    const expiresAt = (user as any).expires_at as number | undefined;
    const lifetimeSec = expiresAt ? expiresAt - Math.floor(Date.now() / 1000) : undefined;
    log.info('[Silent Renew] signinSilent() 成功', {
      elapsed_ms: elapsed,
      old_token: oldFp,
      new_token: newFp,
      token_changed: oldFp !== newFp,
      expires_at: expiresAt ? new Date(expiresAt * 1000).toISOString() : '<unknown>',
      lifetime: lifetimeSec !== undefined ? fmtRemaining(lifetimeSec) : '<unknown>',
    });
  } catch (err) {
    const elapsed = Date.now() - startedAt;
    log.error('[Silent Renew] signinSilent() rejected', {
      elapsed_ms: elapsed,
      error: err,
    });
    throw err;
  }
}

userManager.events.addAccessTokenExpired(() => {
  const user = userManager.getUser();
  log.warn('[Access Token 已过期] 尝试静默刷新，失败则清除本地状态', {
    current_access_token: tokenFingerprint((user as any)?.access_token),
  });
  runSilentRenew().catch((err) => {
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

// 区分 silent renew 触发的 user loaded vs 登录/回调触发的 user loaded
let _lastUserLoadedToken: string | undefined;
userManager.events.addUserLoaded((user) => {
  const fp = tokenFingerprint((user as any)?.access_token);
  const isSilentRenew = !!_lastUserLoadedToken && _lastUserLoadedToken !== fp;
  _lastUserLoadedToken = fp;
  log.info('[User 加载成功]', {
    trigger: isSilentRenew ? 'silent_renew' : 'login_or_callback',
    sub: user?.profile?.sub,
    preferred_username: user?.profile?.preferred_username,
    expires_at: (user as any)?.expires_at
      ? new Date(((user as any).expires_at as number) * 1000).toISOString()
      : '<unknown>',
    access_token: fp,
    scopes: user?.scope,
  });
});

if (typeof (userManager.events as any).addSilentRenewError === 'function') {
  (userManager.events as any).addSilentRenewError((err: any) => {
    log.error('[Silent Renew 错误]', err);
  });
}

if (typeof (userManager.events as any).addSilentRenewSuccess === 'function') {
  (userManager.events as any).addSilentRenewSuccess((user: any) => {
    log.info('[Silent Renew Success] 续期完成,新 token 指纹:', tokenFingerprint(user?.access_token), {
      expires_at: user?.expires_at
        ? new Date(user.expires_at * 1000).toISOString()
        : '<unknown>',
    });
  });
}

// 启动静默刷新服务
if (DEBUG) {
  log.info('[启动 Silent Renew]', {
    automaticSilentRenew: authConfig.automaticSilentRenew,
    monitorAccessTokenExpiry: authConfig.monitorAccessTokenExpiry,
    silent_redirect_uri: authConfig.silent_redirect_uri,
    silent_timeout_s: authConfig.silentRequestTimeoutInSeconds,
    scope: authConfig.scope,
  });
  log.info('[Silent Renew] startSilentRenew() 已调用,后台定时器运转中');
}
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


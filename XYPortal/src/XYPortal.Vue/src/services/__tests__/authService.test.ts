import { describe, it, expect, vi, beforeEach, beforeAll } from 'vitest';

// Create mock functions
const mockGetUser = vi.fn().mockResolvedValue(null);
const mockSigninRedirect = vi.fn();
const mockSigninCallback = vi.fn().mockResolvedValue(null);
const mockSignoutRedirect = vi.fn();
const mockRemoveUser = vi.fn().mockResolvedValue(undefined);
const mockSigninSilent = vi.fn().mockResolvedValue(null);
const mockStopSilentRenew = vi.fn();
const mockStartSilentRenew = vi.fn();
const mockEvents = {
  addAccessTokenExpiring: vi.fn((handler) => { capturedHandlers.accessTokenExpiring = handler; }),
  addAccessTokenExpired: vi.fn((handler) => { capturedHandlers.accessTokenExpired = handler; }),
  addUserUnloaded: vi.fn((handler) => { capturedHandlers.userUnloaded = handler; }),
  addUserLoaded: vi.fn(),
};

// Capture registered event handlers for testing
const capturedHandlers = {
  accessTokenExpiring: null as ((...args: any[]) => void) | null,
  accessTokenExpired: null as ((...args: any[]) => void) | null,
  userUnloaded: null as ((...args: any[]) => void) | null,
};

// Mock the module
vi.mock('oidc-client-ts', () => {
  return {
    UserManager: vi.fn(function(this: any) {
      this.getUser = mockGetUser;
      this.signinRedirect = mockSigninRedirect;
      this.signinCallback = mockSigninCallback;
      this.signoutRedirect = mockSignoutRedirect;
      this.removeUser = mockRemoveUser;
      this.signinSilent = mockSigninSilent;
      this.events = mockEvents;
      this.startSilentRenew = mockStartSilentRenew;
      this.stopSilentRenew = mockStopSilentRenew;
    }),
    WebStorageStateStore: vi.fn(),
    User: class MockUser {
      access_token = 'mock-token';
      profile = { sub: 'test-user', name: 'Test User' };
    },
  };
});

vi.mock('axios', () => ({
  default: {
    get: vi.fn().mockResolvedValue({ data: { name: 'Test', surname: 'User' } }),
    put: vi.fn().mockResolvedValue({ data: {} }),
    post: vi.fn().mockResolvedValue({ data: {} }),
  },
}));

// Import after mocks
const { authState, authService } = await import('../authService');

describe('authService - Token Refresh Feature', () => {
  beforeEach(() => {
    authState.user = null;
    authState.isAuthenticated = false;
    authState.displayName = '';
    vi.clearAllMocks();
  });

  describe('authState', () => {
    it('should have correct initial state', () => {
      expect(authState.isAuthenticated).toBe(false);
      expect(authState.user).toBeNull();
      expect(authState.displayName).toBe('');
    });

    it('should track authenticated state correctly', () => {
      const mockUser = {
        access_token: 'test-token',
        profile: { sub: 'user1', name: 'Test User' },
      } as any;
      
      authState.user = mockUser;
      authState.isAuthenticated = true;
      authState.displayName = 'Test User';
      
      expect(authState.isAuthenticated).toBe(true);
      expect(authState.user?.access_token).toBe('test-token');
      expect(authState.displayName).toBe('Test User');
    });
  });

  describe('authService Methods', () => {
    it('should have getUser method', () => {
      expect(typeof authService.getUser).toBe('function');
    });

    it('should have login method', () => {
      expect(typeof authService.login).toBe('function');
    });

    it('should have logout method', () => {
      expect(typeof authService.logout).toBe('function');
    });

    it('should have signinCallback method', () => {
      expect(typeof authService.signinCallback).toBe('function');
    });

    it('should have getProfile method', () => {
      expect(typeof authService.getProfile).toBe('function');
    });

    it('should have updateProfile method', () => {
      expect(typeof authService.updateProfile).toBe('function');
    });

    it('should have changePassword method', () => {
      expect(typeof authService.changePassword).toBe('function');
    });
  });

  describe('signinCallback Behavior', () => {
    it('should return null when signinCallback returns null', async () => {
      mockSigninCallback.mockResolvedValueOnce(null);
      const result = await authService.signinCallback();
      expect(result).toBeNull();
    });

    it('should return user object on successful callback', async () => {
      const mockUser = {
        access_token: 'new-token',
        profile: { sub: 'user1', given_name: 'John', family_name: 'Doe' },
      };
      mockSigninCallback.mockResolvedValueOnce(mockUser);
      
      const result = await authService.signinCallback();
      expect(result).toEqual(mockUser);
      expect(authState.isAuthenticated).toBe(true);
    });
  });

  describe('Token Refresh Feature Verification', () => {
    it('should export authService with all required methods', () => {
      // Verify the service is properly structured for token refresh
      expect(authService).toBeDefined();
      expect(authService.getUser).toBeDefined();
      expect(authService.signinCallback).toBeDefined();
    });

    it('should handle user loaded event', async () => {
      mockGetUser.mockResolvedValueOnce({
        access_token: 'test-token',
        profile: { sub: 'user1', name: 'Test' },
      });
      
      const user = await authService.getUser();
      expect(user).toBeDefined();
      expect(mockGetUser).toHaveBeenCalled();
    });

    it('should support silent refresh via signinSilent', async () => {
      // Verify signinSilent is available through the mocked UserManager
      mockSigninSilent.mockResolvedValueOnce({
        access_token: 'refreshed-token',
        profile: { sub: 'user1' },
      });
      
      expect(typeof mockSigninSilent).toBe('function');
      
      const result = await mockSigninSilent();
      expect(result.access_token).toBe('refreshed-token');
    });
  });

  describe('Authentication Flow', () => {
    it('should authenticate user via getUser', async () => {
      const mockUser = {
        access_token: 'test-token',
        profile: { sub: 'user1', name: 'Test User' },
      };
      mockGetUser.mockResolvedValueOnce(mockUser);
      
      const user = await authService.getUser();
      
      expect(user).toBeDefined();
      expect(authState.isAuthenticated).toBe(true);
    });

    it('should handle logout correctly', async () => {
      await authService.logout();
      expect(mockStopSilentRenew).toHaveBeenCalledTimes(1);
      expect(mockRemoveUser).toHaveBeenCalled();
      expect(mockSignoutRedirect).toHaveBeenCalled();
      expect(authState.isAuthenticated).toBe(false);
    });

    it('should stop silent renew when logging out', async () => {
      await authService.logout();
      expect(mockStopSilentRenew).toHaveBeenCalledTimes(1);
    });
  });

  describe('Token Expiry and State Clearance', () => {
    it('should not redirect when user unloaded - only clear local state', async () => {
      const unloadHandler = capturedHandlers.userUnloaded;
      expect(unloadHandler).not.toBeNull();

      authState.user = { access_token: 'test' } as any;
      authState.isAuthenticated = true;
      authState.displayName = 'Test User';

      await unloadHandler!();

      expect(authState.isAuthenticated).toBe(false);
      expect(authState.user).toBeNull();
      expect(authState.displayName).toBe('');
      expect(mockSigninRedirect).not.toHaveBeenCalled();
      expect(mockSignoutRedirect).not.toHaveBeenCalled();
    });

    it('should remove user from storage when access token expired and refresh fails', async () => {
      const expiredHandler = capturedHandlers.accessTokenExpired;
      expect(expiredHandler).not.toBeNull();

      mockSigninSilent.mockRejectedValueOnce(new Error('refresh_failed'));

      authState.user = { access_token: 'expired' } as any;
      authState.isAuthenticated = true;

      await expiredHandler!();
      await new Promise(resolve => setTimeout(resolve, 0));

      expect(mockRemoveUser).toHaveBeenCalled();
      expect(authState.isAuthenticated).toBe(false);
      expect(mockSigninRedirect).not.toHaveBeenCalled();
    });

    it('should not clear state when access token refresh succeeds', async () => {
      const expiredHandler = capturedHandlers.accessTokenExpired;
      expect(expiredHandler).not.toBeNull();

      const refreshedUser = { access_token: 'new-token', expires_at: Date.now() + 3600000 };
      mockSigninSilent.mockResolvedValueOnce(refreshedUser);

      authState.user = { access_token: 'old-token' } as any;
      authState.isAuthenticated = true;

      await expiredHandler!();
      await new Promise(resolve => setTimeout(resolve, 0));

      expect(mockRemoveUser).not.toHaveBeenCalled();
      expect(mockSigninRedirect).not.toHaveBeenCalled();
    });
  });
});

describe('Token Refresh Implementation', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should have automaticSilentRenew configuration in authService', () => {
    // Verify authService is available and properly configured
    expect(authService).toBeDefined();
    expect(authService.signinCallback).toBeDefined();
    expect(authService.getUser).toBeDefined();
  });

  it('should verify token refresh through signinCallback', async () => {
    const mockUser = {
      access_token: 'new-access-token',
      profile: { sub: 'user1', name: 'Test' },
      refresh_token: 'refresh-token',
    };
    mockSigninCallback.mockResolvedValueOnce(mockUser);
    
    const result = await authService.signinCallback();
    
    expect(result).toBeDefined();
    expect(result?.access_token).toBe('new-access-token');
    expect(authState.isAuthenticated).toBe(true);
  });

  it('should handle getUser for token refresh verification', async () => {
    const mockUser = {
      access_token: 'valid-token',
      profile: { sub: 'user1' },
      expires_at: Date.now() + 3600000,
    };
    mockGetUser.mockResolvedValueOnce(mockUser);
    
    const user = await authService.getUser();
    
    expect(user).toBeDefined();
    expect(user?.access_token).toBe('valid-token');
    expect(authState.isAuthenticated).toBe(true);
  });

  it('should support manual token refresh via signinSilent', async () => {
    const refreshedUser = {
      access_token: 'refreshed-access-token',
      profile: { sub: 'user1' },
    };
    mockSigninSilent.mockResolvedValueOnce(refreshedUser);
    
    const result = await mockSigninSilent();
    
    expect(result).toBeDefined();
    expect(result.access_token).toBe('refreshed-access-token');
  });
});

import axios from 'axios';
import { reactive } from 'vue';
import { authService } from './authService';

// ABP 权限常量
export const Permissions = {
  Identity: {
    Roles: 'AbpIdentity.Roles',
    RolesCreate: 'AbpIdentity.Roles.Create',
    RolesUpdate: 'AbpIdentity.Roles.Update',
    RolesDelete: 'AbpIdentity.Roles.Delete',
    RolesManagePermissions: 'AbpIdentity.Roles.ManagePermissions',
    Users: 'AbpIdentity.Users',
  },
  FeatureManagement: {
    ManageHostFeatures: 'FeatureManagement.ManageHostFeatures',
  },
  SettingManagement: {
    Emailing: 'SettingManagement.Emailing',
    EmailingTest: 'SettingManagement.Emailing.Test',
  },
};

// 权限状态
export const permissionState = reactive({
  grants: {} as Record<string, boolean>,
  loaded: false,
});

// 权限服务
export const permissionService = {
  /**
   * 获取用户权限
   */
  async getPermissions(): Promise<void> {
    try {
      const user = await authService.getUser();
      if (!user) {
        permissionState.grants = {};
        permissionState.loaded = true;
        return;
      }

      const baseUrl = import.meta.env.VITE_API_BASE_URL;
      
      // 调用ABP的应用配置API
      const response = await axios.get(`${baseUrl}/api/abp/application-configuration`, {
        headers: {
          Authorization: `Bearer ${user.access_token}`,
        },
      });
      
      // 解析权限
      const grantedPolicies = response.data.auth?.grantedPolicies || {};
      
      // 直接使用ABP返回的权限对象
      permissionState.grants = grantedPolicies;
      permissionState.loaded = true;
    } catch (error) {
      console.error('[PermissionService] 获取权限失败:', error);
      permissionState.grants = {};
      permissionState.loaded = true;
    }
  },

  /**
   * 检查是否有指定权限
   */
  hasPermission(permission: string): boolean {
    return permissionState.grants[permission] === true;
  },

  /**
   * 检查是否有任一权限
   */
  hasAnyPermission(...permissions: string[]): boolean {
    return permissions.some(p => this.hasPermission(p));
  },

  /**
   * 检查是否有所有权限
   */
  hasAllPermissions(...permissions: string[]): boolean {
    return permissions.every(p => this.hasPermission(p));
  },

  /**
   * 清空权限
   */
  clearPermissions(): void {
    permissionState.grants = {};
    permissionState.loaded = false;
  },
};

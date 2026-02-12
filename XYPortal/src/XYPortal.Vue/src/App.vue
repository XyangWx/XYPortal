<template>
  <a-layout style="min-height: 100vh">
    <a-layout-sider v-model:collapsed="collapsed" :trigger="null" collapsible>
      <div class="logo" />
      <a-menu v-model:selectedKeys="selectedKeys" theme="dark" mode="inline">
        <a-menu-item key="Home" @click="$router.push('/')">
          <user-outlined />
          <span>主页</span>
        </a-menu-item>
        <a-sub-menu v-if="hasManagementPermission" key="Management">
          <template #title>
            <span>
              <setting-outlined />
              <span>管理</span>
            </span>
          </template>
          <a-sub-menu v-if="hasIdentityPermission" key="Identity">
            <template #title>身份认证管理</template>
            <a-menu-item v-if="hasRolesPermission" key="Roles" @click="$router.push('/management/identity/roles')">角色</a-menu-item>
            <a-menu-item v-if="hasUsersPermission" key="Users" @click="$router.push('/management/identity/users')">用户</a-menu-item>
          </a-sub-menu>
          <a-sub-menu v-if="hasOpenIddictPermission" key="OpenIddict">
            <template #title>OpenIdDict管理</template>
            <a-menu-item key="OpenIddictApplications" @click="$router.push('/management/openiddict/applications')">应用管理</a-menu-item>
            <a-menu-item key="OpenIddictScopes" @click="$router.push('/management/openiddict/scopes')">作用域管理</a-menu-item>
          </a-sub-menu>
          <a-menu-item v-if="hasSettingsPermission" key="Settings" @click="$router.push('/management/settings')">设置</a-menu-item>
        </a-sub-menu>
      </a-menu>
    </a-layout-sider>
    <a-layout>
      <a-layout-header style="background: #fff; padding: 0; display: flex; justify-content: space-between; align-items: center">
        <div style="display: flex; align-items: center">
          <menu-unfold-outlined
            v-if="collapsed"
            class="trigger"
            @click="() => (collapsed = !collapsed)"
          />
          <menu-fold-outlined
            v-else
            class="trigger"
            @click="() => (collapsed = !collapsed)"
          />
          <a-breadcrumb style="margin-left: 16px">
            <template v-if="breadcrumbItems.length > 0">
              <a-breadcrumb-item v-for="(item, index) in breadcrumbItems" :key="index">
                {{ item }}
              </a-breadcrumb-item>
            </template>
          </a-breadcrumb>
        </div>
        <div style="padding-right: 24px">
          <a-dropdown v-if="authState.isAuthenticated">
            <a class="ant-dropdown-link" @click.prevent>
              <a-space>
                <a-avatar size="small">
                  <template #icon><UserOutlined /></template>
                </a-avatar>
                {{ authState.displayName }}
                <down-outlined />
              </a-space>
            </a>
            <template #overlay>
              <a-menu>
                <a-menu-item key="account" @click="$router.push('/my-account')">
                  我的账户
                </a-menu-item>
                <a-menu-divider />
                <a-menu-item key="logout" @click="handleLogout">
                  退出登录
                </a-menu-item>
              </a-menu>
            </template>
          </a-dropdown>
          <a-button v-else type="primary" @click="handleLogin">登录</a-button>
        </div>
      </a-layout-header>
      <a-layout-content
        :style="{ margin: '24px 16px', padding: '24px', background: '#fff', minHeight: '280px' }"
      >
        <router-view></router-view>
      </a-layout-content>
    </a-layout>
  </a-layout>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch, computed } from 'vue';
import { useRoute } from 'vue-router';
import {
  UserOutlined,
  SettingOutlined,
  MenuUnfoldOutlined,
  MenuFoldOutlined,
  DownOutlined,
} from '@ant-design/icons-vue';
import { authService, authState } from './services/authService';
import { permissionService, Permissions } from './services/permissionService';

const route = useRoute();
const selectedKeys = ref<string[]>(['Home']);
const collapsed = ref<boolean>(false);

// 路由名称到中文的映射
const routeNameMap: Record<string, string> = {
  Home: '主页',
  MyAccount: '我的账户',
  Management: '管理',
  Identity: '身份认证管理',
  Roles: '角色',
  Users: '用户',
  Settings: '设置',
  OpenIddict: 'OpenIdDict管理',
  OpenIddictApplications: '应用管理',
  OpenIddictScopes: '作用域管理',
};

// 计算面包屑
const breadcrumbItems = computed(() => {
  const name = route.name as string;
  
  // Welcome 页面不显示面包屑
  if (name === 'Home') {
    return [];
  }
  
  const items: string[] = [];
  
  // 根据路由层级构建面包屑
  if (name === 'MyAccount') {
    items.push('我的账户');
  } else if (name === 'Roles' || name === 'Users') {
    items.push('管理');
    items.push('身份认证管理');
    items.push(routeNameMap[name]);
  } else if (name === 'Settings') {
    items.push('管理');
    items.push('设置');
  } else if (name === 'OpenIddictApplications') {
    items.push('管理');
    items.push('OpenIdDict管理');
    items.push('应用管理');
  } else if (name === 'OpenIddictScopes') {
    items.push('管理');
    items.push('OpenIdDict管理');
    items.push('作用域管理');
  }
  
  return items;
});

// 权限检查计算属性
// 检查是否有身份认证管理权限（角色或用户任一权限）
const hasIdentityPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasAnyPermission(
    Permissions.Identity.Roles,
    Permissions.Identity.Users
  );
});

// 检查是否有角色权限
const hasRolesPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.Identity.Roles);
});

// 检查是否有用户权限
const hasUsersPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.Identity.Users);
});

// 检查是否有设置权限（功能管理或邮件任一权限）
const hasSettingsPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasAnyPermission(
    Permissions.FeatureManagement.ManageHostFeatures,
    Permissions.SettingManagement.Emailing
  );
});

// 检查是否有功能管理权限
const hasFeatureManagementPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.FeatureManagement.ManageHostFeatures);
});

// 检查是否有邮件设置权限
const hasEmailingPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.SettingManagement.Emailing);
});

// 检查是否有OpenIdDict管理权限
const hasOpenIddictPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasAnyPermission(
    Permissions.OpenIdDict.ApplicationManager,
    Permissions.OpenIdDict.ScopeManager
  );
});

// 检查是否有管理菜单权限（身份认证或设置或OpenIdDict任一权限）
const hasManagementPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return hasIdentityPermission.value || hasSettingsPermission.value || hasOpenIddictPermission.value;
});

// 监听路由变化更新菜单选中状态
watch(
  () => route.name,
  (name) => {
    if (name) {
      selectedKeys.value = [name.toString()];
    }
  }
);

onMounted(async () => {
  await authService.getUser();
  // 获取用户权限
  if (authState.isAuthenticated) {
    await permissionService.getPermissions();
  }
});

const handleLogin = async () => {
  try {
    await authService.login();
  } catch (error) {
    console.error('Login failed:', error);
  }
};

const handleLogout = async () => {
  try {
    // 清除权限
    permissionService.clearPermissions();
    await authService.logout();
  } catch (error) {
    console.error('Logout failed:', error);
  }
};
</script>

<style scoped>
.trigger {
  font-size: 18px;
  line-height: 64px;
  padding: 0 24px;
  cursor: pointer;
  transition: color 0.3s;
}

.trigger:hover {
  color: #1890ff;
}

.logo {
  height: 32px;
  background: rgba(255, 255, 255, 0.3);
  margin: 16px;
}
</style>

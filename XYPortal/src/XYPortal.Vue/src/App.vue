<template>
  <a-layout style="min-height: 100vh">
    <a-layout-sider v-model:collapsed="collapsed" :trigger="null" collapsible>
      <div class="logo">
        <!-- Half sun (expanded) -->
        <svg v-if="!collapsed" viewBox="0 0 120 32" xmlns="http://www.w3.org/2000/svg">
          <defs>
            <linearGradient id="sunGradientHalf" x1="0%" y1="100%" x2="0%" y2="0%">
              <stop offset="0%" style="stop-color:#FF6B35;stop-opacity:1" />
              <stop offset="50%" style="stop-color:#FFA500;stop-opacity:1" />
              <stop offset="100%" style="stop-color:#FFD700;stop-opacity:1" />
            </linearGradient>
            <linearGradient id="skyGradientHalf" x1="0%" y1="0%" x2="0%" y2="100%">
              <stop offset="0%" style="stop-color:#87CEEB;stop-opacity:1" />
              <stop offset="100%" style="stop-color:#FFE4B5;stop-opacity:0.8" />
            </linearGradient>
            <filter id="sunGlow">
              <feGaussianBlur stdDeviation="2" result="coloredBlur"/>
              <feMerge>
                <feMergeNode in="coloredBlur"/>
                <feMergeNode in="SourceGraphic"/>
              </feMerge>
            </filter>
          </defs>
          <!-- Sky background -->
          <rect x="2" y="2" width="116" height="28" rx="6" fill="url(#skyGradientHalf)"/>
          <!-- Sun body (half, rising) -->
          <circle cx="60" cy="28" r="12" fill="url(#sunGradientHalf)" filter="url(#sunGlow)"/>
          <!-- Sun rays -->
          <line x1="60" y1="10" x2="60" y2="6" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="75" y1="14" x2="78" y2="10" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="45" y1="14" x2="42" y2="10" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="72" y1="22" x2="76" y2="20" stroke="#FFA500" stroke-width="1.5" stroke-linecap="round"/>
          <line x1="48" y1="22" x2="44" y2="20" stroke="#FFA500" stroke-width="1.5" stroke-linecap="round"/>
          <!-- Horizon line -->
          <path d="M 2 28 Q 30 26 60 28 Q 90 30 118 28" fill="none" stroke="#8B4513" stroke-width="1" opacity="0.5"/>
        </svg>
        <!-- Full sun (collapsed) -->
        <svg v-else viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg">
          <defs>
            <radialGradient id="sunGradientFull" cx="50%" cy="50%" r="50%">
              <stop offset="0%" style="stop-color:#FFFF00;stop-opacity:1" />
              <stop offset="70%" style="stop-color:#FFA500;stop-opacity:1" />
              <stop offset="100%" style="stop-color:#FF6B35;stop-opacity:1" />
            </radialGradient>
            <filter id="sunGlowFull">
              <feGaussianBlur stdDeviation="1.5" result="coloredBlur"/>
              <feMerge>
                <feMergeNode in="coloredBlur"/>
                <feMergeNode in="SourceGraphic"/>
              </feMerge>
            </filter>
          </defs>
          <!-- Sun body -->
          <circle cx="16" cy="16" r="10" fill="url(#sunGradientFull)" filter="url(#sunGlowFull)"/>
          <!-- Sun rays -->
          <line x1="16" y1="2" x2="16" y2="5" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="16" y1="27" x2="16" y2="30" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="2" y1="16" x2="5" y2="16" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="27" y1="16" x2="30" y2="16" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="6" y1="6" x2="8" y2="8" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="24" y1="6" x2="22" y2="8" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="6" y1="26" x2="8" y2="24" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <line x1="24" y1="26" x2="22" y2="24" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
          <!-- Happy face -->
          <circle cx="13" cy="14" r="1.5" fill="#8B4513"/>
          <circle cx="19" cy="14" r="1.5" fill="#8B4513"/>
          <path d="M 12 18 Q 16 21 20 18" fill="none" stroke="#8B4513" stroke-width="1.5" stroke-linecap="round"/>
        </svg>
      </div>
      <a-menu v-model:selectedKeys="selectedKeys" theme="dark" mode="inline">
        <a-menu-item key="Home" @click="$router.push('/')">
          <user-outlined />
          <span>主页</span>
        </a-menu-item>
        <a-sub-menu v-if="hasLinkBoardPermission" key="LinkBoard">
          <template #title>
            <span>
              <link-outlined />
              <span>链接板</span>
            </span>
          </template>
          <a-menu-item v-if="hasLinkCategoryManagerPermission" key="LinkBoardCategories" @click="$router.push('/linkboard/categories')">分类管理</a-menu-item>
          <a-menu-item v-if="hasLinkManagerPermission" key="LinkBoardLinks" @click="$router.push('/linkboard/links')">链接管理</a-menu-item>
        </a-sub-menu>
        <a-menu-item v-if="hasPasswordBookPermission" key="PasswordBook" @click="$router.push('/password-book')">
          <LockOutlined />
          <span>密码本</span>
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
          <a-sub-menu v-if="hasLinkBoardAdminPermission" key="LinkBoardReview">
            <template #title>链接板审核</template>
            <a-menu-item v-if="hasLinkCategoryReviewPermission" key="LinkBoardCategoryReview" @click="$router.push('/management/linkboard-review/categories')">分类审核</a-menu-item>
            <a-menu-item v-if="hasLinkReviewPermission" key="LinkBoardLinkReview" @click="$router.push('/management/linkboard-review/links')">链接审核</a-menu-item>
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
  LinkOutlined,
  LockOutlined,
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
  LinkBoard: '链接板',
  LinkBoardCategories: '分类管理',
  LinkBoardLinks: '链接管理',
  LinkBoardReview: '链接板审核',
  LinkBoardCategoryReview: '分类审核',
  LinkBoardLinkReview: '链接审核',
  PasswordBook: '密码本',
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
  } else if (name === 'LinkBoardCategories') {
    items.push('链接板');
    items.push('分类管理');
  } else if (name === 'LinkBoardLinks') {
    items.push('链接板');
    items.push('链接管理');
  } else if (name === 'LinkBoardCategoryReview') {
    items.push('管理');
    items.push('链接板审核');
    items.push('分类审核');
  } else if (name === 'LinkBoardLinkReview') {
    items.push('管理');
    items.push('链接板审核');
    items.push('链接审核');
  } else if (name === 'PasswordBook') {
    items.push('密码本');
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

// 检查是否有管理菜单权限（身份认证或设置或OpenIdDict或LinkBoard审核任一权限）
const hasManagementPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return hasIdentityPermission.value || hasSettingsPermission.value || hasOpenIddictPermission.value || hasLinkBoardAdminPermission.value;
});

// LinkBoard 权限检查
const hasLinkBoardPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasAnyPermission(
    Permissions.LinkBoard.User,
    Permissions.LinkBoard.Admin
  );
});

const hasLinkCategoryManagerPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.LinkBoard.LinkCategoryManager);
});

const hasLinkManagerPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.LinkBoard.LinkManager);
});

const hasLinkBoardAdminPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.LinkBoard.Admin);
});

const hasLinkCategoryReviewPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.LinkBoard.LinkCategoryReview);
});

const hasLinkReviewPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.LinkBoard.LinkReview);
});

// PasswordBook 权限检查
const hasPasswordBookPermission = computed(() => {
  if (!authState.isAuthenticated) return false;
  return permissionService.hasPermission(Permissions.PasswordBook.User);
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
  margin: 16px;
  border-radius: 8px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.logo svg {
  width: 100%;
  height: 100%;
  display: block;
  border-radius: 6px;
}
</style>

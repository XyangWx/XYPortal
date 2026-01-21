<template>
  <a-layout style="min-height: 100vh">
    <a-layout-sider v-model:collapsed="collapsed" :trigger="null" collapsible>
      <div class="logo" />
      <a-menu v-model:selectedKeys="selectedKeys" theme="dark" mode="inline">
        <a-menu-item key="Home" @click="$router.push('/')">
          <user-outlined />
          <span>Dashboard</span>
        </a-menu-item>
        <a-menu-item key="Nav1" @click="$router.push('/nav1')">
          <video-camera-outlined />
          <span>nav 1</span>
        </a-menu-item>
        <a-menu-item key="Nav2" @click="$router.push('/nav2')">
          <upload-outlined />
          <span>nav 2</span>
        </a-menu-item>
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
            <a-breadcrumb-item>Home</a-breadcrumb-item>
            <a-breadcrumb-item>{{ $route.name }}</a-breadcrumb-item>
          </a-breadcrumb>
        </div>
        <div style="padding-right: 24px">
          <a-dropdown v-if="authState.isAuthenticated">
            <a class="ant-dropdown-link" @click.prevent>
              <a-space>
                <a-avatar size="small">
                  <template #icon><UserOutlined /></template>
                </a-avatar>
                {{ authState.user?.profile?.name || authState.user?.profile?.preferred_username || 'User' }}
                <down-outlined />
              </a-space>
            </a>
            <template #overlay>
              <a-menu>
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
import { ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import {
  UserOutlined,
  VideoCameraOutlined,
  UploadOutlined,
  MenuUnfoldOutlined,
  MenuFoldOutlined,
  DownOutlined,
} from '@ant-design/icons-vue';
import { authService, authState } from './services/authService';

const route = useRoute();
const selectedKeys = ref<string[]>(['Home']);
const collapsed = ref<boolean>(false);

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

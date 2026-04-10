<template>
  <div class="welcome-widget">
    <a-card size="small">
      <template #title>
        <span>👋 欢迎</span>
      </template>
      <div class="welcome-content">
        <h5 v-if="authState.isAuthenticated" class="mb-0">
          欢迎！{{ authState.displayName }}
        </h5>
        <h5 v-else class="mb-0">
          欢迎！
        </h5>
      </div>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
import { authState } from '../services/authService';
</script>

<style scoped>
.welcome-widget {
  height: 100%;
}

.welcome-widget :deep(.ant-card) {
  height: 100%;
}

.welcome-content {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100px;
}

.welcome-content h5 {
  font-weight: 500;
  color: #333;
  font-size: 16px;
}
</style>

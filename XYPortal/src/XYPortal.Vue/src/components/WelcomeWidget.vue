<template>
  <div class="welcome-widget">
    <a-card size="small">
      <template #title>
        <span>👋 欢迎</span>
      </template>
      <div class="welcome-content">
        <div class="welcome-avatar">
          <img v-if="authState.isAuthenticated" src="data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxMDAgMTAwIj48Y2lyY2xlIGN4PSI1MCIgY3k9IjUwIiByPSI0NSIgZmlsbD0iIzY5NmY4ZiIvPjxjaXJjbGUgY3g9IjUwIiBjeT0iMzUiIHI9IjE1IiBmaWxsPSIjZmZmZiIvPjxwYXRoIGQ9Ik0zMCA2NUw1MCA5NUw3MCA2NSIgZmlsbT0iIzY5NmY4ZiIvPjwvc3ZnPg==" alt="avatar" />
          <img v-else src="data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxMDAgMTAwIj48cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2Y1ZjVmNSIvPjxjaXJjbGUgY3g9IjUwIiBjeT0iMzUiIHI9IjE1IiBmaWxsPSIjZmZmZiIvPjxwYXRoIGQ9Ik0zMCA2NUw1MCA5NUw3MCA2NSIgZmlsbD0iI2Y1ZjVmNSIvPjx0ZXh0IHg9IjUwIiB5PSI3NSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZm9udC1mYW1pbHk9IkF2ZW5pciIgZm9udC1zaXplPSIyMCIgZmlsbD0iI2ZmZiIgZHk9Ii4zZW0iPk88L3RleHQ+PC9zdmc+" alt="guest" />
        </div>
        <div class="welcome-text">
          <h5 v-if="authState.isAuthenticated" class="mb-0">
            欢迎！{{ authState.displayName }}
          </h5>
          <h5 v-else class="mb-0">
            欢迎！
          </h5>
          <p class="welcome-subtitle mb-0">XYPortal 管理门户</p>
        </div>
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
  gap: 16px;
  padding: 8px 0;
}

.welcome-avatar {
  flex-shrink: 0;
}

.welcome-avatar img {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  object-fit: cover;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.welcome-text {
  flex: 1;
  min-width: 0;
}

.welcome-text h5 {
  font-weight: 600;
  color: #333;
  font-size: 16px;
}

.welcome-subtitle {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}
</style>

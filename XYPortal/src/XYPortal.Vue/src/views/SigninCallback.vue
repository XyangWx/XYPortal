<template>
  <div class="callback">
    <a-spin tip="正在处理登录请求..." size="large">
      <div style="padding: 50px; text-align: center">
        正在跳转，请稍候...
      </div>
    </a-spin>
  </div>
</template>

<script lang="ts" setup>
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { authService } from '../services/authService';

const router = useRouter();

onMounted(async () => {
  try {
    await authService.signinCallback();
    router.push('/');
  } catch (error) {
    console.error('Login callback failed:', error);
    router.push('/');
  }
});
</script>

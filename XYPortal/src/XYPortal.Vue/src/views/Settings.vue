<template>
  <div class="settings">
    <a-card :bordered="false">
      <a-tabs v-model:activeKey="activeKey">
        <a-tab-pane key="features" tab="功能管理">
          <div class="tab-content">
            <a-typography-title :level="4">功能管理</a-typography-title>
            <a-typography-paragraph>
              在这里配置系统的功能开关和相关设置。
            </a-typography-paragraph>
            <!-- 功能管理内容区域 -->
          </div>
        </a-tab-pane>
        <a-tab-pane key="email" tab="邮件">
          <div class="tab-content">
            <a-typography-title :level="4">邮件设置</a-typography-title>
            <a-typography-paragraph>
              配置系统邮件发送相关参数。
            </a-typography-paragraph>
            <!-- 邮件设置内容区域 -->
          </div>
        </a-tab-pane>
      </a-tabs>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
import { ref } from 'vue';

const activeKey = ref('features');
</script>

<style scoped>
.settings {
  width: 100%;
}

.tab-content {
  padding: 16px 0;
}
</style>

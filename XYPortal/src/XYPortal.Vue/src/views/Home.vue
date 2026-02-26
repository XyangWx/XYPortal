<template>
  <div class="home">
    <a-row :gutter="16">
      <a-col :xs="24" :xl="8">
        <LinkBoardWidget />
      </a-col>
      <a-col :xs="24" :xl="16">
        <a-card>
          <h1>Welcome to XYPortal</h1>
          <p>This is the main dashboard of the Vue 3 application.</p>
        </a-card>
      </a-col>
    </a-row>
  </div>
</template>

<script lang="ts" setup>
import LinkBoardWidget from '../components/LinkBoardWidget.vue';
</script>

<template>
  <div class="home">
    <a-row :gutter="16">
      <a-col :xs="24" :xl="8">
        <WelcomeWidget />
      </a-col>
      <a-col :xs="24" :xl="8">
        <LinkBoardWidget />
      </a-col>
      <a-col :xs="24" :xl="8">
        <RandomStringWidget />
      </a-col>
    </a-row>
  </div>
</template>

<script lang="ts" setup>
import WelcomeWidget from '../components/WelcomeWidget.vue';
import LinkBoardWidget from '../components/LinkBoardWidget.vue';
import RandomStringWidget from '../components/RandomStringWidget.vue';
</script>

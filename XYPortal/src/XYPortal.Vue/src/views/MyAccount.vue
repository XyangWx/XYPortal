<template>
  <div class="my-account">
    <a-card :bordered="false">
      <a-tabs v-model:activeKey="activeKey">
        <a-tab-pane key="profile" tab="个人信息">
          <a-form layout="vertical" :model="profileForm">
            <a-form-item label="用户 Id">
              <a-input v-model:value="profileForm.userId" disabled />
            </a-form-item>
            <a-form-item label="姓">
              <a-input v-model:value="profileForm.surname" />
            </a-form-item>
            <a-form-item label="名">
              <a-input v-model:value="profileForm.name" />
            </a-form-item>
            <a-form-item label="电子邮件">
              <a-input v-model:value="profileForm.email" />
            </a-form-item>
            <a-form-item label="电话号码">
              <a-input v-model:value="profileForm.phoneNumber" />
            </a-form-item>
            <a-form-item>
              <a-button type="primary" @click="saveProfile">保存</a-button>
            </a-form-item>
          </a-form>
        </a-tab-pane>
        <a-tab-pane key="password" tab="更改密码">
          <a-form layout="vertical" :model="passwordForm">
            <a-form-item label="原密码">
              <a-input-password v-model:value="passwordForm.currentPassword" />
            </a-form-item>
            <a-form-item label="新密码">
              <a-input-password v-model:value="passwordForm.newPassword" />
            </a-form-item>
            <a-form-item label="确认新密码">
              <a-input-password v-model:value="passwordForm.confirmNewPassword" />
            </a-form-item>
            <a-form-item>
              <a-button type="primary" @click="changePassword">修改密码</a-button>
            </a-form-item>
          </a-form>
        </a-tab-pane>
      </a-tabs>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from 'vue';
import { message } from 'ant-design-vue';
import { authService } from '../services/authService';

const activeKey = ref('profile');

const profileForm = reactive({
  userId: '',
  surname: '',
  name: '',
  email: '',
  phoneNumber: '',
  concurrencyStamp: '',
});

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmNewPassword: '',
});

onMounted(async () => {
  try {
    const profile = await authService.getProfile();
    if (profile) {
      profileForm.userId = profile.userName || '';
      profileForm.surname = profile.surname || '';
      profileForm.name = profile.name || '';
      profileForm.email = profile.email || '';
      profileForm.phoneNumber = profile.phoneNumber || '';
      profileForm.concurrencyStamp = profile.concurrencyStamp || '';
    }
  } catch (error) {
    console.error('Failed to load profile:', error);
    message.error('无法加载个人信息，请检查登录状态');
  }
});

const saveProfile = async () => {
  try {
    const payload = {
      userName: profileForm.userId,
      email: profileForm.email,
      name: profileForm.name,
      surname: profileForm.surname,
      phoneNumber: profileForm.phoneNumber,
      concurrencyStamp: profileForm.concurrencyStamp,
    };

    await authService.updateProfile(payload);
    message.success('个人信息保存成功');
    
    // 重新加载以更新 concurrencyStamp
    const profile = await authService.getProfile();
    if (profile) {
      profileForm.concurrencyStamp = profile.concurrencyStamp || '';
    }
  } catch (error) {
    console.error('Failed to save profile:', error);
    message.error('保存失败，请检查输入或登录状态');
  }
};

const changePassword = async () => {
  if (!passwordForm.currentPassword) {
    message.error('请输入原密码');
    return;
  }
  if (!passwordForm.newPassword) {
    message.error('请输入新密码');
    return;
  }
  if (passwordForm.newPassword !== passwordForm.confirmNewPassword) {
    message.error('两次输入的新密码不一致');
    return;
  }

  try {
    const payload = {
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    };

    await authService.changePassword(payload);
    message.success('密码修改成功');
    
    // 清空表单
    passwordForm.currentPassword = '';
    passwordForm.newPassword = '';
    passwordForm.confirmNewPassword = '';
  } catch (error: any) {
    console.error('Failed to change password:', error);
    const errorMsg = error.response?.data?.error?.message || '密码修改失败，请检查原密码是否正确';
    message.error(errorMsg);
  }
};
</script>

<style scoped>
.my-account {
  max-width: 600px;
  margin: 0 auto;
}
</style>

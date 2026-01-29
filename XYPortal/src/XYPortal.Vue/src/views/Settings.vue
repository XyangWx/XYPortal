<template>
  <div class="settings">
    <a-card :bordered="false">
      <a-tabs v-model:activeKey="activeKey">
        <a-tab-pane key="features" tab="功能管理">
          <div class="tab-content">
            <a-typography-title :level="4">功能管理</a-typography-title>
            <a-button type="primary" @click="openFeatureManagementDialog">
              打开功能管理对话框
            </a-button>
          </div>
        </a-tab-pane>
        <a-tab-pane key="email" tab="邮件">
          <div class="tab-content">
            <a-typography-title :level="4">邮件设置</a-typography-title>
            <!-- 邮件设置表单 -->
            <a-form
              ref="emailFormRef"
              :model="emailForm"
              :rules="emailRules"
              layout="vertical"
              style="max-width: 600px"
            >
              <a-form-item label="默认显示名称" name="defaultFromDisplayName">
                <a-input
                  v-model:value="emailForm.defaultFromDisplayName"
                  placeholder="请输入默认显示名称"
                />
              </a-form-item>

              <a-form-item label="默认发件人" name="defaultFromAddress">
                <a-input
                  v-model:value="emailForm.defaultFromAddress"
                  placeholder="请输入默认发件人邮箱"
                  type="email"
                />
              </a-form-item>

              <a-form-item label="主机">
                <a-input
                  v-model:value="emailForm.host"
                  placeholder="请输入SMTP服务器地址"
                />
              </a-form-item>

              <a-form-item label="端口">
                <a-input-number
                  v-model:value="emailForm.port"
                  :min="1"
                  :max="65535"
                  placeholder="请输入端口号"
                  style="width: 100%"
                />
              </a-form-item>

              <a-form-item>
                <a-checkbox v-model:checked="emailForm.enableSsl">
                  启用SSL
                </a-checkbox>
              </a-form-item>

              <a-form-item>
                <a-checkbox v-model:checked="emailForm.useDefaultCredentials">
                  使用默认凭据
                </a-checkbox>
              </a-form-item>

              <a-form-item>
                <a-space>
                  <a-button type="primary" @click="saveEmailSettings" :loading="saving">
                    保存
                  </a-button>
                  <a-button @click="sendTestEmail" :loading="sendingTest">
                    发送测试邮件
                  </a-button>
                </a-space>
              </a-form-item>
            </a-form>
          </div>
        </a-tab-pane>
      </a-tabs>
    </a-card>

    <!-- 发送测试邮件对话框 -->
    <a-modal
      v-model:open="testEmailModalVisible"
      title="发送测试邮件"
      :confirm-loading="sendingTest"
      :maskClosable="false"
      :closable="false"
      @ok="handleSendTestEmail"
      @cancel="handleCancelTestEmail"
    >
      <a-form
        ref="testEmailFormRef"
        :model="testEmailForm"
        :rules="testEmailRules"
        layout="vertical"
      >
        <a-form-item label="发件人邮箱地址" name="senderEmailAddress">
          <a-input
            v-model:value="testEmailForm.senderEmailAddress"
            placeholder="请输入发件人邮箱地址"
            type="email"
          />
        </a-form-item>

        <a-form-item label="收件人邮件地址" name="targetEmailAddress">
          <a-input
            v-model:value="testEmailForm.targetEmailAddress"
            placeholder="请输入收件人邮件地址"
            type="email"
          />
        </a-form-item>

        <a-form-item label="主题" name="subject">
          <a-input
            v-model:value="testEmailForm.subject"
            placeholder="请输入邮件主题（256字以内）"
            :maxlength="256"
          />
        </a-form-item>

        <a-form-item label="正文" name="body">
          <a-textarea
            v-model:value="testEmailForm.body"
            placeholder="请输入邮件正文"
            :rows="6"
          />
        </a-form-item>
      </a-form>

      <template #footer>
        <a-button @click="handleCancelTestEmail">取消</a-button>
        <a-button type="primary" :loading="sendingTest" @click="handleSendTestEmail">
          发送
        </a-button>
      </template>
    </a-modal>

    <!-- 功能管理对话框 -->
    <a-modal
      v-model:open="featureManagementModalVisible"
      title="功能管理"
      width="800px"
      :maskClosable="false"
      :closable="false"
    >
      <a-spin :spinning="loadingFeatures">
        <div class="feature-management-content">
          <div v-if="!featureGroups || featureGroups.length === 0" style="text-align: center; padding: 40px;">
            <a-empty description="暂无功能数据" />
          </div>
          <div v-else>
            <a-collapse v-model:activeKey="activeFeatureGroups" :bordered="false">
              <a-collapse-panel 
                v-for="group in featureGroups" 
                :key="group.name" 
                :header="group.displayName || group.name"
              >
                <div 
                  v-for="feature in (group.features || [])" 
                  :key="feature.name" 
                  :style="{ paddingLeft: ((feature.depth || 0) * 20) + 'px', marginBottom: '16px' }"
                >
                  <!-- Toggle类型 -->
                  <div v-if="feature.valueType && feature.valueType.name === 'ToggleStringValueType'" class="feature-item">
                    <a-switch 
                      v-model:checked="featureValues[feature.name]"
                      :checked-value="'true'"
                      :un-checked-value="'false'"
                    />
                    <span style="margin-left: 8px; font-weight: 500;">{{ feature.displayName || feature.name }}</span>
                    <span v-if="feature.description" style="margin-left: 8px; color: #999; font-size: 12px;">
                      ({{ feature.description }})
                    </span>
                  </div>
                  <!-- FreeText类型 -->
                  <div v-else-if="feature.valueType && feature.valueType.name === 'FreeTextStringValueType'" class="feature-item">
                    <div style="margin-bottom: 4px;">
                      <span style="font-weight: 500;">{{ feature.displayName || feature.name }}</span>
                      <span v-if="feature.description" style="margin-left: 8px; color: #999; font-size: 12px;">
                        ({{ feature.description }})
                      </span>
                    </div>
                    <a-input 
                      v-if="feature.valueType.validator && feature.valueType.validator.name === 'NUMERIC'"
                      v-model:value="featureValues[feature.name]"
                      type="number"
                      :min="feature.valueType.validator.properties?.MinValue"
                      :max="feature.valueType.validator.properties?.MaxValue"
                      style="width: 300px;"
                    />
                    <a-input 
                      v-else
                      v-model:value="featureValues[feature.name]"
                      :maxlength="256"
                      style="width: 300px;"
                    />
                  </div>
                </div>
              </a-collapse-panel>
            </a-collapse>
          </div>
        </div>
      </a-spin>

      <template #footer>
        <a-space>
          <a-button @click="handleCancelFeatureManagement">取消</a-button>
          <a-button @click="handleResetFeatureManagement">重置为默认值</a-button>
          <a-button type="primary" :loading="savingFeatures" @click="handleSaveFeatureManagement">
            保存
          </a-button>
        </a-space>
      </template>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from 'vue';
import { message, Modal } from 'ant-design-vue';
import { authService } from '../services/authService';
import axios from 'axios';

const activeKey = ref('features');
const saving = ref(false);
const sendingTest = ref(false);
const loading = ref(false);
const emailFormRef = ref();
const testEmailModalVisible = ref(false);
const testEmailFormRef = ref();
const featureManagementModalVisible = ref(false);
const savingFeatures = ref(false);
const loadingFeatures = ref(false);
const featureGroups = ref<any[]>([]);
const featureValues = reactive<Record<string, any>>({});
const activeFeatureGroups = ref<string[]>([]);

// 邮件设置表单数据
const emailForm = reactive({
  defaultFromDisplayName: '',
  defaultFromAddress: '',
  host: '',
  port: 587,
  enableSsl: false,
  useDefaultCredentials: true,
});

// 表单验证规则
const emailRules = {
  defaultFromDisplayName: [
    { required: true, message: '请输入默认显示名称', trigger: 'blur' },
  ],
  defaultFromAddress: [
    { required: true, message: '请输入默认发件人', trigger: 'blur' },
    { type: 'email', message: '请输入有效的电子邮件地址', trigger: 'blur' },
  ],
};

// 测试邮件表单数据
const testEmailForm = reactive({
  senderEmailAddress: '',
  targetEmailAddress: '',
  subject: '',
  body: '',
});

// 测试邮件表单验证规则
const testEmailRules = {
  senderEmailAddress: [
    { required: true, message: '请输入发件人邮箱地址', trigger: 'blur' },
    { type: 'email', message: '请输入有效的电子邮件地址', trigger: 'blur' },
  ],
  targetEmailAddress: [
    { required: true, message: '请输入收件人邮件地址', trigger: 'blur' },
    { type: 'email', message: '请输入有效的电子邮件地址', trigger: 'blur' },
  ],
  subject: [
    { required: true, message: '请输入邮件主题', trigger: 'blur' },
    { max: 256, message: '主题不能超过256个字符', trigger: 'blur' },
  ],
};

// 获取邮件设置
const fetchEmailSettings = async () => {
  try {
    loading.value = true;
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/setting-management/emailing`, {
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    // 映射API响应到表单字段
    const data = response.data;
    emailForm.defaultFromDisplayName = data.defaultFromDisplayName || '';
    emailForm.defaultFromAddress = data.defaultFromAddress || '';
    emailForm.host = data.smtpHost || '';
    emailForm.port = data.smtpPort || 587;
    emailForm.enableSsl = data.smtpEnableSsl || false;
    emailForm.useDefaultCredentials = data.smtpUseDefaultCredentials || false;
  } catch (error) {
    message.error('获取邮件设置失败');
    console.error('获取邮件设置失败:', error);
  } finally {
    loading.value = false;
  }
};

// 组件挂载时获取邮件设置
onMounted(() => {
  fetchEmailSettings();
});

// 保存邮件设置
const saveEmailSettings = async () => {
  try {
    // 验证表单
    await emailFormRef.value?.validate();
    
    saving.value = true;
    
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    
    // 构建请求payload
    const payload = {
      smtpHost: emailForm.host,
      smtpPort: emailForm.port,
      smtpUserName: '',
      smtpPassword: '',
      smtpDomain: '',
      smtpEnableSsl: emailForm.enableSsl,
      smtpUseDefaultCredentials: emailForm.useDefaultCredentials,
      defaultFromAddress: emailForm.defaultFromAddress,
      defaultFromDisplayName: emailForm.defaultFromDisplayName,
    };

    await axios.post(`${baseUrl}/api/setting-management/emailing`, payload, {
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });
    
    message.success('邮件设置保存成功');
  } catch (error: any) {
    if (error.errorFields) {
      // 表单验证错误
      message.error('请检查表单填写是否正确');
    } else {
      message.error('保存失败，请重试');
      console.error('保存邮件设置失败:', error);
    }
  } finally {
    saving.value = false;
  }
};

// 发送测试邮件
const sendTestEmail = () => {
  // 打开对话框前，填充默认发件人
  testEmailForm.senderEmailAddress = emailForm.defaultFromAddress;
  testEmailModalVisible.value = true;
};

// 处理发送测试邮件
const handleSendTestEmail = async () => {
  try {
    // 验证表单
    await testEmailFormRef.value?.validate();
    
    sendingTest.value = true;
    
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    
    // 构建请求payload
    const payload = {
      senderEmailAddress: testEmailForm.senderEmailAddress,
      targetEmailAddress: testEmailForm.targetEmailAddress,
      subject: testEmailForm.subject,
      body: testEmailForm.body,
    };

    await axios.post(`${baseUrl}/api/setting-management/emailing/send-test-email`, payload, {
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });
    
    message.success('测试邮件已发送，请检查收件箱');
    testEmailModalVisible.value = false;
    // 清空表单
    testEmailForm.senderEmailAddress = '';
    testEmailForm.targetEmailAddress = '';
    testEmailForm.subject = '';
    testEmailForm.body = '';
  } catch (error: any) {
    if (error.errorFields) {
      message.error('请检查表单填写是否正确');
    } else {
      message.error('发送失败，请检查邮件配置');
      console.error('发送测试邮件失败:', error);
    }
  } finally {
    sendingTest.value = false;
  }
};

// 取消测试邮件
const handleCancelTestEmail = () => {
  testEmailModalVisible.value = false;
  // 清空表单
  testEmailForm.senderEmailAddress = '';
  testEmailForm.targetEmailAddress = '';
  testEmailForm.subject = '';
  testEmailForm.body = '';
  // 清除验证错误
  testEmailFormRef.value?.clearValidate();
};

// 获取功能列表
const fetchFeatures = async () => {
  try {
    loadingFeatures.value = true;
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/feature-management/features?providerName=T`, {
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    featureGroups.value = response.data.groups || [];
    
    // 清空并重新初始化featureValues
    Object.keys(featureValues).forEach(key => {
      delete featureValues[key];
    });
    
    featureGroups.value.forEach(group => {
      if (group.features && Array.isArray(group.features)) {
        group.features.forEach((feature: any) => {
          if (feature && feature.name) {
            featureValues[feature.name] = feature.value || '';
          }
        });
      }
    });
    
    // 默认展开所有分组
    activeFeatureGroups.value = featureGroups.value.map(g => g.name);
  } catch (error) {
    message.error('获取功能列表失败');
    console.error('获取功能列表失败:', error);
  } finally {
    loadingFeatures.value = false;
  }
};

// 打开功能管理对话框
const openFeatureManagementDialog = () => {
  featureManagementModalVisible.value = true;
  // 延迟加载数据，确保对话框先渲染
  setTimeout(() => {
    fetchFeatures();
  }, 100);
};

// 取消功能管理
const handleCancelFeatureManagement = () => {
  featureManagementModalVisible.value = false;
};

// 重置功能为默认值
const handleResetFeatureManagement = async () => {
  // 显示确认对话框
  Modal.confirm({
    title: '确认重置',
    content: '重置操作将恢复所有功能设置为默认值，此操作不可撤销，确定要继续吗？',
    okText: '确定',
    cancelText: '取消',
    onOk: async () => {
      try {
        savingFeatures.value = true;
        
        const user = await authService.getUser();
        if (!user) {
          message.error('未登录，请先登录');
          return;
        }

        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        await axios.delete(
          `${baseUrl}/api/feature-management/features?providerName=T`,
          {
            headers: {
              Authorization: `Bearer ${user.access_token}`,
            },
          }
        );

        message.success('已重置为默认值');
        // 关闭功能管理对话框
        featureManagementModalVisible.value = false;
      } catch (error) {
        message.error('重置失败，请重试');
        console.error('重置功能设置失败:', error);
      } finally {
        savingFeatures.value = false;
      }
    },
  });
};

// 保存功能管理
const handleSaveFeatureManagement = async () => {
  try {
    savingFeatures.value = true;
    
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    // 构建保存的数据格式
    const features = Object.keys(featureValues).map(name => ({
      name: name,
      value: featureValues[name]
    }));

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    await axios.put(
      `${baseUrl}/api/feature-management/features?providerName=T`,
      { features },
      {
        headers: {
          Authorization: `Bearer ${user.access_token}`,
          'Content-Type': 'application/json',
        },
      }
    );

    message.success('功能设置保存成功');
    featureManagementModalVisible.value = false;
  } catch (error) {
    message.error('保存失败，请重试');
    console.error('保存功能设置失败:', error);
  } finally {
    savingFeatures.value = false;
  }
};
</script>

<style scoped>
.settings {
  width: 100%;
}

.tab-content {
  padding: 16px 0;
}
</style>

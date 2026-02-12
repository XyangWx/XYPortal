<template>
  <div class="openiddict-applications">
    <a-card :bordered="false">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px">
        <a-typography-title :level="4" style="margin: 0">应用管理</a-typography-title>
        <a-button
          v-if="hasCreatePermission"
          type="primary"
          style="background-color: #52c41a; border-color: #52c41a"
          @click="handleCreate"
        >
          <template #icon><PlusOutlined /></template>
          新应用
        </a-button>
      </div>

      <a-table
        :columns="columns"
        :data-source="tableData"
        :loading="loading"
        :pagination="pagination"
        row-key="id"
        @change="handleTableChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'actions'">
            <a-dropdown v-if="hasEditPermission || hasDeletePermission">
              <a-button type="primary" size="small">
                <template #icon><SettingOutlined /></template>
                操作
              </a-button>
              <template #overlay>
                <a-menu @click="({ key }: { key: string }) => handleAction(key, record)">
                  <a-menu-item v-if="hasEditPermission" key="edit">编辑</a-menu-item>
                  <a-menu-item v-if="hasDeletePermission" key="delete">删除</a-menu-item>
                </a-menu>
              </template>
            </a-dropdown>
          </template>
          <template v-else-if="column.dataIndex === 'clientType'">
            <a-tag :color="record.clientType === 'confidential' ? 'orange' : 'blue'">
              {{ record.clientType === 'confidential' ? 'Confidential' : 'Public' }}
            </a-tag>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 新建/编辑应用对话框 -->
    <a-modal
      v-model:open="appModalVisible"
      :title="editingApp ? '编辑应用' : '新应用'"
      :maskClosable="false"
      :closable="false"
      width="640px"
    >
      <a-form
        ref="appFormRef"
        :model="appForm"
        :rules="appRules"
        layout="vertical"
      >
        <a-form-item label="Client Id" name="clientId" required>
          <a-input
            v-model:value="appForm.clientId"
            placeholder="请输入 Client Id"
            :disabled="!!editingApp"
          />
        </a-form-item>
        <a-form-item label="显示名称" name="displayName" required>
          <a-input v-model:value="appForm.displayName" placeholder="请输入显示名称" />
        </a-form-item>
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="客户端类型" name="clientType" required>
              <a-select v-model:value="appForm.clientType" @change="handleClientTypeChange">
                <a-select-option value="public">Public</a-select-option>
                <a-select-option value="confidential">Confidential</a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="同意类型" name="consentType" required>
              <a-select v-model:value="appForm.consentType">
                <a-select-option value="explicit">Explicit</a-select-option>
                <a-select-option value="implicit">Implicit</a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
        </a-row>
        <a-form-item
          v-if="appForm.clientType === 'confidential'"
          label="Client Secret"
          name="clientSecret"
          :required="!editingApp"
        >
          <a-input-password
            v-model:value="appForm.clientSecret"
            :placeholder="editingApp ? '留空则不修改' : '请输入 Client Secret'"
          />
        </a-form-item>
        <a-form-item label="Client URI" name="clientUri">
          <a-input v-model:value="appForm.clientUri" placeholder="https://example.com" />
        </a-form-item>

        <a-form-item label="授权类型" required>
          <a-checkbox-group v-model:value="appForm.grantTypes">
            <a-row>
              <a-col :span="12" v-for="gt in availableGrantTypes" :key="gt.value">
                <a-checkbox :value="gt.value" style="margin-bottom: 8px">{{ gt.label }}</a-checkbox>
              </a-col>
            </a-row>
          </a-checkbox-group>
        </a-form-item>

        <a-form-item label="作用域" required>
          <a-checkbox-group v-model:value="appForm.scopes">
            <a-row>
              <a-col :span="8" v-for="scope in availableScopes" :key="scope">
                <a-checkbox :value="scope" style="margin-bottom: 8px">{{ scope }}</a-checkbox>
              </a-col>
            </a-row>
          </a-checkbox-group>
        </a-form-item>

        <a-form-item label="重定向 URIs">
          <div v-for="(uri, index) in appForm.redirectUris" :key="'r' + index" style="display: flex; gap: 8px; margin-bottom: 8px">
            <a-input v-model:value="appForm.redirectUris[index]" placeholder="https://example.com/callback" style="flex: 1" />
            <a-button danger @click="appForm.redirectUris.splice(index, 1)">
              <template #icon><DeleteOutlined /></template>
            </a-button>
          </div>
          <a-button type="dashed" block @click="appForm.redirectUris.push('')">
            <template #icon><PlusOutlined /></template>
            添加
          </a-button>
        </a-form-item>

        <a-form-item label="注销后重定向 URIs">
          <div v-for="(uri, index) in appForm.postLogoutRedirectUris" :key="'p' + index" style="display: flex; gap: 8px; margin-bottom: 8px">
            <a-input v-model:value="appForm.postLogoutRedirectUris[index]" placeholder="https://example.com" style="flex: 1" />
            <a-button danger @click="appForm.postLogoutRedirectUris.splice(index, 1)">
              <template #icon><DeleteOutlined /></template>
            </a-button>
          </div>
          <a-button type="dashed" block @click="appForm.postLogoutRedirectUris.push('')">
            <template #icon><PlusOutlined /></template>
            添加
          </a-button>
        </a-form-item>
      </a-form>
      <template #footer>
        <a-button @click="handleCancelModal">取消</a-button>
        <a-button type="primary" :loading="appModalLoading" @click="handleSave">保存</a-button>
      </template>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { PlusOutlined, SettingOutlined, DeleteOutlined } from '@ant-design/icons-vue';
import { Modal, message } from 'ant-design-vue';
import { permissionService, Permissions } from '../services/permissionService';
import { authService } from '../services/authService';
import axios from 'axios';

// 权限
const hasCreatePermission = computed(() => permissionService.hasPermission(Permissions.OpenIdDict.ApplicationCreate));
const hasEditPermission = computed(() => permissionService.hasPermission(Permissions.OpenIdDict.ApplicationEdit));
const hasDeletePermission = computed(() => permissionService.hasPermission(Permissions.OpenIdDict.ApplicationDelete));

// 表格
const columns = [
  { title: '操作', dataIndex: 'actions', width: 100 },
  { title: 'Client Id', dataIndex: 'clientId' },
  { title: '显示名称', dataIndex: 'displayName' },
  { title: '客户端类型', dataIndex: 'clientType', width: 140 },
];

const tableData = ref<any[]>([]);
const loading = ref(false);
const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: (total: number) => `共 ${total} 条`,
  showSizeChanger: true,
});

// 可选项
const availableGrantTypes = [
  { value: 'authorization_code', label: 'Authorization Code' },
  { value: 'implicit', label: 'Implicit' },
  { value: 'client_credentials', label: 'Client Credentials' },
  { value: 'password', label: 'Password' },
  { value: 'refresh_token', label: 'Refresh Token' },
  { value: 'urn:ietf:params:oauth:grant-type:device_code', label: 'Device Code' },
];

const availableScopes = [
  'address', 'email', 'phone', 'profile', 'roles', 'XYPortal',
];

// 对话框
const appModalVisible = ref(false);
const appModalLoading = ref(false);
const editingApp = ref<any>(null);
const appFormRef = ref();
const appForm = reactive({
  clientId: '',
  displayName: '',
  clientType: 'public' as string,
  consentType: 'implicit' as string,
  clientSecret: '',
  clientUri: '',
  grantTypes: [] as string[],
  scopes: [] as string[],
  redirectUris: [] as string[],
  postLogoutRedirectUris: [] as string[],
});

const appRules = computed(() => ({
  clientId: [{ required: true, message: '请输入 Client Id', trigger: 'blur' }],
  displayName: [{ required: true, message: '请输入显示名称', trigger: 'blur' }],
  clientType: [{ required: true, message: '请选择客户端类型', trigger: 'change' }],
  consentType: [{ required: true, message: '请选择同意类型', trigger: 'change' }],
  clientSecret: editingApp.value
    ? []
    : appForm.clientType === 'confidential'
      ? [{ required: true, message: '请输入 Client Secret', trigger: 'blur' }]
      : [],
}));

const handleClientTypeChange = () => {
  if (appForm.clientType === 'public') {
    appForm.clientSecret = '';
  }
};

// 获取数据
const fetchList = async () => {
  try {
    loading.value = true;
    const user = await authService.getUser();
    if (!user) return;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/app/open-iddict-application`, {
      params: {
        skipCount: (pagination.current - 1) * pagination.pageSize,
        maxResultCount: pagination.pageSize,
      },
      headers: { Authorization: `Bearer ${user.access_token}` },
    });

    tableData.value = response.data.items || [];
    pagination.total = response.data.totalCount || 0;
  } catch (error) {
    message.error('获取应用列表失败');
    console.error('获取应用列表失败:', error);
  } finally {
    loading.value = false;
  }
};

const handleTableChange = (pag: any) => {
  pagination.current = pag.current;
  pagination.pageSize = pag.pageSize;
  fetchList();
};

// 操作
const handleAction = (key: string, record: any) => {
  if (key === 'edit') handleEdit(record);
  else if (key === 'delete') handleDelete(record);
};

const handleCreate = () => {
  editingApp.value = null;
  appForm.clientId = '';
  appForm.displayName = '';
  appForm.clientType = 'public';
  appForm.consentType = 'implicit';
  appForm.clientSecret = '';
  appForm.clientUri = '';
  appForm.grantTypes = [];
  appForm.scopes = [];
  appForm.redirectUris = [];
  appForm.postLogoutRedirectUris = [];
  appFormRef.value?.clearValidate();
  appModalVisible.value = true;
};

const handleEdit = async (record: any) => {
  try {
    const user = await authService.getUser();
    if (!user) return;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/app/open-iddict-application/${record.id}`, {
      headers: { Authorization: `Bearer ${user.access_token}` },
    });

    const data = response.data;
    editingApp.value = data;
    appForm.clientId = data.clientId || '';
    appForm.displayName = data.displayName || '';
    appForm.clientType = data.clientType || 'public';
    appForm.consentType = data.consentType || 'implicit';
    appForm.clientSecret = '';
    appForm.clientUri = data.clientUri || '';
    appForm.grantTypes = data.grantTypes || [];
    appForm.scopes = data.scopes || [];
    appForm.redirectUris = data.redirectUris?.length ? [...data.redirectUris] : [];
    appForm.postLogoutRedirectUris = data.postLogoutRedirectUris?.length ? [...data.postLogoutRedirectUris] : [];
    appFormRef.value?.clearValidate();
    appModalVisible.value = true;
  } catch (error) {
    message.error('获取应用详情失败');
    console.error('获取应用详情失败:', error);
  }
};

const handleDelete = (record: any) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定要删除应用"${record.displayName || record.clientId}"吗？`,
    okText: '确定',
    cancelText: '取消',
    onOk: async () => {
      try {
        const user = await authService.getUser();
        if (!user) return;

        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        await axios.delete(`${baseUrl}/api/app/open-iddict-application/${record.id}`, {
          headers: { Authorization: `Bearer ${user.access_token}` },
        });

        message.success('删除成功');
        fetchList();
      } catch (error) {
        message.error('删除失败');
        console.error('删除失败:', error);
      }
    },
  });
};

const handleCancelModal = () => {
  appModalVisible.value = false;
};

const handleSave = async () => {
  try {
    await appFormRef.value?.validate();
    appModalLoading.value = true;

    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const headers = { Authorization: `Bearer ${user.access_token}` };

    // Filter out empty URIs
    const redirectUris = appForm.redirectUris.filter(u => u.trim());
    const postLogoutRedirectUris = appForm.postLogoutRedirectUris.filter(u => u.trim());

    if (editingApp.value) {
      const payload: any = {
        displayName: appForm.displayName,
        clientType: appForm.clientType,
        consentType: appForm.consentType,
        clientUri: appForm.clientUri || null,
        grantTypes: appForm.grantTypes,
        scopes: appForm.scopes,
        redirectUris,
        postLogoutRedirectUris,
      };
      if (appForm.clientSecret) {
        payload.clientSecret = appForm.clientSecret;
      }
      await axios.put(`${baseUrl}/api/app/open-iddict-application/${editingApp.value.id}`, payload, { headers });
      message.success('更新成功');
    } else {
      const payload: any = {
        clientId: appForm.clientId,
        displayName: appForm.displayName,
        clientType: appForm.clientType,
        consentType: appForm.consentType,
        clientSecret: appForm.clientSecret || null,
        clientUri: appForm.clientUri || null,
        grantTypes: appForm.grantTypes,
        scopes: appForm.scopes,
        redirectUris,
        postLogoutRedirectUris,
      };
      await axios.post(`${baseUrl}/api/app/open-iddict-application`, payload, { headers });
      message.success('创建成功');
    }

    appModalVisible.value = false;
    fetchList();
  } catch (error: any) {
    if (error.errorFields) return;
    message.error(editingApp.value ? '更新失败' : '创建失败');
    console.error('保存失败:', error);
  } finally {
    appModalLoading.value = false;
  }
};

onMounted(() => {
  fetchList();
});
</script>

<style scoped>
.openiddict-applications {
  width: 100%;
}
</style>

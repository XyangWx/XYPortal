<template>
  <div class="openiddict-scopes">
    <a-card :bordered="false">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px">
        <a-typography-title :level="4" style="margin: 0">作用域管理</a-typography-title>
        <a-button
          v-if="hasCreatePermission"
          type="primary"
          style="background-color: #52c41a; border-color: #52c41a"
          @click="handleCreate"
        >
          <template #icon><PlusOutlined /></template>
          新作用域
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
        </template>
      </a-table>
    </a-card>

    <!-- 新建/编辑作用域对话框 -->
    <a-modal
      v-model:open="modalVisible"
      :title="editingScope ? '编辑作用域' : '新作用域'"
      :maskClosable="false"
      :closable="false"
      width="640px"
    >
      <a-form
        ref="formRef"
        :model="form"
        :rules="formRules"
        layout="vertical"
      >
        <a-form-item label="名称" name="name" required>
          <a-input
            v-model:value="form.name"
            placeholder="请输入名称"
            :disabled="!!editingScope"
          />
          <div class="ant-form-item-extra">名称中不能包含空格</div>
        </a-form-item>
        <a-form-item label="显示名称" name="displayName">
          <a-input v-model:value="form.displayName" placeholder="请输入显示名称" />
        </a-form-item>
        <a-form-item label="描述" name="description">
          <a-input v-model:value="form.description" placeholder="请输入描述" />
        </a-form-item>
        <a-form-item label="资源">
          <div v-for="(res, index) in form.resources" :key="'res' + index" style="display: flex; gap: 8px; margin-bottom: 8px">
            <a-input v-model:value="form.resources[index]" placeholder="请输入资源名称" style="flex: 1" />
            <a-button danger @click="form.resources.splice(index, 1)">
              <template #icon><DeleteOutlined /></template>
            </a-button>
          </div>
          <a-button type="dashed" block @click="form.resources.push('')">
            <template #icon><PlusOutlined /></template>
            添加
          </a-button>
        </a-form-item>
      </a-form>
      <template #footer>
        <a-button @click="handleCancelModal">取消</a-button>
        <a-button type="primary" :loading="modalLoading" @click="handleSave">保存</a-button>
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
const hasCreatePermission = computed(() => permissionService.hasPermission(Permissions.OpenIdDict.ScopeCreate));
const hasEditPermission = computed(() => permissionService.hasPermission(Permissions.OpenIdDict.ScopeEdit));
const hasDeletePermission = computed(() => permissionService.hasPermission(Permissions.OpenIdDict.ScopeDelete));

// 表格
const columns = [
  { title: '操作', dataIndex: 'actions', width: 100 },
  { title: '名称', dataIndex: 'name' },
  { title: '显示名称', dataIndex: 'displayName' },
  { title: '描述', dataIndex: 'description' },
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

// 对话框
const modalVisible = ref(false);
const modalLoading = ref(false);
const editingScope = ref<any>(null);
const formRef = ref();
const form = reactive({
  name: '',
  displayName: '',
  description: '',
  resources: [] as string[],
});

const formRules = {
  name: [
    { required: true, message: '请输入名称', trigger: 'blur' },
    { pattern: /^\S+$/, message: '名称中不能包含空格', trigger: 'blur' },
  ],
};

// 获取数据
const fetchList = async () => {
  try {
    loading.value = true;
    const user = await authService.getUser();
    if (!user) return;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/app/open-iddict-scope`, {
      params: {
        skipCount: (pagination.current - 1) * pagination.pageSize,
        maxResultCount: pagination.pageSize,
      },
      headers: { Authorization: `Bearer ${user.access_token}` },
    });

    tableData.value = response.data.items || [];
    pagination.total = response.data.totalCount || 0;
  } catch (error) {
    message.error('获取作用域列表失败');
    console.error('获取作用域列表失败:', error);
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
  editingScope.value = null;
  form.name = '';
  form.displayName = '';
  form.description = '';
  form.resources = [];
  formRef.value?.clearValidate();
  modalVisible.value = true;
};

const handleEdit = async (record: any) => {
  try {
    const user = await authService.getUser();
    if (!user) return;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/app/open-iddict-scope/${record.id}`, {
      headers: { Authorization: `Bearer ${user.access_token}` },
    });

    const data = response.data;
    editingScope.value = data;
    form.name = data.name || '';
    form.displayName = data.displayName || '';
    form.description = data.description || '';
    form.resources = data.resources?.length ? [...data.resources] : [];
    formRef.value?.clearValidate();
    modalVisible.value = true;
  } catch (error) {
    message.error('获取作用域详情失败');
    console.error('获取作用域详情失败:', error);
  }
};

const handleDelete = (record: any) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定要删除作用域"${record.displayName || record.name}"吗？`,
    okText: '确定',
    cancelText: '取消',
    onOk: async () => {
      try {
        const user = await authService.getUser();
        if (!user) return;

        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        await axios.delete(`${baseUrl}/api/app/open-iddict-scope/${record.id}`, {
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
  modalVisible.value = false;
};

const handleSave = async () => {
  try {
    await formRef.value?.validate();
    modalLoading.value = true;

    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const headers = { Authorization: `Bearer ${user.access_token}` };

    const resources = form.resources.filter(r => r.trim());

    if (editingScope.value) {
      const payload = {
        displayName: form.displayName || null,
        description: form.description || null,
        resources,
      };
      await axios.put(`${baseUrl}/api/app/open-iddict-scope/${editingScope.value.id}`, payload, { headers });
      message.success('更新成功');
    } else {
      const payload = {
        name: form.name,
        displayName: form.displayName || null,
        description: form.description || null,
        resources,
      };
      await axios.post(`${baseUrl}/api/app/open-iddict-scope`, payload, { headers });
      message.success('创建成功');
    }

    modalVisible.value = false;
    fetchList();
  } catch (error: any) {
    if (error.errorFields) return;
    const detail = error.response?.data?.error?.message || error.response?.data?.error?.details;
    message.error(detail || (editingScope.value ? '更新失败' : '创建失败'));
    console.error('保存失败:', error);
  } finally {
    modalLoading.value = false;
  }
};

onMounted(() => {
  fetchList();
});
</script>

<style scoped>
.openiddict-scopes {
  width: 100%;
}
</style>

<template>
  <div class="linkboard-categories">
    <a-card :bordered="false">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px">
        <a-typography-title :level="4" style="margin: 0">分类管理</a-typography-title>
        <a-button
          v-if="hasCreatePermission"
          type="primary"
          style="background-color: #52c41a; border-color: #52c41a"
          @click="handleCreate"
        >
          <template #icon><PlusOutlined /></template>
          新建分类
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
          <template v-if="column.dataIndex === 'isPublic'">
            <a-tag :color="record.isPublic ? 'green' : 'default'">{{ record.isPublic ? '公开' : '私有' }}</a-tag>
          </template>
          <template v-else-if="column.dataIndex === 'status'">
            <a-tag :color="getStatusColor(record.status)">{{ getStatusText(record.status) }}</a-tag>
          </template>
          <template v-else-if="column.dataIndex === 'actions'">
            <a-dropdown v-if="hasModifyPermission || hasDeletePermission">
              <a-button type="primary" size="small">
                <template #icon><SettingOutlined /></template>
                操作
              </a-button>
              <template #overlay>
                <a-menu @click="({ key }: { key: string }) => handleAction(key, record)">
                  <a-menu-item v-if="hasModifyPermission" key="edit">编辑</a-menu-item>
                  <a-menu-item v-if="hasModifyPermission && (record.status === 0 || record.status === 3)" key="submit">提交审核</a-menu-item>
                  <a-menu-item v-if="hasModifyPermission && record.status === 1" key="withdraw">撤回</a-menu-item>
                  <a-menu-item v-if="hasDeletePermission" key="delete">删除</a-menu-item>
                </a-menu>
              </template>
            </a-dropdown>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 新建/编辑分类对话框 -->
    <a-modal
      v-model:open="modalVisible"
      :title="editingItem ? '编辑分类' : '新建分类'"
      :maskClosable="false"
      :closable="false"
      width="640px"
    >
      <a-form ref="formRef" :model="form" :rules="formRules" layout="vertical">
        <a-form-item label="名称" name="name" required>
          <a-input v-model:value="form.name" placeholder="请输入名称" :disabled="!!editingItem" />
        </a-form-item>
        <a-form-item label="显示名称" name="displayName">
          <a-input v-model:value="form.displayName" placeholder="请输入显示名称" />
        </a-form-item>
        <a-form-item label="描述" name="description">
          <a-textarea v-model:value="form.description" placeholder="请输入描述" :rows="3" />
        </a-form-item>
        <a-form-item label="图标" name="icon">
          <a-input v-model:value="form.icon" placeholder="fas fa-folder" />
        </a-form-item>
        <a-form-item label="排序" name="sortOrder">
          <a-input-number v-model:value="form.sortOrder" :min="0" style="width: 100%" />
        </a-form-item>
        <a-form-item v-if="!editingItem" label="公开" name="isPublic">
          <a-switch v-model:checked="form.isPublic" />
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
import { PlusOutlined, SettingOutlined } from '@ant-design/icons-vue';
import { Modal, message } from 'ant-design-vue';
import { permissionService, Permissions } from '../services/permissionService';
import { authService } from '../services/authService';
import axios from 'axios';

interface CategoryDto {
  id: string;
  name: string;
  displayName?: string;
  description?: string;
  icon?: string;
  sortOrder: number;
  isPublic: boolean;
  status: number;
}

const loading = ref(false);
const tableData = ref<CategoryDto[]>([]);
const modalVisible = ref(false);
const modalLoading = ref(false);
const editingItem = ref<CategoryDto | null>(null);
const formRef = ref();

const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showSizeChanger: true,
  showTotal: (total: number) => `共 ${total} 条`,
});

const form = reactive({
  name: '',
  displayName: '',
  description: '',
  icon: '',
  sortOrder: 0,
  isPublic: false,
});

const formRules = {
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }],
};

const columns = [
  { title: '名称', dataIndex: 'name', key: 'name' },
  { title: '显示名称', dataIndex: 'displayName', key: 'displayName' },
  { title: '排序', dataIndex: 'sortOrder', key: 'sortOrder', width: 80 },
  { title: '公开', dataIndex: 'isPublic', key: 'isPublic', width: 80 },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100 },
  { title: '操作', dataIndex: 'actions', key: 'actions', width: 100 },
];

const hasCreatePermission = computed(() => permissionService.hasPermission(Permissions.LinkBoard.LinkCategoryCreate));
const hasModifyPermission = computed(() => permissionService.hasPermission(Permissions.LinkBoard.LinkCategoryModify));
const hasDeletePermission = computed(() => permissionService.hasPermission(Permissions.LinkBoard.LinkCategoryDelete));

const getStatusText = (status: number) => {
  const statusMap: Record<number, string> = { 0: '草稿', 1: '待审核', 2: '已批准', 3: '已拒绝' };
  return statusMap[status] || '未知';
};

const getStatusColor = (status: number) => {
  const colorMap: Record<number, string> = { 0: 'default', 1: 'processing', 2: 'success', 3: 'error' };
  return colorMap[status] || 'default';
};

const fetchData = async () => {
  loading.value = true;
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/app/link-category`, {
      headers: { Authorization: `Bearer ${user?.access_token}` },
      params: { skipCount: (pagination.current - 1) * pagination.pageSize, maxResultCount: pagination.pageSize },
    });
    tableData.value = response.data.items;
    pagination.total = response.data.totalCount;
  } catch (error) {
    console.error('获取数据失败:', error);
    message.error('获取数据失败');
  } finally {
    loading.value = false;
  }
};

const handleTableChange = (pag: any) => {
  pagination.current = pag.current;
  pagination.pageSize = pag.pageSize;
  fetchData();
};

const handleCreate = () => {
  editingItem.value = null;
  Object.assign(form, { name: '', displayName: '', description: '', icon: '', sortOrder: 0, isPublic: false });
  modalVisible.value = true;
};

const handleAction = async (key: string, record: CategoryDto) => {
  if (key === 'edit') {
    editingItem.value = record;
    Object.assign(form, { name: record.name, displayName: record.displayName || '', description: record.description || '', icon: record.icon || '', sortOrder: record.sortOrder, isPublic: record.isPublic });
    modalVisible.value = true;
  } else if (key === 'delete') {
    Modal.confirm({
      title: '确认删除', content: `确定要删除分类 "${record.name}" 吗？`,
      async onOk() {
        try {
          const user = await authService.getUser();
          const baseUrl = import.meta.env.VITE_API_BASE_URL;
          await axios.delete(`${baseUrl}/api/app/link-category/${record.id}`, { headers: { Authorization: `Bearer ${user?.access_token}` } });
          message.success('删除成功');
          fetchData();
        } catch { message.error('删除失败'); }
      },
    });
  } else if (key === 'submit' || key === 'withdraw') {
    try {
      const user = await authService.getUser();
      const baseUrl = import.meta.env.VITE_API_BASE_URL;
      await axios.post(`${baseUrl}/api/app/link-category/${record.id}/${key}`, {}, { headers: { Authorization: `Bearer ${user?.access_token}` } });
      message.success('操作成功');
      fetchData();
    } catch { message.error('操作失败'); }
  }
};

const handleCancelModal = () => { modalVisible.value = false; };

const handleSave = async () => {
  try {
    await formRef.value.validate();
    modalLoading.value = true;
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    if (editingItem.value) {
      await axios.put(`${baseUrl}/api/app/link-category/${editingItem.value.id}`, { displayName: form.displayName, description: form.description, icon: form.icon, sortOrder: form.sortOrder }, { headers: { Authorization: `Bearer ${user?.access_token}` } });
    } else {
      await axios.post(`${baseUrl}/api/app/link-category`, form, { headers: { Authorization: `Bearer ${user?.access_token}` } });
    }
    message.success('保存成功');
    modalVisible.value = false;
    fetchData();
  } catch { message.error('保存失败'); } finally { modalLoading.value = false; }
};

onMounted(() => { fetchData(); });
</script>

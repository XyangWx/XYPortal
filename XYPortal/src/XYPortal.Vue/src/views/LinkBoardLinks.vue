<template>
  <div class="linkboard-links">
    <a-card :bordered="false">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px">
        <a-typography-title :level="4" style="margin: 0">链接管理</a-typography-title>
        <a-button
          v-if="hasCreatePermission"
          type="primary"
          style="background-color: #52c41a; border-color: #52c41a"
          @click="handleCreate"
        >
          <template #icon><PlusOutlined /></template>
          新建链接
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
          <template v-if="column.dataIndex === 'url'">
            <a :href="record.url" target="_blank">{{ record.url }}</a>
          </template>
          <template v-else-if="column.dataIndex === 'isPublic'">
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

    <!-- 新建/编辑链接对话框 -->
    <a-modal
      v-model:open="modalVisible"
      :title="editingItem ? '编辑链接' : '新建链接'"
      :maskClosable="false"
      :closable="false"
      width="640px"
    >
      <a-form ref="formRef" :model="form" :rules="formRules" layout="vertical">
        <a-form-item label="分类" name="categoryId" required>
          <a-select v-model:value="form.categoryId" placeholder="请选择分类" @change="refreshSortOrder">
            <a-select-option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.displayName || cat.name }}</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="标题" name="title" required>
          <a-input v-model:value="form.title" placeholder="请输入标题" />
        </a-form-item>
        <a-form-item label="网址" name="url" required>
          <a-input v-model:value="form.url" placeholder="请输入网址" />
        </a-form-item>
        <a-form-item label="描述" name="description">
          <a-textarea v-model:value="form.description" placeholder="请输入描述" :rows="3" />
        </a-form-item>
        <a-form-item label="图标" name="icon">
          <a-input v-model:value="form.icon" placeholder="fas fa-link" />
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

interface LinkDto { id: string; categoryId: string; categoryName?: string; title: string; url: string; description?: string; icon?: string; sortOrder: number; isPublic: boolean; status: number; }
interface CategoryDto { id: string; name: string; displayName?: string; }

const loading = ref(false);
const tableData = ref<LinkDto[]>([]);
const categories = ref<CategoryDto[]>([]);
const modalVisible = ref(false);
const modalLoading = ref(false);
const editingItem = ref<LinkDto | null>(null);
const formRef = ref();

const pagination = reactive({ current: 1, pageSize: 10, total: 0, showSizeChanger: true, showTotal: (total: number) => `共 ${total} 条` });

const form = reactive({ categoryId: '', title: '', url: '', description: '', icon: '', sortOrder: 0, isPublic: false });

const formRules = {
  categoryId: [{ required: true, message: '请选择分类', trigger: 'change' }],
  title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
  url: [{ required: true, message: '请输入网址', trigger: 'blur' }],
};

const columns = [
  { title: '标题', dataIndex: 'title', key: 'title' },
  { title: '网址', dataIndex: 'url', key: 'url', ellipsis: true },
  { title: '分类', dataIndex: 'categoryName', key: 'categoryName', width: 120 },
  { title: '排序', dataIndex: 'sortOrder', key: 'sortOrder', width: 80 },
  { title: '公开', dataIndex: 'isPublic', key: 'isPublic', width: 80 },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100 },
  { title: '操作', dataIndex: 'actions', key: 'actions', width: 100 },
];

const hasCreatePermission = computed(() => permissionService.hasPermission(Permissions.LinkBoard.LinkCreate));
const hasModifyPermission = computed(() => permissionService.hasPermission(Permissions.LinkBoard.LinkModify));
const hasDeletePermission = computed(() => permissionService.hasPermission(Permissions.LinkBoard.LinkDelete));

const getStatusText = (status: number) => ({ 0: '草稿', 1: '待审核', 2: '已批准', 3: '已拒绝' }[status] || '未知');
const getStatusColor = (status: number) => ({ 0: 'default', 1: 'processing', 2: 'success', 3: 'error' }[status] || 'default');

const fetchCategories = async () => {
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/app/link-category`, { headers: { Authorization: `Bearer ${user?.access_token}` }, params: { maxResultCount: 1000 } });
    categories.value = response.data.items;
  } catch (error) { console.error('获取分类失败:', error); }
};

const fetchData = async () => {
  loading.value = true;
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/app/link`, { headers: { Authorization: `Bearer ${user?.access_token}` }, params: { skipCount: (pagination.current - 1) * pagination.pageSize, maxResultCount: pagination.pageSize } });
    tableData.value = response.data.items;
    pagination.total = response.data.totalCount;
  } catch (error) { message.error('获取数据失败'); } finally { loading.value = false; }
};

const handleTableChange = (pag: any) => { pagination.current = pag.current; pagination.pageSize = pag.pageSize; fetchData(); };

const handleCreate = () => {
  editingItem.value = null;
  Object.assign(form, { categoryId: '', title: '', url: '', description: '', icon: '', sortOrder: 0, isPublic: false });
  modalVisible.value = true;
};

const refreshSortOrder = async () => {
  if (!form.categoryId) {
    form.sortOrder = 0;
    return;
  }
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/app/link/max-index`, {
      headers: { Authorization: `Bearer ${user?.access_token}` },
      params: { categoryId: form.categoryId }
    });
    form.sortOrder = response.data.index;
  } catch (error) {
    console.error('获取最大排序索引失败:', error);
  }
};

const handleAction = async (key: string, record: LinkDto) => {
  if (key === 'edit') {
    editingItem.value = record;
    Object.assign(form, { categoryId: record.categoryId, title: record.title, url: record.url, description: record.description || '', icon: record.icon || '', sortOrder: record.sortOrder, isPublic: record.isPublic });
    modalVisible.value = true;
  } else if (key === 'delete') {
    Modal.confirm({ title: '确认删除', content: `确定要删除链接 "${record.title}" 吗？`, async onOk() {
      try { const user = await authService.getUser(); const baseUrl = import.meta.env.VITE_API_BASE_URL;
        await axios.delete(`${baseUrl}/api/app/link/${record.id}`, { headers: { Authorization: `Bearer ${user?.access_token}` } });
        message.success('删除成功'); fetchData();
      } catch { message.error('删除失败'); }
    }});
  } else if (key === 'submit' || key === 'withdraw') {
    try { const user = await authService.getUser(); const baseUrl = import.meta.env.VITE_API_BASE_URL;
      await axios.post(`${baseUrl}/api/app/link/${record.id}/${key}`, {}, { headers: { Authorization: `Bearer ${user?.access_token}` } });
      message.success('操作成功'); fetchData();
    } catch { message.error('操作失败'); }
  }
};

const handleCancelModal = () => { modalVisible.value = false; };

const handleSave = async () => {
  try {
    await formRef.value.validate(); modalLoading.value = true;
    const user = await authService.getUser(); const baseUrl = import.meta.env.VITE_API_BASE_URL;
    if (editingItem.value) {
      await axios.put(`${baseUrl}/api/app/link/${editingItem.value.id}`, { categoryId: form.categoryId, title: form.title, url: form.url, description: form.description, icon: form.icon, sortOrder: form.sortOrder }, { headers: { Authorization: `Bearer ${user?.access_token}` } });
    } else {
      await axios.post(`${baseUrl}/api/app/link`, form, { headers: { Authorization: `Bearer ${user?.access_token}` } });
    }
    message.success('保存成功'); modalVisible.value = false; fetchData();
  } catch { message.error('保存失败'); } finally { modalLoading.value = false; }
};

onMounted(() => { fetchCategories(); fetchData(); });
</script>

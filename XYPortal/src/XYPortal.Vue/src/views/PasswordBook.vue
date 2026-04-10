<template>
  <div class="password-book">
    <a-card :bordered="false">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px">
        <a-typography-title :level="4" style="margin: 0">密码本</a-typography-title>
        <a-button
          v-if="hasCreatePermission"
          type="primary"
          style="background-color: #1890ff; border-color: #1890ff"
          @click="handleCreate"
        >
          <template #icon><PlusOutlined /></template>
          新建密码本
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
          <template v-if="column.dataIndex === 'allowedType'">
            <a-tag :color="record.allowedType === 1 ? 'blue' : 'green'">
              {{ record.allowedType === 1 ? 'General' : 'NumericOnly' }}
            </a-tag>
          </template>
          <template v-else-if="column.dataIndex === 'creationTime'">
            {{ formatDate(record.creationTime) }}
          </template>
          <template v-else-if="column.dataIndex === 'actions'">
            <a-dropdown v-if="hasModifyPermission || hasDeletePermission">
              <a-button type="primary" size="small">
                <template #icon><SettingOutlined /></template>
                操作
              </a-button>
              <template #overlay>
                <a-menu @click="({ key }: { key: string }) => handleAction(key, record)">
                  <a-menu-item v-if="hasModifyPermission" key="view">查看</a-menu-item>
                  <a-menu-item v-if="hasDeletePermission" key="delete">删除</a-menu-item>
                </a-menu>
              </template>
            </a-dropdown>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 新建/编辑密码本对话框 -->
    <a-modal
      v-model:open="modalVisible"
      :title="editingItem ? '编辑密码本' : '新建密码本'"
      :maskClosable="false"
      width="540px"
      @ok="handleSave"
      @cancel="handleCancelModal"
    >
      <a-form ref="formRef" :model="form" :rules="formRules" layout="vertical">
        <a-form-item label="名称" name="name" required>
          <a-input v-model:value="form.name" placeholder="请输入密码本名称" />
        </a-form-item>
        <a-form-item label="描述" name="description">
          <a-textarea v-model:value="form.description" placeholder="请输入描述" :rows="3" />
        </a-form-item>
        <a-form-item label="密码类型" name="allowedType">
          <a-select v-model:value="form.allowedType" placeholder="请选择密码类型">
            <a-select-option :value="1">General</a-select-option>
            <a-select-option :value="0">NumericOnly</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="最小长度" name="minLength">
          <a-input-number v-model:value="form.minLength" :min="4" :max="100" style="width: 100%" />
        </a-form-item>
        <a-form-item label="最大长度" name="maxLength">
          <a-input-number v-model:value="form.maxLength" :min="8" :max="200" style="width: 100%" />
        </a-form-item>
      </a-form>
      <template #footer>
        <a-button @click="handleCancelModal">取消</a-button>
        <a-button type="primary" :loading="modalLoading" @click="handleSave">保存</a-button>
      </template>
    </a-modal>

    <!-- 查看密码本详情对话框 -->
    <a-modal
      v-model:open="viewModalVisible"
      title="密码本详情"
      :maskClosable="false"
      width="800px"
      @cancel="viewModalVisible = false"
    >
      <a-descriptions bordered :column="2">
        <a-descriptions-item label="名称">{{ viewData.name }}</a-descriptions-item>
        <a-descriptions-item label="描述">{{ viewData.description || '-' }}</a-descriptions-item>
        <a-descriptions-item label="密码类型">
          <a-tag :color="viewData.allowedType === 1 ? 'blue' : 'green'">
            {{ viewData.allowedType === 1 ? 'General' : 'NumericOnly' }}
          </a-tag>
        </a-descriptions-item>
        <a-descriptions-item label="创建时间">{{ formatDate(viewData.creationTime) }}</a-descriptions-item>
        <a-descriptions-item label="ID" :span="2">{{ viewData.id }}</a-descriptions-item>
      </a-descriptions>

      <a-divider>密码条目</a-divider>

      <div style="margin-bottom: 16px">
        <a-button type="primary" @click="showCreateEntryModal">
          <template #icon><PlusOutlined /></template>
          添加密码条目
        </a-button>
      </div>

      <a-table
        :columns="entryColumns"
        :data-source="entryTableData"
        :loading="entryLoading"
        :pagination="entryPagination"
        row-key="id"
        size="small"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'isDeleted'">
            <a-tag :color="record.isDeleted ? 'default' : 'success'">
              {{ record.isDeleted ? '已删除' : '有效' }}
            </a-tag>
          </template>
          <template v-else-if="column.dataIndex === 'actions'">
            <a-space>
              <a-button type="link" size="small" @click="showPasswordEntry(record)">查看密码</a-button>
              <a-button v-if="!record.isDeleted" type="link" size="small" @click="copyPassword(record)">复制</a-button>
              <a-button v-if="record.isDeleted" type="link" size="small" @click="restoreEntry(record)">恢复</a-button>
              <a-button v-if="!record.isDeleted" type="link" danger size="small" @click="deleteEntry(record)">删除</a-button>
            </a-space>
          </template>
        </template>
      </a-table>

      <template #footer>
        <a-button @click="viewModalVisible = false">关闭</a-button>
      </template>
    </a-modal>

    <!-- 添加密码条目对话框 -->
    <a-modal
      v-model:open="createEntryModalVisible"
      title="添加密码条目"
      :maskClosable="false"
      width="600px"
      @ok="handleCreateEntry"
      @cancel="createEntryModalVisible = false"
    >
      <a-form ref="entryFormRef" :model="entryForm" :rules="entryFormRules" layout="vertical">
        <a-form-item label="标题" name="title" required>
          <a-input v-model:value="entryForm.title" placeholder="请输入标题" />
        </a-form-item>
        <a-form-item label="密码" name="password" required>
          <a-input-password v-model:value="entryForm.password" placeholder="请输入密码" />
        </a-form-item>
        <a-form-item label="是否有用户名">
          <a-switch v-model:checked="entryForm.hasUsername" />
        </a-form-item>
        <a-form-item v-if="entryForm.hasUsername" label="用户名" name="username">
          <a-input v-model:value="entryForm.username" placeholder="请输入用户名" />
        </a-form-item>
        <a-form-item label="密码类型" name="passwordType">
          <a-select v-model:value="entryForm.passwordType" placeholder="请选择密码类型">
            <a-select-option :value="1">General</a-select-option>
            <a-select-option :value="0">NumericOnly</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="备注">
          <a-textarea v-model:value="entryForm.remark" placeholder="请输入备注" :rows="2" />
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- 查看密码对话框 -->
    <a-modal
      v-model:open="showPasswordModalVisible"
      title="查看密码"
      :maskClosable="false"
      :closable="false"
      width="400px"
    >
      <a-descriptions bordered :column="1">
        <a-descriptions-item label="标题">{{ showPasswordData.title }}</a-descriptions-item>
        <a-descriptions-item label="用户名">{{ showPasswordData.username || '-' }}</a-descriptions-item>
        <a-descriptions-item label="密码">
          <a-input-password v-model:value="showPasswordData.password" readonly />
        </a-descriptions-item>
        <a-descriptions-item label="备注">{{ showPasswordData.remark || '-' }}</a-descriptions-item>
      </a-descriptions>
      <template #footer>
        <a-button @click="copyShowPassword">复制密码</a-button>
        <a-button type="primary" @click="showPasswordModalVisible = false">关闭</a-button>
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

interface PasswordBookDto {
  id: string;
  name: string;
  description?: string;
  allowedType: number;
  minLength: number;
  maxLength: number;
  creationTime: string;
  entryCount: number;
}

interface PasswordEntryDto {
  id: string;
  passwordBookId: string;
  title: string;
  hasUsername: boolean;
  username?: string;
  passwordType: number;
  weakLevel?: number;
  remark?: string;
  currentPassword?: string;
  isDeleted: boolean;
}

const loading = ref(false);
const tableData = ref<PasswordBookDto[]>([]);
const pagination = reactive({ current: 1, pageSize: 10, total: 0, showSizeChanger: true, showTotal: (total: number) => `共 ${total} 条` });

const modalVisible = ref(false);
const modalLoading = ref(false);
const editingItem = ref<PasswordBookDto | null>(null);
const formRef = ref();

const form = reactive({
  name: '',
  description: '',
  allowedType: 1,
  minLength: 8,
  maxLength: 20,
});

const formRules = {
  name: [{ required: true, message: '请输入密码本名称', trigger: 'blur' }],
};

const columns = [
  { title: '名称', dataIndex: 'name', key: 'name' },
  { title: '描述', dataIndex: 'description', key: 'description', ellipsis: true },
  { title: '密码类型', dataIndex: 'allowedType', key: 'allowedType', width: 120 },
  { title: '创建时间', dataIndex: 'creationTime', key: 'creationTime', width: 180 },
  { title: '操作', dataIndex: 'actions', key: 'actions', width: 120 },
];

// 查看详情相关
const viewModalVisible = ref(false);
const viewData = reactive<Partial<PasswordBookDto>>({});
const entryTableData = ref<PasswordEntryDto[]>([]);
const entryLoading = ref(false);
const entryPagination = reactive({ current: 1, pageSize: 5, total: 0, showSizeChanger: true, showTotal: (total: number) => `共 ${total} 条` });

const entryColumns = [
  { title: '标题', dataIndex: 'title', key: 'title' },
  { title: '用户名', dataIndex: 'username', key: 'username', width: 120 },
  { title: '状态', dataIndex: 'isDeleted', key: 'isDeleted', width: 80 },
  { title: '操作', dataIndex: 'actions', key: 'actions', width: 180 },
];

// 创建条目相关
const createEntryModalVisible = ref(false);
const entryFormRef = ref();
const entryForm = reactive({
  title: '',
  password: '',
  hasUsername: false,
  username: '',
  passwordType: 1,
  remark: '',
});
const entryFormRules = {
  title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
};

// 查看密码相关
const showPasswordModalVisible = ref(false);
const showPasswordData = reactive<Partial<PasswordEntryDto>>({});

const hasCreatePermission = computed(() => permissionService.hasPermission(Permissions.PasswordBook.Create));
const hasModifyPermission = computed(() => permissionService.hasPermission(Permissions.PasswordBook.Update));
const hasDeletePermission = computed(() => permissionService.hasPermission(Permissions.PasswordBook.Delete));
const hasUserPermission = computed(() => permissionService.hasPermission(Permissions.PasswordBook.User));

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleString();
};

const fetchData = async () => {
  if (!hasUserPermission.value) {
    message.error('您没有使用密码本的权限');
    return;
  }

  loading.value = true;
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/password-book`, {
      headers: { Authorization: `Bearer ${user?.access_token}` },
    });
    tableData.value = response.data.items || [];
    pagination.total = tableData.value.length;
  } catch (error: any) {
    message.error('获取数据失败: ' + (error?.message || '未知错误'));
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
  Object.assign(form, { name: '', description: '', allowedType: 1, minLength: 8, maxLength: 20 });
  modalVisible.value = true;
};

const handleCancelModal = () => {
  modalVisible.value = false;
};

const handleSave = async () => {
  try {
    await formRef.value.validate();
    modalLoading.value = true;
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;

    if (editingItem.value) {
      await axios.put(
        `${baseUrl}/api/password-book/${editingItem.value.id}`,
        {
          name: form.name,
          description: form.description,
          allowedType: form.allowedType,
          minLength: form.minLength,
          maxLength: form.maxLength,
        },
        { headers: { Authorization: `Bearer ${user?.access_token}` } }
      );
      message.success('更新成功');
    } else {
      await axios.post(
        `${baseUrl}/api/password-book`,
        {
          name: form.name,
          description: form.description,
          allowedType: form.allowedType,
          minLength: form.minLength,
          maxLength: form.maxLength,
        },
        { headers: { Authorization: `Bearer ${user?.access_token}` } }
      );
      message.success('创建成功');
    }
    modalVisible.value = false;
    fetchData();
  } catch (error: any) {
    message.error('保存失败: ' + (error?.message || '未知错误'));
  } finally {
    modalLoading.value = false;
  }
};

const handleAction = async (key: string, record: PasswordBookDto) => {
  if (key === 'view') {
    await viewPasswordBook(record);
  } else if (key === 'delete') {
    Modal.confirm({
      title: '确认删除',
      content: `确定要删除密码本 "${record.name}" 吗？`,
      async onOk() {
        try {
          const user = await authService.getUser();
          const baseUrl = import.meta.env.VITE_API_BASE_URL;
          await axios.delete(`${baseUrl}/api/password-book/${record.id}`, {
            headers: { Authorization: `Bearer ${user?.access_token}` },
          });
          message.success('删除成功');
          fetchData();
        } catch {
          message.error('删除失败');
        }
      },
    });
  }
};

const viewPasswordBook = async (record: PasswordBookDto) => {
  Object.assign(viewData, record);
  viewModalVisible.value = true;
  await fetchEntries(record.id);
};

const fetchEntries = async (passwordBookId: string) => {
  entryLoading.value = true;
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/password-book/${passwordBookId}/with-entries`, {
      headers: { Authorization: `Bearer ${user?.access_token}` },
    });
    entryTableData.value = response.data.passwordEntries || [];
    entryPagination.total = entryTableData.value.length;
  } catch {
    message.error('获取密码条目失败');
  } finally {
    entryLoading.value = false;
  }
};

const showCreateEntryModal = () => {
  Object.assign(entryForm, { title: '', password: '', hasUsername: false, username: '', passwordType: 1, remark: '' });
  createEntryModalVisible.value = true;
};

const handleCreateEntry = async () => {
  try {
    await entryFormRef.value.validate();
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    await axios.post(
      `${baseUrl}/api/password-book/${viewData.id}/entries`,
      {
        title: entryForm.title,
        password: entryForm.password,
        hasUsername: entryForm.hasUsername,
        username: entryForm.hasUsername ? entryForm.username : null,
        passwordType: entryForm.passwordType,
        remark: entryForm.remark || null,
      },
      { headers: { Authorization: `Bearer ${user?.access_token}` } }
    );
    message.success('添加成功');
    createEntryModalVisible.value = false;
    await fetchEntries(viewData.id!);
  } catch {
    message.error('添加失败');
  }
};

const showPasswordEntry = async (entry: PasswordEntryDto) => {
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(
      `${baseUrl}/api/password-book/${entry.passwordBookId}/entries/${entry.id}?queryKind=${entry.isDeleted ? 1 : 0}`,
      { headers: { Authorization: `Bearer ${user?.access_token}` } }
    );
    Object.assign(showPasswordData, response.data);
    showPasswordModalVisible.value = true;
  } catch {
    message.error('获取密码失败');
  }
};

const copyPassword = async (entry: PasswordEntryDto) => {
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(
      `${baseUrl}/api/password-book/${entry.passwordBookId}/entries/${entry.id}`,
      { headers: { Authorization: `Bearer ${user?.access_token}` } }
    );
    await navigator.clipboard.writeText(response.data.currentPassword || '');
    message.success('密码已复制');
  } catch {
    message.error('复制失败');
  }
};

const copyShowPassword = async () => {
  await navigator.clipboard.writeText(showPasswordData.password || '');
  message.success('密码已复制');
};

const deleteEntry = async (entry: PasswordEntryDto) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定要删除密码条目 "${entry.title}" 吗？`,
    async onOk() {
      try {
        const user = await authService.getUser();
        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        await axios.delete(
          `${baseUrl}/api/password-book/${entry.passwordBookId}/entries/${entry.id}?queryKind=0`,
          { headers: { Authorization: `Bearer ${user?.access_token}` } }
        );
        message.success('删除成功');
        await fetchEntries(entry.passwordBookId);
      } catch {
        message.error('删除失败');
      }
    },
  });
};

const restoreEntry = async (entry: PasswordEntryDto) => {
  try {
    const user = await authService.getUser();
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    await axios.post(
      `${baseUrl}/api/password-book/${entry.passwordBookId}/entries/${entry.id}/restore`,
      {},
      { headers: { Authorization: `Bearer ${user?.access_token}` } }
    );
    message.success('恢复成功');
    await fetchEntries(entry.passwordBookId);
  } catch {
    message.error('恢复失败');
  }
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped>
.password-book {
  width: 100%;
}
</style>

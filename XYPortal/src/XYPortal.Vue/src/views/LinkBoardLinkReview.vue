<template>
  <div class="linkboard-link-review">
    <a-card :bordered="false">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px">
        <a-typography-title :level="4" style="margin: 0">链接审核</a-typography-title>
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
            <a :href="record.url" target="_blank" rel="noopener noreferrer">{{ record.url }}</a>
          </template>
          <template v-else-if="column.dataIndex === 'isPublic'">
            <a-tag :color="record.isPublic ? 'green' : 'default'">{{ record.isPublic ? '公开' : '私有' }}</a-tag>
          </template>
          <template v-else-if="column.dataIndex === 'status'">
            <a-tag :color="getStatusColor(record.status)">{{ getStatusText(record.status) }}</a-tag>
          </template>
          <template v-else-if="column.dataIndex === 'actions'">
            <a-space>
              <a-button type="primary" size="small" @click="handleApprove(record)">
                <template #icon><CheckOutlined /></template>
                通过
              </a-button>
              <a-button danger size="small" @click="handleReject(record)">
                <template #icon><CloseOutlined /></template>
                拒绝
              </a-button>
            </a-space>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 拒绝对话框 -->
    <a-modal
      v-model:open="rejectModalVisible"
      title="拒绝原因"
      :maskClosable="false"
      :closable="false"
      width="480px"
    >
      <a-form layout="vertical">
        <a-form-item label="拒绝原因">
          <a-textarea v-model:value="rejectComment" placeholder="请输入拒绝原因（可选）" :rows="3" />
        </a-form-item>
      </a-form>
      <template #footer>
        <a-button @click="handleCancelReject">取消</a-button>
        <a-button type="primary" danger :loading="modalLoading" @click="handleConfirmReject">确认拒绝</a-button>
      </template>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from 'vue';
import { CheckOutlined, CloseOutlined } from '@ant-design/icons-vue';
import { Modal, message } from 'ant-design-vue';
import { authService } from '../services/authService';
import axios from 'axios';

interface LinkDto {
  id: string;
  categoryId: string;
  categoryName?: string;
  title: string;
  url: string;
  description?: string;
  icon?: string;
  sortOrder: number;
  isPublic: boolean;
  status: number;
}

const columns = [
  { title: '标题', dataIndex: 'title', key: 'title' },
  { title: 'URL', dataIndex: 'url', key: 'url', ellipsis: true },
  { title: '分类', dataIndex: 'categoryName', key: 'categoryName' },
  { title: '排序', dataIndex: 'sortOrder', key: 'sortOrder', width: 80 },
  { title: '公开', dataIndex: 'isPublic', key: 'isPublic', width: 80 },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100 },
  { title: '操作', dataIndex: 'actions', key: 'actions', width: 160 },
];

const loading = ref(false);
const tableData = ref<LinkDto[]>([]);
const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showSizeChanger: true,
  showTotal: (total: number) => `共 ${total} 条`,
});

const rejectModalVisible = ref(false);
const rejectComment = ref('');
const rejectingItem = ref<LinkDto | null>(null);
const modalLoading = ref(false);

const getStatusText = (status: number) => {
  const statusMap: Record<number, string> = {
    0: '草稿',
    1: '待审核',
    2: '已通过',
    3: '已拒绝',
  };
  return statusMap[status] || '未知';
};

const getStatusColor = (status: number) => {
  const colorMap: Record<number, string> = {
    0: 'default',
    1: 'processing',
    2: 'success',
    3: 'error',
  };
  return colorMap[status] || 'default';
};

const getAccessToken = async () => {
  const user = await authService.getUser();
  return user?.access_token || '';
};

const fetchData = async () => {
  loading.value = true;
  try {
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const token = await getAccessToken();
    const response = await axios.get(`${baseUrl}/api/app/link-review`, {
      params: {
        SkipCount: (pagination.current - 1) * pagination.pageSize,
        MaxResultCount: pagination.pageSize,
        Status: 1, // 只获取待审核的
      },
      headers: { Authorization: `Bearer ${token}` },
    });
    tableData.value = response.data.items;
    pagination.total = response.data.totalCount;
  } catch (error) {
    message.error('获取数据失败');
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const handleTableChange = (pag: any) => {
  pagination.current = pag.current;
  pagination.pageSize = pag.pageSize;
  fetchData();
};

const handleApprove = (record: LinkDto) => {
  Modal.confirm({
    title: '确认通过',
    content: `确定要通过链接「${record.title}」吗？`,
    okText: '确认',
    cancelText: '取消',
    onOk: async () => {
      try {
        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        const token = await getAccessToken();
        await axios.post(`${baseUrl}/api/app/link-review/${record.id}/review`, {
          status: 2, // Approved
        }, {
          headers: { Authorization: `Bearer ${token}` },
        });
        message.success('审核通过');
        fetchData();
      } catch (error) {
        message.error('操作失败');
        console.error(error);
      }
    },
  });
};

const handleReject = (record: LinkDto) => {
  rejectingItem.value = record;
  rejectComment.value = '';
  rejectModalVisible.value = true;
};

const handleCancelReject = () => {
  rejectModalVisible.value = false;
  rejectingItem.value = null;
  rejectComment.value = '';
};

const handleConfirmReject = async () => {
  if (!rejectingItem.value) return;
  
  modalLoading.value = true;
  try {
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const token = await getAccessToken();
    await axios.post(`${baseUrl}/api/app/link-review/${rejectingItem.value.id}/review`, {
      status: 3, // Rejected
      reviewComment: rejectComment.value || undefined,
    }, {
      headers: { Authorization: `Bearer ${token}` },
    });
    message.success('已拒绝');
    rejectModalVisible.value = false;
    rejectingItem.value = null;
    rejectComment.value = '';
    fetchData();
  } catch (error) {
    message.error('操作失败');
    console.error(error);
  } finally {
    modalLoading.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped>
.linkboard-link-review {
  padding: 16px;
}
</style>

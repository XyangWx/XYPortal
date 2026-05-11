<template>
  <div class="users">
    <a-card :bordered="false">
      <div class="users-header">
        <a-typography-title :level="4" style="margin: 0">用户</a-typography-title>
        <div style="display: flex; align-items: center; gap: 12px">
          <a-input-search
            v-model:value="searchText"
            placeholder="搜索"
            style="width: 250px"
            allow-clear
            @search="handleSearch"
          />
          <a-button
            v-if="hasCreatePermission"
            type="primary"
            style="background-color: #52c41a; border-color: #52c41a"
            @click="handleCreate"
          >
            <template #icon><PlusOutlined /></template>
            新用户
          </a-button>
        </div>
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
            <a-dropdown v-if="hasUpdatePermission || hasManagePermissionsPermission || (hasDeletePermission && !isCurrentUser(record))">
              <a-button type="primary" size="small">
                <template #icon><SettingOutlined /></template>
                操作
              </a-button>
              <template #overlay>
                <a-menu @click="({ key }: { key: string }) => handleAction(key, record)">
                  <a-menu-item v-if="hasUpdatePermission" key="edit">编辑</a-menu-item>
                  <a-menu-item v-if="hasManagePermissionsPermission" key="permissions">权限</a-menu-item>
                  <a-menu-item v-if="hasDeletePermission && !isCurrentUser(record)" key="delete">删除</a-menu-item>
                </a-menu>
              </template>
            </a-dropdown>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 新建/编辑用户对话框 -->
    <a-modal
      v-model:open="userModalVisible"
      :title="editingUser ? '编辑用户' : '新用户'"
      :maskClosable="false"
      :closable="false"
      :confirm-loading="userModalLoading"
    >
      <a-tabs v-model:activeKey="userModalTab">
        <a-tab-pane key="info" tab="用户信息">
          <a-form
            ref="userFormRef"
            :model="userForm"
            :rules="userRules"
            layout="vertical"
          >
            <a-form-item label="用户名称" name="userName" required>
              <a-input v-model:value="userForm.userName" placeholder="请输入用户名称" />
            </a-form-item>
            <a-row :gutter="16">
              <a-col :span="12">
                <a-form-item label="姓" name="surname">
                  <a-input v-model:value="userForm.surname" placeholder="请输入姓" />
                </a-form-item>
              </a-col>
              <a-col :span="12">
                <a-form-item label="名" name="name">
                  <a-input v-model:value="userForm.name" placeholder="请输入名" />
                </a-form-item>
              </a-col>
            </a-row>
            <a-form-item label="密码" name="password" :required="!editingUser">
              <a-input-password v-model:value="userForm.password" :placeholder="editingUser ? '留空则不修改密码' : '请输入密码'" />
            </a-form-item>
            <a-form-item label="邮箱地址" name="email" required>
              <a-input v-model:value="userForm.email" placeholder="请输入邮箱地址" />
            </a-form-item>
            <a-form-item label="手机号" name="phoneNumber">
              <a-input v-model:value="userForm.phoneNumber" placeholder="请输入手机号" />
            </a-form-item>
            <a-form-item name="isActive">
              <a-checkbox v-model:checked="userForm.isActive">启用</a-checkbox>
            </a-form-item>
            <a-form-item name="lockoutEnabled">
              <a-checkbox v-model:checked="userForm.lockoutEnabled">账户锁定</a-checkbox>
              <a-tooltip title="登录尝试失败后锁定账户" placement="right">
                <InfoCircleOutlined style="margin-left: 8px; color: #999; cursor: pointer" />
              </a-tooltip>
            </a-form-item>
          </a-form>
        </a-tab-pane>
        <a-tab-pane key="roles" tab="角色">
          <a-spin :spinning="rolesLoading">
            <div v-for="role in allRoles" :key="role.id" style="margin-bottom: 8px">
              <a-checkbox
                :checked="selectedRoleNames.includes(role.name)"
                @update:checked="(val: boolean) => handleRoleToggle(role.name, val)"
              >
                {{ role.name }}
              </a-checkbox>
            </div>
          </a-spin>
        </a-tab-pane>
        <a-tab-pane v-if="editingUser" key="details" tab="详细信息">
          <a-descriptions :column="1" bordered size="small">
            <a-descriptions-item label="创建者">
              {{ creatorName || '-' }}
            </a-descriptions-item>
            <a-descriptions-item label="创建时间">
              {{ editingUser.creationTime ? formatDateTime(editingUser.creationTime) : '-' }}
            </a-descriptions-item>
            <a-descriptions-item label="修改者">
              {{ modifierName || '-' }}
            </a-descriptions-item>
            <a-descriptions-item label="修改时间">
              {{ editingUser.lastModificationTime ? formatDateTime(editingUser.lastModificationTime) : '-' }}
            </a-descriptions-item>
            <a-descriptions-item label="密码更新时间">
              {{ editingUser.lastPasswordChangeTime ? formatDateTime(editingUser.lastPasswordChangeTime) : '-' }}
            </a-descriptions-item>
            <a-descriptions-item label="锁定结束时间">
              {{ editingUser.lockoutEnd ? formatDateTime(editingUser.lockoutEnd) : '-' }}
            </a-descriptions-item>
            <a-descriptions-item label="访问失败次数">
              {{ editingUser.accessFailedCount ?? '-' }}
            </a-descriptions-item>
          </a-descriptions>
        </a-tab-pane>
      </a-tabs>
      <template #footer>
        <a-button @click="handleCancelUserModal">取消</a-button>
        <a-button type="primary" :loading="userModalLoading" @click="handleSaveUser">保存</a-button>
      </template>
    </a-modal>

    <!-- 权限管理对话框 -->
    <a-modal
      v-model:open="permModalVisible"
      :title="`权限 - ${permUserName}`"
      :maskClosable="false"
      :closable="false"
    >
      <div style="display: flex; align-items: center; gap: 12px; margin-bottom: 16px">
        <a-input-search
          v-model:value="permSearchText"
          placeholder="搜索"
          style="flex: 1"
          allow-clear
        />
        <a-checkbox
          :checked="permGrantAllChecked"
          :indeterminate="permGrantAllIndeterminate"
          @change="handleGrantAllChange"
        >授予所有权限</a-checkbox>
      </div>
      <a-spin :spinning="permLoading">
        <a-tabs v-model:activeKey="permActiveTab" tab-position="left" style="min-height: 300px">
          <a-tab-pane
            v-for="group in filteredPermGroups"
            :key="group.name"
            :tab="group.displayName"
          >
            <div v-for="perm in group.permissions.filter(p => p.parentName === null)" :key="perm.name">
              <div style="margin-bottom: 8px">
                <a-checkbox
                  :checked="permValues[perm.name]"
                  :disabled="isRoleGranted(perm.name)"
                  @update:checked="(val: boolean) => handlePermToggle(perm, val)"
                >
                  {{ perm.displayName }}
                </a-checkbox>
              </div>
              <div
                v-for="child in getChildPermissions(group.permissions, perm.name)"
                :key="child.name"
                style="padding-left: 24px; margin-bottom: 8px"
              >
                <a-checkbox
                  :checked="permValues[child.name]"
                  :disabled="isRoleGranted(child.name)"
                  @update:checked="(val: boolean) => handlePermToggle(child, val)"
                >
                  {{ child.displayName }}
                </a-checkbox>
                <div
                  v-for="grandchild in getChildPermissions(group.permissions, child.name)"
                  :key="grandchild.name"
                  style="padding-left: 24px; margin-top: 8px"
                >
                  <a-checkbox
                    :checked="permValues[grandchild.name]"
                    :disabled="isRoleGranted(grandchild.name)"
                    @update:checked="(val: boolean) => handlePermToggle(grandchild, val)"
                  >
                    {{ grandchild.displayName }}
                  </a-checkbox>
                </div>
              </div>
            </div>
          </a-tab-pane>
        </a-tabs>
      </a-spin>
      <template #footer>
        <a-button @click="handleCancelPermModal">取消</a-button>
        <a-button type="primary" :loading="permSaving" @click="handleSavePermissions">保存</a-button>
      </template>
    </a-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue';
import { message, Modal } from 'ant-design-vue';
import { PlusOutlined, SettingOutlined, InfoCircleOutlined } from '@ant-design/icons-vue';
import { authService, authState } from '../services/authService';
import { permissionService, Permissions, permissionState } from '../services/permissionService';
import axios from 'axios';

const formatDateTime = (isoString: string): string => {
  const date = new Date(isoString);
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  const h = String(date.getHours()).padStart(2, '0');
  const min = String(date.getMinutes()).padStart(2, '0');
  const s = String(date.getSeconds()).padStart(2, '0');
  return `${y}-${m}-${d} ${h}:${min}:${s}`;
};

const isCurrentUser = (record: any): boolean => {
  return authState.user?.profile?.sub === record.id;
};

// 权限检查
const hasCreatePermission = computed(() => {
  if (!permissionState.loaded) return false;
  return permissionService.hasPermission(Permissions.Identity.UsersCreate);
});

const hasUpdatePermission = computed(() => {
  if (!permissionState.loaded) return false;
  return permissionService.hasPermission(Permissions.Identity.UsersUpdate);
});

const hasDeletePermission = computed(() => {
  if (!permissionState.loaded) return false;
  return permissionService.hasPermission(Permissions.Identity.UsersDelete);
});

const hasManagePermissionsPermission = computed(() => {
  if (!permissionState.loaded) return false;
  return permissionService.hasPermission(Permissions.Identity.UsersManagePermissions);
});

// 表格列定义
const columns = [
  {
    title: '操作',
    dataIndex: 'actions',
    key: 'actions',
    width: 120,
  },
  {
    title: '用户名',
    dataIndex: 'userName',
    key: 'userName',
  },
  {
    title: '邮箱',
    dataIndex: 'email',
    key: 'email',
  },
  {
    title: '手机号',
    dataIndex: 'phoneNumber',
    key: 'phoneNumber',
  },
];

// 表格数据
const tableData = ref<any[]>([]);
const loading = ref(false);

// 分页配置
const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: (total: number) => `共 ${total} 条`,
  showSizeChanger: true,
  pageSizeOptions: ['10', '20', '50'],
});

// 获取用户列表
const fetchUsers = async () => {
  try {
    loading.value = true;
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const skipCount = (pagination.current - 1) * pagination.pageSize;
    const response = await axios.get(`${baseUrl}/api/identity/users`, {
      params: {
        filter: searchText.value || undefined,
        skipCount: skipCount,
        maxResultCount: pagination.pageSize,
      },
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    tableData.value = response.data.items || [];
    pagination.total = response.data.totalCount || 0;
  } catch (error) {
    message.error('获取用户列表失败');
    console.error('获取用户列表失败:', error);
  } finally {
    loading.value = false;
  }
};

// 处理表格分页变化
const handleTableChange = (pag: any) => {
  pagination.current = pag.current;
  pagination.pageSize = pag.pageSize;
  fetchUsers();
};

// 搜索
const searchText = ref('');
const handleSearch = () => {
  pagination.current = 1;
  fetchUsers();
};

// 新建/编辑用户对话框
const userModalVisible = ref(false);
const userModalLoading = ref(false);
const userModalTab = ref('info');
const editingUser = ref<any>(null);
const creatorName = ref('');
const modifierName = ref('');
const userFormRef = ref();
const userForm = reactive({
  userName: '',
  surname: '',
  name: '',
  password: '',
  email: '',
  phoneNumber: '',
  isActive: true,
  lockoutEnabled: true,
});
const userRules = computed(() => ({
  userName: [{ required: true, message: '请输入用户名称', trigger: 'blur' }],
  password: editingUser.value
    ? []
    : [{ required: true, message: '请输入密码', trigger: 'blur' }],
  email: [
    { required: true, message: '请输入邮箱地址', trigger: 'blur' },
    { type: 'email', message: '请输入有效的邮箱地址', trigger: 'blur' },
  ],
}));

// 角色列表
const allRoles = ref<any[]>([]);
const rolesLoading = ref(false);
const selectedRoleNames = ref<string[]>([]);

const fetchAllRoles = async () => {
  try {
    rolesLoading.value = true;
    const user = await authService.getUser();
    if (!user) return;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/identity/roles/all`, {
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    allRoles.value = response.data.items || [];
  } catch (error) {
    message.error('获取角色列表失败');
    console.error('获取角色列表失败:', error);
  } finally {
    rolesLoading.value = false;
  }
};

const handleRoleToggle = (roleName: string, checked: boolean) => {
  if (checked) {
    selectedRoleNames.value = [...selectedRoleNames.value, roleName];
  } else {
    selectedRoleNames.value = selectedRoleNames.value.filter(n => n !== roleName);
  }
};

// 新建用户
const handleCreate = () => {
  editingUser.value = null;
  userModalTab.value = 'info';
  userForm.userName = '';
  userForm.surname = '';
  userForm.name = '';
  userForm.password = '';
  userForm.email = '';
  userForm.phoneNumber = '';
  userForm.isActive = true;
  userForm.lockoutEnabled = true;
  selectedRoleNames.value = [];
  userFormRef.value?.clearValidate();
  userModalVisible.value = true;
  fetchAllRoles();
};

const handleCancelUserModal = () => {
  userModalVisible.value = false;
};

const handleSaveUser = async () => {
  try {
    await userFormRef.value?.validate();
    userModalLoading.value = true;

    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const payload: any = {
      userName: userForm.userName,
      name: userForm.name,
      surname: userForm.surname,
      email: userForm.email,
      phoneNumber: userForm.phoneNumber,
      isActive: userForm.isActive,
      lockoutEnabled: userForm.lockoutEnabled,
      roleNames: selectedRoleNames.value,
    };

    if (editingUser.value) {
      // 编辑模式
      if (userForm.password) {
        payload.password = userForm.password;
      }
      payload.concurrencyStamp = editingUser.value.concurrencyStamp;
      await axios.put(`${baseUrl}/api/identity/users/${editingUser.value.id}`, payload, {
        headers: {
          Authorization: `Bearer ${user.access_token}`,
        },
      });
      message.success('用户更新成功');
    } else {
      // 新建模式
      payload.password = userForm.password;
      await axios.post(`${baseUrl}/api/identity/users`, payload, {
        headers: {
          Authorization: `Bearer ${user.access_token}`,
        },
      });
      message.success('用户创建成功');
    }

    userModalVisible.value = false;
    fetchUsers();
  } catch (error: any) {
    if (error.errorFields) {
      userModalTab.value = 'info';
      return;
    }
    message.error(editingUser.value ? '更新用户失败' : '创建用户失败');
    console.error('保存用户失败:', error);
  } finally {
    userModalLoading.value = false;
  }
};

// 处理操作菜单点击
const handleAction = (key: string, record: any) => {
  switch (key) {
    case 'edit':
      handleEdit(record);
      break;
    case 'permissions':
      handlePermissions(record);
      break;
    case 'delete':
      handleDeleteUser(record);
      break;
  }
};

// 删除用户
const handleDeleteUser = (record: any) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定要删除用户"${record.userName}"吗？`,
    okText: '确定',
    cancelText: '取消',
    onOk: async () => {
      try {
        const user = await authService.getUser();
        if (!user) {
          message.error('未登录，请先登录');
          return;
        }

        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        await axios.delete(`${baseUrl}/api/identity/users/${record.id}`, {
          headers: {
            Authorization: `Bearer ${user.access_token}`,
          },
        });

        message.success('删除成功');
        fetchUsers();
      } catch (error) {
        message.error('删除用户失败');
        console.error('删除用户失败:', error);
      }
    },
  });
};

// 编辑用户
const handleEdit = async (record: any) => {
  userModalTab.value = 'info';
  creatorName.value = '';
  modifierName.value = '';
  userForm.userName = record.userName || '';
  userForm.surname = record.surname || '';
  userForm.name = record.name || '';
  userForm.password = '';
  userForm.email = record.email || '';
  userForm.phoneNumber = record.phoneNumber || '';
  userForm.isActive = record.isActive ?? true;
  userForm.lockoutEnabled = record.lockoutEnabled ?? true;
  userFormRef.value?.clearValidate();
  userModalVisible.value = true;

  // 获取完整用户信息（包含详细信息字段）
  try {
    const user = await authService.getUser();
    if (!user) return;

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const headers = { Authorization: `Bearer ${user.access_token}` };
    const [userDetailRes, userRolesRes] = await Promise.all([
      axios.get(`${baseUrl}/api/identity/users/${record.id}`, { headers }),
      axios.get(`${baseUrl}/api/identity/users/${record.id}/roles`, { headers }),
    ]);

    editingUser.value = userDetailRes.data;
    // 用完整数据更新表单
    userForm.userName = userDetailRes.data.userName || '';
    userForm.surname = userDetailRes.data.surname || '';
    userForm.name = userDetailRes.data.name || '';
    userForm.email = userDetailRes.data.email || '';
    userForm.phoneNumber = userDetailRes.data.phoneNumber || '';
    userForm.isActive = userDetailRes.data.isActive ?? true;
    userForm.lockoutEnabled = userDetailRes.data.lockoutEnabled ?? true;

    selectedRoleNames.value = (userRolesRes.data.items || []).map((r: any) => r.name);

    // 获取创建者和修改者名称
    const fetchUserName = async (userId: string | null): Promise<string> => {
      if (!userId) return '';
      try {
        const res = await axios.get(`${baseUrl}/api/identity/users/${userId}`, { headers });
        return res.data.userName || '';
      } catch {
        return '';
      }
    };

    const detail = userDetailRes.data;
    const [creator, modifier] = await Promise.all([
      fetchUserName(detail.creatorId),
      fetchUserName(detail.lastModifierId),
    ]);
    creatorName.value = creator;
    modifierName.value = modifier;
  } catch (error) {
    message.error('获取用户信息失败');
    console.error('获取用户信息失败:', error);
    selectedRoleNames.value = [];
  }

  fetchAllRoles();
};

// 权限管理
interface PermissionGrantInfo {
  allowedProviders: string[];
  displayName: string;
  grantedProviders: { providerKey: string; providerName: string }[];
  isGranted: boolean;
  name: string;
  parentName: string | null;
}

interface PermissionGroup {
  displayName: string;
  name: string;
  permissions: PermissionGrantInfo[];
}

const permModalVisible = ref(false);
const permUserName = ref('');
const permUserId = ref('');
const permSearchText = ref('');
const permLoading = ref(false);
const permGroups = ref<PermissionGroup[]>([]);
const permValues = reactive<Record<string, boolean>>({});
const permRoleGranted = reactive<Record<string, boolean>>({});
const permActiveTab = ref('');
const permSaving = ref(false);

const isRoleGranted = (permName: string): boolean => {
  return !!permRoleGranted[permName];
};

const fetchPermissions = async (userId: string) => {
  try {
    permLoading.value = true;
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const response = await axios.get(`${baseUrl}/api/permission-management/permissions`, {
      params: {
        providerName: 'U',
        providerKey: userId,
      },
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    permGroups.value = response.data.groups || [];

    Object.keys(permValues).forEach(key => delete permValues[key]);
    Object.keys(permRoleGranted).forEach(key => delete permRoleGranted[key]);
    permGroups.value.forEach(group => {
      group.permissions.forEach(perm => {
        permValues[perm.name] = perm.isGranted;
        permRoleGranted[perm.name] = perm.grantedProviders.some(gp => gp.providerName === 'R');
      });
    });

    if (permGroups.value.length > 0) {
      permActiveTab.value = permGroups.value[0].name;
    }
  } catch (error) {
    message.error('获取权限数据失败');
    console.error('获取权限数据失败:', error);
  } finally {
    permLoading.value = false;
  }
};

const filteredPermGroups = computed(() => {
  if (!permSearchText.value) return permGroups.value;
  const keyword = permSearchText.value.toLowerCase();
  return permGroups.value
    .map(group => {
      const matchedNames = new Set<string>();
      group.permissions.forEach(p => {
        if (p.displayName.toLowerCase().includes(keyword)) {
          matchedNames.add(p.name);
          let parentName = p.parentName;
          while (parentName) {
            matchedNames.add(parentName);
            const parent = group.permissions.find(pp => pp.name === parentName);
            parentName = parent?.parentName || null;
          }
          const addChildren = (name: string) => {
            group.permissions.forEach(cp => {
              if (cp.parentName === name) {
                matchedNames.add(cp.name);
                addChildren(cp.name);
              }
            });
          };
          addChildren(p.name);
        }
      });
      return {
        ...group,
        permissions: group.permissions.filter(p => matchedNames.has(p.name)),
      };
    })
    .filter(group => group.permissions.length > 0);
});

const getChildPermissions = (groupPermissions: PermissionGrantInfo[], parentName: string) => {
  return groupPermissions.filter(p => p.parentName === parentName);
};

const handlePermToggle = (perm: PermissionGrantInfo, checked: boolean) => {
  if (isRoleGranted(perm.name)) return;
  permValues[perm.name] = checked;

  const group = permGroups.value.find(g => g.permissions.some(p => p.name === perm.name));
  if (group) {
    const setChildren = (parentName: string, val: boolean) => {
      group.permissions.forEach(p => {
        if (p.parentName === parentName && !isRoleGranted(p.name)) {
          permValues[p.name] = val;
          setChildren(p.name, val);
        }
      });
    };
    setChildren(perm.name, checked);
  }

  if (checked && perm.parentName) {
    if (!isRoleGranted(perm.parentName)) {
      permValues[perm.parentName] = true;
    }
    const findAndCheckParent = (parentName: string) => {
      for (const grp of permGroups.value) {
        const parent = grp.permissions.find(p => p.name === parentName);
        if (parent && parent.parentName && !isRoleGranted(parent.parentName)) {
          permValues[parent.parentName] = true;
          findAndCheckParent(parent.parentName);
        }
      }
    };
    findAndCheckParent(perm.parentName);
  }
};

const permGrantAllChecked = computed(() => {
  const vals = Object.values(permValues);
  return vals.length > 0 && vals.every(v => v);
});

const permGrantAllIndeterminate = computed(() => {
  const vals = Object.values(permValues);
  if (vals.length === 0) return false;
  const someChecked = vals.some(v => v);
  const allChecked = vals.every(v => v);
  return someChecked && !allChecked;
});

const handleGrantAllChange = (e: any) => {
  const checked = e.target.checked;
  Object.keys(permValues).forEach(key => {
    if (!isRoleGranted(key)) {
      permValues[key] = checked;
    }
  });
};

const handlePermissions = (record: any) => {
  permUserName.value = record.userName;
  permUserId.value = record.id;
  permSearchText.value = '';
  permModalVisible.value = true;
  fetchPermissions(record.id);
};

const handleCancelPermModal = () => {
  permModalVisible.value = false;
};

const handleSavePermissions = async () => {
  try {
    permSaving.value = true;
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const permissions = Object.keys(permValues).map(name => ({
      name,
      isGranted: permValues[name],
    }));

    await axios.put(`${baseUrl}/api/permission-management/permissions`, {
      permissions,
    }, {
      params: {
        providerName: 'U',
        providerKey: permUserId.value,
      },
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    message.success('权限保存成功');
    permModalVisible.value = false;
  } catch (error) {
    message.error('保存权限失败');
    console.error('保存权限失败:', error);
  } finally {
    permSaving.value = false;
  }
};

onMounted(() => {
  fetchUsers();
});
</script>

<style scoped>
.users {
  width: 100%;
}

.users-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
</style>

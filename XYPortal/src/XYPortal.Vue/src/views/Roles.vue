<template>
  <div class="roles">
    <a-card :bordered="false">
      <div class="roles-header">
        <a-typography-title :level="4" style="margin: 0">角色</a-typography-title>
        <a-button
          v-if="hasCreatePermission"
          type="primary"
          style="background-color: #52c41a; border-color: #52c41a"
          @click="handleCreate"
        >
          <template #icon><PlusOutlined /></template>
          新角色
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
            <a-dropdown v-if="hasUpdatePermission || hasManagePermissionsPermission || (!record.isStatic && hasDeletePermission && !isCurrentUserRole(record.name))">
              <a-button type="primary" size="small">
                <template #icon><SettingOutlined /></template>
                操作
              </a-button>
              <template #overlay>
                <a-menu @click="({ key }: { key: string }) => handleAction(key, record)">
                  <a-menu-item v-if="hasUpdatePermission" key="edit">编辑</a-menu-item>
                  <a-menu-item v-if="hasManagePermissionsPermission" key="permissions">权限</a-menu-item>
                  <a-menu-item v-if="!record.isStatic && hasDeletePermission && !isCurrentUserRole(record.name)" key="delete">删除</a-menu-item>
                </a-menu>
              </template>
            </a-dropdown>
          </template>
          <template v-else-if="column.dataIndex === 'name'">
            {{ record.name }}
            <a-tag v-if="record.isDefault" color="blue" style="margin-left: 8px">默认</a-tag>
            <a-tag v-if="record.isPublic" color="green" style="margin-left: 4px">公开</a-tag>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 新建/编辑角色对话框 -->
    <a-modal
      v-model:open="roleModalVisible"
      :title="editingRole ? '编辑' : '新角色'"
      :maskClosable="false"
      :closable="false"
      :confirm-loading="roleModalLoading"
    >
      <a-form
        ref="roleFormRef"
        :model="roleForm"
        :rules="roleRules"
        layout="vertical"
      >
        <a-form-item label="角色名称" name="name" required>
          <a-input v-model:value="roleForm.name" placeholder="请输入角色名称" />
        </a-form-item>
        <a-form-item name="isDefault">
          <a-checkbox v-model:checked="roleForm.isDefault">默认</a-checkbox>
        </a-form-item>
        <a-form-item name="isPublic">
          <a-checkbox v-model:checked="roleForm.isPublic">公开</a-checkbox>
        </a-form-item>
      </a-form>
      <template #footer>
        <a-button @click="handleCancelRoleModal">取消</a-button>
        <a-button type="primary" :loading="roleModalLoading" @click="handleSaveRole">保存</a-button>
      </template>
    </a-modal>

    <!-- 权限管理对话框 -->
    <a-modal
      v-model:open="permModalVisible"
      :title="`权限 - ${permRoleName}`"
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
import { PlusOutlined, SettingOutlined } from '@ant-design/icons-vue';
import { authService } from '../services/authService';
import { permissionService, Permissions, permissionState } from '../services/permissionService';
import axios from 'axios';

interface RoleDto {
  id: string;
  name: string;
  isDefault: boolean;
  isStatic: boolean;
  isPublic: boolean;
  concurrencyStamp: string;
}

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

// 当前用户的角色
const currentUserRoles = ref<string[]>([]);

const isCurrentUserRole = (roleName: string): boolean => {
  return currentUserRoles.value.includes(roleName);
};

// 权限检查
const hasCreatePermission = computed(() => {
  if (!permissionState.loaded) return false;
  return permissionService.hasPermission(Permissions.Identity.RolesCreate);
});

const hasDeletePermission = computed(() => {
  if (!permissionState.loaded) return false;
  return permissionService.hasPermission(Permissions.Identity.RolesDelete);
});

const hasUpdatePermission = computed(() => {
  if (!permissionState.loaded) return false;
  return permissionService.hasPermission(Permissions.Identity.RolesUpdate);
});

const hasManagePermissionsPermission = computed(() => {
  if (!permissionState.loaded) return false;
  return permissionService.hasPermission(Permissions.Identity.RolesManagePermissions);
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
    title: '角色名称',
    dataIndex: 'name',
    key: 'name',
  },
];

// 表格数据
const tableData = ref<RoleDto[]>([]);
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

// 获取角色列表
const fetchRoles = async () => {
  try {
    loading.value = true;
    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const skipCount = (pagination.current - 1) * pagination.pageSize;
    const response = await axios.get(`${baseUrl}/api/identity/roles`, {
      params: {
        SkipCount: skipCount,
        MaxResultCount: pagination.pageSize,
      },
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    tableData.value = response.data.items || [];
    pagination.total = response.data.totalCount || 0;
  } catch (error) {
    message.error('获取角色列表失败');
    console.error('获取角色列表失败:', error);
  } finally {
    loading.value = false;
  }
};

// 处理表格分页变化
const handleTableChange = (pag: any) => {
  pagination.current = pag.current;
  pagination.pageSize = pag.pageSize;
  fetchRoles();
};

// 新建/编辑角色对话框
const roleModalVisible = ref(false);
const roleModalLoading = ref(false);
const roleFormRef = ref();
const editingRole = ref<RoleDto | null>(null);
const roleForm = reactive({
  name: '',
  isDefault: false,
  isPublic: false,
});
const roleRules = {
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
};

// 添加新角色
const handleCreate = () => {
  editingRole.value = null;
  roleForm.name = '';
  roleForm.isDefault = false;
  roleForm.isPublic = false;
  roleFormRef.value?.clearValidate();
  roleModalVisible.value = true;
};

// 编辑角色
const handleEdit = (record: RoleDto) => {
  editingRole.value = record;
  roleForm.name = record.name;
  roleForm.isDefault = record.isDefault;
  roleForm.isPublic = record.isPublic;
  roleFormRef.value?.clearValidate();
  roleModalVisible.value = true;
};

// 取消
const handleCancelRoleModal = () => {
  roleModalVisible.value = false;
};

// 保存角色
const handleSaveRole = async () => {
  try {
    await roleFormRef.value?.validate();
    roleModalLoading.value = true;

    const user = await authService.getUser();
    if (!user) {
      message.error('未登录，请先登录');
      return;
    }

    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const payload = {
      name: roleForm.name,
      isDefault: roleForm.isDefault,
      isPublic: roleForm.isPublic,
    };

    if (editingRole.value) {
      await axios.put(`${baseUrl}/api/identity/roles/${editingRole.value.id}`, {
        ...payload,
        concurrencyStamp: editingRole.value.concurrencyStamp,
      }, {
        headers: {
          Authorization: `Bearer ${user.access_token}`,
        },
      });
      message.success('角色更新成功');
    } else {
      await axios.post(`${baseUrl}/api/identity/roles`, payload, {
        headers: {
          Authorization: `Bearer ${user.access_token}`,
        },
      });
      message.success('角色创建成功');
    }

    roleModalVisible.value = false;
    fetchRoles();
  } catch (error: any) {
    if (error.errorFields) {
      return;
    }
    message.error(editingRole.value ? '更新角色失败' : '创建角色失败');
    console.error('保存角色失败:', error);
  } finally {
    roleModalLoading.value = false;
  }
};

// 处理操作菜单点击
const handleAction = (key: string, record: RoleDto) => {
  switch (key) {
    case 'edit':
      handleEdit(record);
      break;
    case 'permissions':
      handlePermissions(record);
      break;
    case 'delete':
      handleDelete(record);
      break;
  }
};

// 权限管理对话框
const permModalVisible = ref(false);
const permRoleName = ref('');
const permSearchText = ref('');
const permLoading = ref(false);
const permGroups = ref<PermissionGroup[]>([]);
const permValues = reactive<Record<string, boolean>>({});
const permActiveTab = ref('');

// 获取权限数据
const fetchPermissions = async (roleName: string) => {
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
        providerName: 'R',
        providerKey: roleName,
      },
      headers: {
        Authorization: `Bearer ${user.access_token}`,
      },
    });

    permGroups.value = response.data.groups || [];

    // 初始化权限值
    Object.keys(permValues).forEach(key => delete permValues[key]);
    permGroups.value.forEach(group => {
      group.permissions.forEach(perm => {
        permValues[perm.name] = perm.isGranted;
      });
    });

    // 默认选中第一个Tab
    if (permGroups.value.length > 0) {
      permActiveTab.value = permGroups.value[0].name;
    }

    // 判断是否全部授予
  } catch (error) {
    message.error('获取权限数据失败');
    console.error('获取权限数据失败:', error);
  } finally {
    permLoading.value = false;
  }
};

// 过滤后的权限组
const filteredPermGroups = computed(() => {
  if (!permSearchText.value) return permGroups.value;
  const keyword = permSearchText.value.toLowerCase();
  return permGroups.value
    .map(group => {
      // 找出所有 displayName 匹配的权限名称
      const matchedNames = new Set<string>();
      group.permissions.forEach(p => {
        if (p.displayName.toLowerCase().includes(keyword)) {
          matchedNames.add(p.name);
          // 向上保留父链
          let parentName = p.parentName;
          while (parentName) {
            matchedNames.add(parentName);
            const parent = group.permissions.find(pp => pp.name === parentName);
            parentName = parent?.parentName || null;
          }
          // 向下保留子树
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

// 获取某个权限的子权限
const getChildPermissions = (groupPermissions: PermissionGrantInfo[], parentName: string) => {
  return groupPermissions.filter(p => p.parentName === parentName);
};

// 切换单个权限
const handlePermToggle = (perm: PermissionGrantInfo, checked: boolean) => {
  permValues[perm.name] = checked;

  // 如果取消勾选父权限，则子权限也要取消；如果勾选父权限，则子权限也要勾选
  const group = permGroups.value.find(g => g.permissions.some(p => p.name === perm.name));
  if (group) {
    const setChildren = (parentName: string, val: boolean) => {
      group.permissions.forEach(p => {
        if (p.parentName === parentName) {
          permValues[p.name] = val;
          setChildren(p.name, val);
        }
      });
    };
    setChildren(perm.name, checked);
  }

  // 如果勾选子权限，则父权限也要勾选
  if (checked && perm.parentName) {
    permValues[perm.parentName] = true;
    // 递归勾选更上层的父权限
    const findAndCheckParent = (parentName: string) => {
      for (const group of permGroups.value) {
        const parent = group.permissions.find(p => p.name === parentName);
        if (parent && parent.parentName) {
          permValues[parent.parentName] = true;
          findAndCheckParent(parent.parentName);
        }
      }
    };
    findAndCheckParent(perm.parentName);
  }

  // 更新全选状态
};

// 授予所有权限 - 三态 checkbox
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
    permValues[key] = checked;
  });
};

const handlePermissions = (record: RoleDto) => {
  permRoleName.value = record.name;
  permSearchText.value = '';
  permModalVisible.value = true;
  fetchPermissions(record.name);
};

const handleCancelPermModal = () => {
  permModalVisible.value = false;
};

const permSaving = ref(false);

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
        providerName: 'R',
        providerKey: permRoleName.value,
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

// 删除角色
const handleDelete = (record: RoleDto) => {
  Modal.confirm({
    title: '确认删除',
    content: `确定要删除角色"${record.name}"吗？`,
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
        await axios.delete(`${baseUrl}/api/identity/roles/${record.id}`, {
          headers: {
            Authorization: `Bearer ${user.access_token}`,
          },
        });

        message.success('删除成功');
        fetchRoles();
      } catch (error) {
        message.error('删除角色失败');
        console.error('删除角色失败:', error);
      }
    },
  });
};

onMounted(async () => {
  const user = await authService.getUser();
  if (user) {
    const roles = user.profile?.role;
    if (Array.isArray(roles)) {
      currentUserRoles.value = roles;
    } else if (typeof roles === 'string') {
      currentUserRoles.value = [roles];
    }
  }
  fetchRoles();
});
</script>

<style scoped>
.roles {
  width: 100%;
}

.roles-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
</style>

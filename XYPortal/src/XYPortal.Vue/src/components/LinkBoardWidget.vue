<template>
  <div class="linkboard-widget">
    <a-card :loading="loading && !initialized" size="small">
      <template #title>
        <span><LinkOutlined style="margin-right: 8px" />链接板</span>
      </template>
      <template v-if="categories.length > 0">
        <a-tabs v-model:activeKey="activeTab" size="small" :tabBarStyle="{ marginBottom: '0' }" @change="onTabChange">
          <a-tab-pane v-for="category in categories" :key="category.id">
            <template #tab>
              <span>
                <i v-if="category.icon" :class="category.icon" style="margin-right: 4px"></i>
                {{ category.displayName || category.name }}
              </span>
            </template>
            <div class="lb-info-bar">
              <span>共 <strong>{{ getCategoryTotal(category.id) }}</strong> 条链接</span>
              <span v-if="getCategoryTotalPages(category.id) > 1">
                第 {{ getCategoryPage(category.id) }}/{{ getCategoryTotalPages(category.id) }} 页
              </span>
            </div>
            <a-spin :spinning="tabLoading">
              <div class="link-list">
                <a
                  v-for="link in getCategoryLinks(category.id)"
                  :key="link.id"
                  :href="link.url"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="link-item"
                >
                  <span class="link-icon">
                    <i v-if="link.icon" :class="link.icon"></i>
                    <LinkOutlined v-else />
                  </span>
                  <span class="link-content">
                    <span class="link-title">{{ link.title }}</span>
                    <span v-if="link.description" class="link-description">{{ link.description }}</span>
                  </span>
                </a>
                <a-empty v-if="getCategoryLinks(category.id).length === 0 && !tabLoading" description="暂无链接" :image="simpleImage" />
              </div>
            </a-spin>
            <div v-if="getCategoryTotalPages(category.id) > 1" class="lb-pagination">
              <a-button size="small" :disabled="getCategoryPage(category.id) <= 1" @click="goPage(category.id, 1)" title="第一页">
                <template #icon><DoubleLeftOutlined /></template>
              </a-button>
              <a-button size="small" :disabled="getCategoryPage(category.id) <= 1" @click="goPage(category.id, getCategoryPage(category.id) - 1)" title="前一页">
                <template #icon><LeftOutlined /></template>
              </a-button>
              <a-button size="small" :disabled="getCategoryPage(category.id) >= getCategoryTotalPages(category.id)" @click="goPage(category.id, getCategoryPage(category.id) + 1)" title="后一页">
                <template #icon><RightOutlined /></template>
              </a-button>
              <a-button size="small" :disabled="getCategoryPage(category.id) >= getCategoryTotalPages(category.id)" @click="goPage(category.id, getCategoryTotalPages(category.id))" title="最后一页">
                <template #icon><DoubleRightOutlined /></template>
              </a-button>
            </div>
          </a-tab-pane>
        </a-tabs>
      </template>
      <a-empty v-else-if="!loading" description="暂无链接" />
    </a-card>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from 'vue';
import { LinkOutlined, LeftOutlined, RightOutlined, DoubleLeftOutlined, DoubleRightOutlined } from '@ant-design/icons-vue';
import { Empty } from 'ant-design-vue';
import { authService } from '../services/authService';
import axios from 'axios';

const simpleImage = Empty.PRESENTED_IMAGE_SIMPLE;

interface CategoryDto {
  id: string;
  name: string;
  displayName?: string;
  icon?: string;
  sortOrder: number;
}

interface LinkDto {
  id: string;
  categoryId: string;
  title: string;
  url: string;
  description?: string;
  icon?: string;
  sortOrder: number;
}

interface PagedResult {
  totalCount: number;
  items: LinkDto[];
}

interface CategoryPagination {
  links: LinkDto[];
  totalCount: number;
  currentPage: number;
}

const loading = ref(false);
const tabLoading = ref(false);
const initialized = ref(false);
const categories = ref<CategoryDto[]>([]);
const activeTab = ref<string>('');
const pageSize = ref(15);

// Per-category pagination state
const categoryState = reactive<Record<string, CategoryPagination>>({});

const getCategoryLinks = (categoryId: string): LinkDto[] => {
  return categoryState[categoryId]?.links || [];
};

const getCategoryTotal = (categoryId: string): number => {
  return categoryState[categoryId]?.totalCount || 0;
};

const getCategoryPage = (categoryId: string): number => {
  return categoryState[categoryId]?.currentPage || 1;
};

const getCategoryTotalPages = (categoryId: string): number => {
  const total = getCategoryTotal(categoryId);
  return Math.ceil(total / pageSize.value) || 1;
};

const getAccessToken = async () => {
  const user = await authService.getUser();
  return user?.access_token || '';
};

const getHeaders = async () => {
  const token = await getAccessToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
};

const fetchMaxLinks = async () => {
  try {
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const headers = await getHeaders();
    const response = await axios.get<number>(`${baseUrl}/api/app/link/max-links`, { headers });
    const value = response.data;
    if (value && value > 0) {
      pageSize.value = value;
    }
  } catch (error) {
    console.warn('Failed to fetch MaxLinks, using default:', pageSize.value);
  }
};

const fetchCategoryPage = async (categoryId: string, page: number) => {
  const baseUrl = import.meta.env.VITE_API_BASE_URL;
  const headers = await getHeaders();
  const skipCount = (page - 1) * pageSize.value;
  const response = await axios.get<PagedResult>(`${baseUrl}/api/app/link/public-board`, {
    headers,
    params: {
      categoryId,
      skipCount,
      maxResultCount: pageSize.value
    }
  });
  return response.data;
};

const fetchData = async () => {
  loading.value = true;
  try {
    // First fetch MaxLinks from server
    await fetchMaxLinks();
    // Fetch public categories
    const baseUrl = import.meta.env.VITE_API_BASE_URL;
    const headers = await getHeaders();
    const categoriesResponse = await axios.get(`${baseUrl}/api/app/link-category/public-list`, { headers });
    const allCategories: CategoryDto[] = (categoriesResponse.data || [])
      .sort((a: CategoryDto, b: CategoryDto) => a.sortOrder - b.sortOrder);

    // Fetch first page for each category to determine which have links
    const validCategories: CategoryDto[] = [];
    for (const cat of allCategories) {
      const result = await fetchCategoryPage(cat.id, 1);
      if (result.totalCount > 0) {
        validCategories.push(cat);
        categoryState[cat.id] = {
          links: result.items,
          totalCount: result.totalCount,
          currentPage: 1
        };
      }
    }

    categories.value = validCategories;
    if (validCategories.length > 0) {
      activeTab.value = validCategories[0].id;
    }
    initialized.value = true;
  } catch (error) {
    console.error('Failed to fetch linkboard data:', error);
  } finally {
    loading.value = false;
  }
};

const onTabChange = async (key: string) => {
  // If this category hasn't been loaded yet, fetch it
  if (!categoryState[key]) {
    tabLoading.value = true;
    try {
      const result = await fetchCategoryPage(key, 1);
      categoryState[key] = {
        links: result.items,
        totalCount: result.totalCount,
        currentPage: 1
      };
    } catch (error) {
      console.error('Failed to fetch category links:', error);
    } finally {
      tabLoading.value = false;
    }
  }
};

const goPage = async (categoryId: string, page: number) => {
  tabLoading.value = true;
  try {
    const result = await fetchCategoryPage(categoryId, page);
    categoryState[categoryId] = {
      links: result.items,
      totalCount: result.totalCount,
      currentPage: page
    };
  } catch (error) {
    console.error('Failed to fetch page:', error);
  } finally {
    tabLoading.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped>
.linkboard-widget {
  height: 100%;
}

.linkboard-widget :deep(.ant-card) {
  height: 100%;
}

.lb-info-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 12px;
  font-size: 12px;
  color: #999;
  border-bottom: 1px solid #f0f0f0;
}

.link-list {
  display: flex;
  flex-direction: column;
  min-height: 60px;
}

.link-item {
  display: flex;
  align-items: center;
  padding: 8px 12px;
  text-decoration: none;
  color: inherit;
  border-bottom: 1px solid #f0f0f0;
  transition: background-color 0.15s;
}

.link-item:last-child {
  border-bottom: none;
}

.link-item:hover {
  background-color: #e6f7ff;
}

.link-icon {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f5f5;
  border-radius: 6px;
  margin-right: 10px;
  font-size: 16px;
  color: #1890ff;
  flex-shrink: 0;
}

.link-content {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.link-title {
  font-weight: 500;
  color: #333;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 14px;
}

.link-description {
  font-size: 12px;
  color: #999;
  margin-top: 2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.lb-pagination {
  display: flex;
  justify-content: center;
  gap: 4px;
  padding: 8px 12px;
  border-top: 1px solid #f0f0f0;
}
</style>

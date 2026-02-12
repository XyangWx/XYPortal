<template>
  <div class="linkboard-widget">
    <a-spin :spinning="loading">
      <div v-if="categories.length > 0" class="categories-container">
        <div v-for="category in categories" :key="category.id" class="category-section">
          <div class="category-header">
            <i v-if="category.icon" :class="category.icon" style="margin-right: 8px"></i>
            <span>{{ category.displayName || category.name }}</span>
          </div>
          <div class="links-grid">
            <a
              v-for="link in getLinksByCategory(category.id)"
              :key="link.id"
              :href="link.url"
              target="_blank"
              rel="noopener noreferrer"
              class="link-card"
            >
              <div class="link-icon">
                <i v-if="link.icon" :class="link.icon"></i>
                <LinkOutlined v-else />
              </div>
              <div class="link-content">
                <div class="link-title">{{ link.title }}</div>
                <div v-if="link.description" class="link-description">{{ link.description }}</div>
              </div>
            </a>
          </div>
        </div>
      </div>
      <a-empty v-else-if="!loading" description="暂无链接" />
    </a-spin>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { LinkOutlined } from '@ant-design/icons-vue';
import { authService } from '../services/authService';
import axios from 'axios';

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

const loading = ref(false);
const categories = ref<CategoryDto[]>([]);
const links = ref<LinkDto[]>([]);

const getLinksByCategory = (categoryId: string) => {
  return links.value
    .filter(link => link.categoryId === categoryId)
    .sort((a, b) => a.sortOrder - b.sortOrder);
};

const getAccessToken = async () => {
  const user = await authService.getUser();
  return user?.access_token || '';
};

const fetchData = async () => {
  loading.value = true;
  try {
    const token = await getAccessToken();
    const headers = token ? { Authorization: `Bearer ${token}` } : {};
    
    // 获取公开的链接（使用GetPublicBoard接口）
    const linksResponse = await axios.get('/api/app/link/public-board', { headers });
    links.value = linksResponse.data || [];
    
    // 获取公开分类列表
    const categoriesResponse = await axios.get('/api/app/link-category/public-list', { headers });
    
    // 过滤只有链接的分类
    const categoryIdsWithLinks = new Set(links.value.map(l => l.categoryId));
    categories.value = (categoriesResponse.data || [])
      .filter((c: CategoryDto & { isPublic?: boolean }) => categoryIdsWithLinks.has(c.id))
      .sort((a: CategoryDto, b: CategoryDto) => a.sortOrder - b.sortOrder);
  } catch (error) {
    console.error('Failed to fetch linkboard data:', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped>
.linkboard-widget {
  margin-bottom: 24px;
}

.categories-container {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.category-section {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.category-header {
  font-size: 16px;
  font-weight: 600;
  color: #1890ff;
  margin-bottom: 16px;
  padding-bottom: 8px;
  border-bottom: 1px solid #f0f0f0;
}

.links-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 12px;
}

.link-card {
  display: flex;
  align-items: center;
  padding: 12px;
  background: #fafafa;
  border-radius: 6px;
  text-decoration: none;
  color: inherit;
  transition: all 0.2s;
}

.link-card:hover {
  background: #e6f7ff;
  transform: translateY(-2px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.09);
}

.link-icon {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  border-radius: 8px;
  margin-right: 12px;
  font-size: 18px;
  color: #1890ff;
  flex-shrink: 0;
}

.link-content {
  flex: 1;
  min-width: 0;
}

.link-title {
  font-weight: 500;
  color: #333;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.link-description {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>

<template>
  <div class="random-string-widget">
    <a-card size="small">
      <template #title>
        <span>🎲 随机字符串生成器</span>
      </template>
      <a-spin :spinning="loading">
        <div class="mb-3">
          <a-label>生成结果</a-label>
          <a-input-group compact>
            <a-input
              v-model:value="generatedString"
              readonly
              style="width: calc(100% - 80px); font-family: monospace;"
            />
            <a-tooltip title="复制">
              <a-button @click="copyToClipboard">
                <template #icon><CopyOutlined /></template>
              </a-button>
            </a-tooltip>
            <a-tooltip title="生成新字符串">
              <a-button type="primary" @click="generate">
                <template #icon><ReloadOutlined /></template>
              </a-button>
            </a-tooltip>
          </a-input-group>
        </div>
        
        <a-row :gutter="12">
          <a-col :span="8">
            <a-form-item label="前缀" :label-col="{ span: 24 }">
              <a-input v-model:value="prefix" placeholder="可选前缀" />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item label="后缀" :label-col="{ span: 24 }">
              <a-input v-model:value="suffix" placeholder="可选后缀" />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item label="长度" :label-col="{ span: 24 }">
              <a-input-number v-model:value="length" :min="1" :max="100" style="width: 100%" />
            </a-form-item>
          </a-col>
        </a-row>
        
        <a-form-item>
          <a-checkbox v-model:checked="isOnlyOnce">仅唯一字符</a-checkbox>
        </a-form-item>
        
        <a-form-item label="忽略字符">
          <a-input v-model:value="ignoreChars" placeholder="a,b,c (用逗号分隔)" />
          <a-text type="secondary" style="font-size: 12px">多个字符用逗号分隔</a-text>
        </a-form-item>
        
        <a-form-item label="字符类型">
          <a-row :gutter="[8, 8]">
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.lower">小写字母</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.upper">大写字母</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.number">阿拉伯数字</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.punctuation">英文标点</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.chinese">中文大写数字</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.heavenly">天干</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.earthly">地支</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.bagua">八卦</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.hexagrams">六十四卦</a-checkbox>
            </a-col>
            <a-col :span="8">
              <a-checkbox v-model:checked="categories.unicode">Unicode杂项</a-checkbox>
            </a-col>
          </a-row>
        </a-form-item>
      </a-spin>
    </a-card>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from 'vue';
import { CopyOutlined, ReloadOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { authService } from '../services/authService';
import axios from 'axios';

const loading = ref(false);
const generatedString = ref('');
const prefix = ref('');
const suffix = ref('');
const length = ref(12);
const isOnlyOnce = ref(false);
const ignoreChars = ref('');

const categories = reactive({
  lower: true,
  upper: true,
  number: true,
  punctuation: true,
  chinese: false,
  heavenly: false,
  earthly: false,
  bagua: false,
  hexagrams: false,
  unicode: false,
});

// Flag values matching RandomCategory enum
const categoryFlags = {
  lower: 1,        // 1 << 0
  upper: 2,       // 1 << 1
  number: 4,      // 1 << 2
  punctuation: 8, // 1 << 3
  chinese: 16,    // 1 << 4
  heavenly: 32,   // 1 << 5
  earthly: 64,    // 1 << 6
  bagua: 128,    // 1 << 7
  hexagrams: 256, // 1 << 8
  unicode: 512,   // 1 << 9
};

const getAccessToken = async () => {
  const user = await authService.getUser();
  return user?.access_token || '';
};

const getHeaders = async () => {
  const token = await getAccessToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
};

const calculateSymbolCategories = (): number => {
  let result = 0;
  for (const [key, flag] of Object.entries(categoryFlags)) {
    if (categories[key as keyof typeof categories]) {
      result |= flag;
    }
  }
  return result;
};

const generate = async () => {
  const symbolCategories = calculateSymbolCategories();
  if (symbolCategories === 0) {
    message.warning('请至少选择一个字符类型');
    return;
  }

  // Parse ignoreChars - split by comma and filter empty
  let ignoreCharsList: string[] | null = null;
  if (ignoreChars.value.trim()) {
    ignoreCharsList = ignoreChars.value.split(',').map(c => c.trim()).filter(c => c.length > 0);
  }

  loading.value = true;
  try {
    const headers = await getHeaders();
    const response = await axios.post('/api/app/random-string/make', {
      prefix: prefix.value || null,
      suffix: suffix.value || null,
      length: length.value,
      ignoreChars: ignoreCharsList,
      isOnlyOnce: isOnlyOnce.value,
      symbolCategories: symbolCategories,
    }, {
      headers: {
        ...headers,
        'Content-Type': 'application/json',
      },
    });
    generatedString.value = response.data;
    message.success('生成成功！');
  } catch (error: any) {
    console.error('Failed to generate random string:', error);
    message.error(error.response?.data?.message || '生成失败');
  } finally {
    loading.value = false;
  }
};

const copyToClipboard = async () => {
  if (!generatedString.value) return;
  
  try {
    await navigator.clipboard.writeText(generatedString.value);
    message.success('已复制到剪贴板！');
  } catch (error) {
    console.error('Failed to copy:', error);
    message.error('复制失败');
  }
};

onMounted(() => {
  generate();
});
</script>

<style scoped>
.random-string-widget {
  height: 100%;
}

.random-string-widget :deep(.ant-card) {
  height: 100%;
}

.random-string-widget :deep(.ant-form-item) {
  margin-bottom: 12px;
}

.random-string-widget :deep(.ant-form-item-label) {
  padding-bottom: 4px;
}

.random-string-widget :deep(.ant-form-item-label > label) {
  font-size: 13px;
}
</style>

import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { viteStaticCopy } from 'vite-plugin-static-copy'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd());

  return {
    plugins: [
      vue(),
      // silent-renew.html 引用 /oidc-client.min.js(无 -ts 后缀),
      // 而 oidc-client-ts@3.x 真实产物在 dist/browser/ 下、文件名带 -ts。
      // 用 viteStaticCopy 拷一份并重命名为 silent-renew.html 期望的名字,
      // 否则静默续期 iframe 启动即 404,silent renew 全程失败。
      viteStaticCopy({
        targets: [
          {
            src: 'node_modules/oidc-client-ts/dist/browser/oidc-client-ts.min.js',
            dest: '.',
            rename: 'oidc-client.min.js',
          },
        ],
      }),
    ],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src'),
      },
    },
    server: {
      host: mode === 'development' ? '0.0.0.0' : 'localhost',
      port: 3000,
      proxy: {
        '/api': {
          target: env.VITE_API_BASE_URL,
          changeOrigin: true
        }
      }
    }
  }
})

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Backend dev server URL - use loopback IP to avoid localhost resolution issues
const backend = 'http://127.0.0.1:5120';

export default defineConfig({
  plugins: [react()],
  server: {
    host: '127.0.0.1',
    open: false,
    proxy: {
      // Proxy API calls under /robot/ (trailing slash) so static assets like /robot.png are NOT proxied
      '/robot/': {
        target: backend,
        changeOrigin: true,
      },
    },
  },
})

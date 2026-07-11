import path from 'path'
import { defineConfig } from 'vite'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'
import tailwindcss from '@tailwindcss/vite'

// Aspire injects service URLs as env vars
const apiHttps = process.env['services__scrumpoker-api__https__0']
const apiHttp = process.env['services__scrumpoker-api__http__0']
const apiTarget = apiHttps || apiHttp || 'https://localhost:7234'

// https://vite.dev/config/
export default defineConfig({
  base: '/scrum-poker/',
  plugins: [
    react(),
    babel({ presets: [reactCompilerPreset()] }),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    host: true,
    proxy: {
      '/api/hub': {
        target: apiTarget,
        ws: true, // SignalR WebSocket
        secure: false,
      },
      '/api': {
        target: apiTarget,
        secure: false, // Aspire uses self-signed certs
      },
    },
  },
})

/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Where `npm run dev` forwards /api requests. Matches the http profile in
// OlapApi/Properties/launchSettings.json.
const apiUrl = process.env.OLAP_API_URL ?? 'http://localhost:5298'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': apiUrl,
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  },
})

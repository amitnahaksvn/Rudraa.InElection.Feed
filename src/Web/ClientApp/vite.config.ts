import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Build output goes straight into the ASP.NET Core Web project's wwwroot (served via
// app.UseStaticFiles(), see Program.cs) so the whole app is one deployable unit - no separate
// static hosting/CORS to configure. The dev server proxies /api to the real backend (see
// launchSettings.json's http profile) so `npm run dev` talks to live data without CORS either.
export default defineConfig({
  plugins: [react()],
  base: '/',
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5096',
    },
  },
})

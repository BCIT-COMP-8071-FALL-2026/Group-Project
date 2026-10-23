declare global {
  interface ImportMetaEnv {
    /**
     * Where the browser reaches OlapApi. Set at build time: .env.development for `npm run dev`,
     * the VITE_API_BASE_URL build argument in Docker. Unset means the same origin as the client.
     */
    readonly VITE_API_BASE_URL?: string
  }
}

/** Builds an OlapApi URL from the configured base URL, e.g. apiUrl('/api/reports/x'). */
export function apiUrl(path: string): string {
  const baseUrl = import.meta.env.VITE_API_BASE_URL ?? ''
  return baseUrl.replace(/\/+$/, '') + path
}

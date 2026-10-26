import { useSyncExternalStore } from 'react'

function subscribe(onChange: () => void) {
  window.addEventListener('hashchange', onChange)
  return () => window.removeEventListener('hashchange', onChange)
}

// Hash routes (#/late-check-ins) need no server rewrites, so a refresh works wherever the static build is served.
function currentRoute() {
  return window.location.hash.replace(/^#\/?/, '')
}

/** The path after "#/" in the URL, e.g. "late-check-ins", or "" when there is none. */
export function useHashRoute(): string {
  return useSyncExternalStore(subscribe, currentRoute)
}

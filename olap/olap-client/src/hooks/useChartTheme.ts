import { useSyncExternalStore } from 'react'

export interface ChartTheme {
  surface: string
  textPrimary: string
  textSecondary: string
  textMuted: string
  grid: string
  baseline: string
  border: string
  series: string
  seriesHover: string
}

const darkScheme = '(prefers-color-scheme: dark)'

function subscribe(onChange: () => void) {
  const media = window.matchMedia(darkScheme)
  media.addEventListener('change', onChange)
  return () => media.removeEventListener('change', onChange)
}

// Chart.js draws on a canvas, so it can't use CSS variables directly. Read the
// tokens from index.css and re-read them whenever the OS color scheme flips.
function readTheme(): ChartTheme {
  const styles = getComputedStyle(document.documentElement)
  const token = (name: string) => styles.getPropertyValue(name).trim()

  return {
    surface: token('--surface'),
    textPrimary: token('--text-primary'),
    textSecondary: token('--text-secondary'),
    textMuted: token('--text-muted'),
    grid: token('--grid'),
    baseline: token('--baseline'),
    border: token('--border'),
    series: token('--series-1'),
    seriesHover: token('--series-1-hover'),
  }
}

let cached: { dark: boolean; theme: ChartTheme } | undefined

// useSyncExternalStore needs the same object back until the scheme changes.
function getTheme(): ChartTheme {
  const dark = window.matchMedia(darkScheme).matches
  if (cached?.dark !== dark) cached = { dark, theme: readTheme() }
  return cached.theme
}

export function useChartTheme(): ChartTheme {
  return useSyncExternalStore(subscribe, getTheme)
}

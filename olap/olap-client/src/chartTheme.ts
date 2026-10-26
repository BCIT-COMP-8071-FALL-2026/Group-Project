export interface ChartTheme {
  fontFamily: string
  surface: string
  textPrimary: string
  textSecondary: string
  textMuted: string
  divider: string
  grid: string
  baseline: string
  successful: string
  missed: string
  bar: string
  barHover: string
}

// Chart.js draws on a canvas, so it can't use CSS variables directly. Read the tokens from index.css.
export function readChartTheme(): ChartTheme {
  const styles = getComputedStyle(document.documentElement)
  const token = (name: string) => styles.getPropertyValue(name).trim()

  return {
    fontFamily: token('--sans'),
    surface: token('--surface'),
    textPrimary: token('--text-primary'),
    textSecondary: token('--text-secondary'),
    textMuted: token('--text-muted'),
    divider: token('--divider'),
    grid: token('--grid'),
    baseline: token('--baseline'),
    successful: token('--series-successful'),
    missed: token('--series-missed'),
    bar: token('--series-bar'),
    barHover: token('--series-bar-hover'),
  }
}

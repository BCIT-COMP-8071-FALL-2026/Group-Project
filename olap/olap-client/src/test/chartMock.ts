/** What a chart's props look like to the tests. */
export interface MockChartProps {
  data: { labels?: unknown[]; datasets: { label?: string; data: unknown[] }[] }
  'aria-label'?: string
}

/**
 * jsdom can't draw on a canvas, so every chart renders as MockChart (setup.ts), which records what the last
 * chart was given here, with a stand-in for its Chart.js instance (used by the print resizing).
 */
export const lastChart = {
  props: undefined as MockChartProps | undefined,
  instance: {
    options: {} as { devicePixelRatio?: number },
    resize: (() => {}) as (width?: number, height?: number) => void,
  },
}

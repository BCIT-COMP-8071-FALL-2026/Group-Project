/**
 * Chart.js sizes the canvas to its container, and print layout doesn't resize it, so the chart is redrawn
 * at this size (and a high pixel ratio, for sharp lines on paper) for printing. Print CSS then scales it
 * to the page width.
 */
export const CHART_PRINT_SIZE = { width: 700, height: 240, devicePixelRatio: 3 }

// Charts are redrawn at these sizes while printing (see hooks/useChartPrintSize.ts), and print CSS then
// scales them to the page width. The sizes leave room for the rest of each report on one Letter page.

/** A high pixel ratio keeps lines and text sharp on paper. */
export const PRINT_PIXEL_RATIO = 3

export const MISSED_VISITS_PRINT_SIZE = { width: 700, height: 240 }

/** Taller than the line chart, for the caregiver names drawn at an angle under the bars. */
export const LATE_CHECK_INS_PRINT_SIZE = { width: 700, height: 340 }

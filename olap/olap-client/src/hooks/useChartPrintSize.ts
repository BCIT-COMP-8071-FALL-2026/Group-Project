import type { Chart } from 'chart.js'
import { useEffect, type RefObject } from 'react'
import { PRINT_PIXEL_RATIO } from '../chartPrintSize'

export interface PrintSize {
  width: number
  height: number
}

/**
 * Chart.js sizes the canvas to its container, and print layout doesn't resize it, so the chart is redrawn
 * at `size` while printing. Print CSS then scales it to the page width.
 */
export function useChartPrintSize(chartRef: RefObject<Chart | null>, size: PrintSize) {
  const { width, height } = size

  useEffect(() => {
    // Unset by default, which makes Chart.js follow the screen's pixel ratio.
    let screenRatio: number | undefined
    const beforePrint = () => {
      const chart = chartRef.current
      if (!chart) return
      screenRatio = chart.options.devicePixelRatio
      chart.options.devicePixelRatio = PRINT_PIXEL_RATIO
      chart.resize(width, height)
    }
    const afterPrint = () => {
      const chart = chartRef.current
      if (!chart) return
      chart.options.devicePixelRatio = screenRatio as number
      chart.resize()
    }
    window.addEventListener('beforeprint', beforePrint)
    window.addEventListener('afterprint', afterPrint)
    return () => {
      window.removeEventListener('beforeprint', beforePrint)
      window.removeEventListener('afterprint', afterPrint)
    }
  }, [chartRef, width, height])
}

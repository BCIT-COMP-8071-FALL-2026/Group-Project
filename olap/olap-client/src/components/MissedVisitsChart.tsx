import {
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  LineElement,
  PointElement,
  Tooltip,
  type ChartData,
  type ChartOptions,
  type Plugin,
} from 'chart.js'
import { useEffect, useMemo, useRef } from 'react'
import { Line } from 'react-chartjs-2'
import type { MissedVisitsMonth } from '../api/missedVisits'
import { CHART_PRINT_SIZE } from '../chartPrintSize'
import { readChartTheme } from '../chartTheme'
import {
  formatAxisMonth,
  formatCount,
  formatMonth,
  formatMonthRange,
  formatPercent,
  isCurrentMonth,
} from '../format'

ChartJS.register(CategoryScale, LinearScale, LineElement, PointElement, Tooltip)

// Room on the right for the series names drawn at the end of each line. "Successful" is about 64px
// in Segoe UI (Windows), plus the 10px gap, with a margin to spare.
const END_LABEL_SPACE = 88

export function MissedVisitsChart({ months }: { months: MissedVisitsMonth[] }) {
  const chartRef = useRef<ChartJS<'line'>>(null)
  const theme = useMemo(() => readChartTheme(), [])

  useEffect(() => {
    // Unset by default, which makes Chart.js follow the screen's pixel ratio.
    let screenRatio: number | undefined
    const beforePrint = () => {
      const chart = chartRef.current
      if (!chart) return
      screenRatio = chart.options.devicePixelRatio
      chart.options.devicePixelRatio = CHART_PRINT_SIZE.devicePixelRatio
      chart.resize(CHART_PRINT_SIZE.width, CHART_PRINT_SIZE.height)
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
  }, [])

  const data = useMemo<ChartData<'line'>>(() => {
    // The current month is still filling up, so it gets a hollow point and a dashed segment
    // instead of reading as a drop.
    const inProgress = (index: number) => isCurrentMonth(months[index].month)
    const series = (label: string, color: string, values: number[]) => {
      const fill = (context: { dataIndex: number }) => (inProgress(context.dataIndex) ? theme.surface : color)
      const ring = (context: { dataIndex: number }) => (inProgress(context.dataIndex) ? color : theme.surface)
      return {
        label,
        data: values,
        borderColor: color,
        backgroundColor: color,
        borderWidth: 2,
        borderCapStyle: 'round' as const,
        borderJoinStyle: 'round' as const,
        pointRadius: 5,
        pointHoverRadius: 6,
        pointBorderWidth: 2,
        pointHoverBorderWidth: 2,
        pointBackgroundColor: fill,
        pointHoverBackgroundColor: fill,
        pointBorderColor: ring,
        pointHoverBorderColor: ring,
        segment: {
          borderDash: (context: { p1DataIndex: number }) => (inProgress(context.p1DataIndex) ? [4, 4] : undefined),
        },
      }
    }

    return {
      labels: months.map((month) => month.month),
      datasets: [
        series('Successful', theme.successful, months.map((month) => month.visitCount - month.missedCount)),
        series('Missed', theme.missed, months.map((month) => month.missedCount)),
      ],
    }
  }, [months, theme])

  const options = useMemo<ChartOptions<'line'>>(
    () => ({
      responsive: true,
      maintainAspectRatio: false,
      animation: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? false : undefined,
      // Hovering anywhere in a month's column shows that month for both lines.
      interaction: { mode: 'index', intersect: false },
      layout: { padding: { top: 8, right: END_LABEL_SPACE } },
      scales: {
        x: {
          grid: { display: false },
          border: { color: theme.baseline },
          ticks: {
            color: theme.textSecondary,
            font: { family: theme.fontFamily, size: 12 },
            maxRotation: 0,
            autoSkipPadding: 4,
            callback: (_value, index) => formatAxisMonth(months[index].month, index),
          },
        },
        y: {
          beginAtZero: true,
          grid: { color: theme.grid, drawTicks: false },
          border: { display: false },
          ticks: {
            color: theme.textMuted,
            font: { family: theme.fontFamily, size: 12 },
            precision: 0,
            padding: 8,
            maxTicksLimit: 6,
          },
        },
      },
      plugins: {
        tooltip: {
          backgroundColor: theme.surface,
          borderColor: theme.divider,
          borderWidth: 1,
          cornerRadius: 8,
          padding: 10,
          // Chart.js draws a key as an outline plus an inner fill (1px in on each side). With a transparent
          // outline that leaves a solid 14 x 3 line key, matching the legend.
          boxWidth: 16,
          boxHeight: 5,
          boxPadding: 5,
          titleColor: theme.textPrimary,
          titleFont: { family: theme.fontFamily, weight: 600, size: 13 },
          bodyColor: theme.textPrimary,
          bodyFont: { family: theme.fontFamily, size: 13 },
          footerColor: theme.textSecondary,
          footerFont: { family: theme.fontFamily, weight: 400, size: 12 },
          footerMarginTop: 8,
          callbacks: {
            title: (items) => {
              const month = months[items[0].dataIndex].month
              return isCurrentMonth(month) ? `${formatMonth(month)} (to date)` : formatMonth(month)
            },
            // Value first, then the series.
            label: (item) => `${formatCount(item.parsed.y ?? 0)} ${item.dataset.label?.toLowerCase()}`,
            labelColor: (item) => ({
              borderColor: 'transparent',
              backgroundColor: item.dataset.borderColor as string,
            }),
            footer: (items) => {
              const month = months[items[0].dataIndex]
              return `${formatPercent(month.missedPercent)} of ${formatCount(month.visitCount)} visits missed`
            },
          },
        },
      },
    }),
    [months, theme],
  )

  const plugins = useMemo(() => [crosshair(theme.baseline), endLabels(theme.textSecondary, theme.fontFamily)], [theme])

  const percents = months.map((month) => month.missedPercent)
  const summary =
    `Line chart of successful and missed visits per month, ` +
    `${formatMonthRange(months[0].month, months[months.length - 1].month)}. ` +
    `Between ${formatPercent(Math.min(...percents))} and ${formatPercent(Math.max(...percents))} ` +
    `of visits were missed each month. The table below lists every month.`

  return (
    <div className="chart-frame">
      <Line ref={chartRef} data={data} options={options} plugins={plugins} role="img" aria-label={summary} />
    </div>
  )
}

/** A vertical hairline at the hovered month, drawn behind the lines. */
function crosshair(color: string): Plugin<'line'> {
  return {
    id: 'crosshair',
    beforeDatasetsDraw(chart) {
      const [active] = chart.tooltip?.getActiveElements() ?? []
      if (!active) return
      const { ctx, chartArea } = chart
      const x = Math.round(active.element.x) + 0.5
      ctx.save()
      ctx.strokeStyle = color
      ctx.lineWidth = 1
      ctx.beginPath()
      ctx.moveTo(x, chartArea.top)
      ctx.lineTo(x, chartArea.bottom)
      ctx.stroke()
      ctx.restore()
    },
  }
}

/** Each series' name at the end of its line, so identity never relies on color alone. */
function endLabels(color: string, fontFamily: string): Plugin<'line'> {
  return {
    id: 'endLabels',
    afterDatasetsDraw(chart) {
      const { ctx } = chart
      ctx.save()
      ctx.font = `500 12px ${fontFamily}`
      ctx.fillStyle = color
      ctx.textBaseline = 'middle'
      chart.data.datasets.forEach((dataset, index) => {
        const last = chart.getDatasetMeta(index).data.at(-1)
        if (last && dataset.label) ctx.fillText(dataset.label, last.x + 10, last.y)
      })
      ctx.restore()
    },
  }
}

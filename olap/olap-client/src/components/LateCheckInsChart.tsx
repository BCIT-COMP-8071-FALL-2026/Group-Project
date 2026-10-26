import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  Tooltip,
  type ChartData,
  type ChartOptions,
} from 'chart.js'
import { useMemo, useRef } from 'react'
import { Bar } from 'react-chartjs-2'
import type { CaregiverLateness } from '../api/reports'
import { LATE_CHECK_INS_PRINT_SIZE } from '../chartPrintSize'
import { readChartTheme } from '../chartTheme'
import { formatCount, formatMinutes } from '../format'
import { useChartPrintSize } from '../hooks/useChartPrintSize'

ChartJS.register(BarElement, CategoryScale, LinearScale, Tooltip)

/** OLAP-2: one bar per caregiver, caregiver names along the x axis, average minutes late up the y axis. */
export function LateCheckInsChart({ caregivers }: { caregivers: CaregiverLateness[] }) {
  const chartRef = useRef<ChartJS<'bar'>>(null)
  const theme = useMemo(() => readChartTheme(), [])

  useChartPrintSize(chartRef, LATE_CHECK_INS_PRINT_SIZE)

  const data = useMemo<ChartData<'bar'>>(
    () => ({
      labels: caregivers.map((caregiver) => caregiver.caregiverName),
      datasets: [
        {
          label: 'Average minutes late',
          data: caregivers.map((caregiver) => caregiver.avgMinutesLate),
          backgroundColor: theme.bar,
          hoverBackgroundColor: theme.barHover,
          borderRadius: 4,
          borderSkipped: 'start',
          maxBarThickness: 24,
        },
      ],
    }),
    [caregivers, theme],
  )

  const options = useMemo<ChartOptions<'bar'>>(() => {
    const font = { family: theme.fontFamily, size: 12 }
    return {
      responsive: true,
      maintainAspectRatio: false,
      animation: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? false : undefined,
      // The whole column under a name is the hover target, not just the bar.
      interaction: { mode: 'index', intersect: false },
      layout: { padding: { top: 8 } },
      scales: {
        x: {
          grid: { display: false },
          border: { color: theme.baseline },
          // Names turn upright when they don't fit side by side; on a narrow screen some are skipped
          // (the table view lists every caregiver).
          ticks: { color: theme.textSecondary, font, autoSkip: true, autoSkipPadding: 2, maxRotation: 90 },
        },
        y: {
          beginAtZero: true,
          grid: { color: theme.grid, drawTicks: false },
          border: { display: false },
          ticks: { color: theme.textMuted, font, precision: 0, padding: 8, maxTicksLimit: 7 },
          title: { display: true, text: 'Average minutes late', color: theme.textSecondary, font },
        },
      },
      plugins: {
        tooltip: {
          backgroundColor: theme.surface,
          borderColor: theme.divider,
          borderWidth: 1,
          cornerRadius: 8,
          padding: 10,
          displayColors: false,
          titleColor: theme.textPrimary,
          titleFont: { family: theme.fontFamily, weight: 600, size: 13 },
          bodyColor: theme.textPrimary,
          bodyFont: { family: theme.fontFamily, size: 13 },
          callbacks: {
            title: (items) => caregivers[items[0].dataIndex].caregiverName,
            label: (item) => `${formatMinutes(item.parsed.y ?? 0)} minutes late on average`,
          },
        },
      },
    }
  }, [caregivers, theme])

  const latest = caregivers.reduce((max, caregiver) => (caregiver.avgMinutesLate > max.avgMinutesLate ? caregiver : max))
  const summary =
    `Bar chart of average minutes late for ${formatCount(caregivers.length)} ` +
    `${caregivers.length === 1 ? 'caregiver' : 'caregivers'} with late check-ins. ` +
    `${latest.caregiverName} is the latest, at ${formatMinutes(latest.avgMinutesLate)} minutes on average. ` +
    `The table view lists every caregiver.`

  return (
    <div className="chart-frame chart-frame-tall">
      <Bar ref={chartRef} data={data} options={options} role="img" aria-label={summary} />
    </div>
  )
}

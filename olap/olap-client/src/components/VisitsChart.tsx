import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  Tooltip,
  type ChartData,
  type ChartOptions,
} from 'chart.js'
import { useMemo } from 'react'
import { Bar } from 'react-chartjs-2'
import type { SeniorVisit } from '../api/seniorVisits'
import { formatVisits } from '../format'
import { useChartTheme } from '../hooks/useChartTheme'

ChartJS.register(BarElement, CategoryScale, LinearScale, Tooltip)
ChartJS.defaults.font.family = 'system-ui, -apple-system, "Segoe UI", sans-serif'
ChartJS.defaults.font.size = 12

export function VisitsChart({ visits }: { visits: SeniorVisit[] }) {
  const theme = useChartTheme()

  const data = useMemo<ChartData<'bar'>>(
    () => ({
      labels: visits.map((visit) => visit.seniorName),
      datasets: [
        {
          label: 'Visits',
          data: visits.map((visit) => visit.visitCount),
          backgroundColor: theme.series,
          hoverBackgroundColor: theme.seriesHover,
          borderRadius: 4,
          borderSkipped: 'start',
          maxBarThickness: 24,
        },
      ],
    }),
    [visits, theme],
  )

  const options = useMemo<ChartOptions<'bar'>>(
    () => ({
      responsive: true,
      maintainAspectRatio: false,
      // The whole category band is the hover target, not just the 24px bar.
      interaction: { mode: 'index', intersect: false },
      scales: {
        x: {
          grid: { display: false },
          border: { color: theme.baseline },
          ticks: { color: theme.textSecondary },
        },
        y: {
          beginAtZero: true,
          grid: { color: theme.grid, drawTicks: false },
          border: { display: false },
          ticks: { color: theme.textMuted, precision: 0, padding: 8 },
        },
      },
      plugins: {
        tooltip: {
          backgroundColor: theme.surface,
          borderColor: theme.border,
          borderWidth: 1,
          cornerRadius: 6,
          padding: 10,
          displayColors: false,
          titleColor: theme.textPrimary,
          titleFont: { weight: 600, size: 13 },
          bodyColor: theme.textSecondary,
          callbacks: {
            // Value first, then the senior's name.
            title: (items) => formatVisits(Number(items[0]?.raw ?? 0)),
            label: (item) => item.label,
          },
        },
      },
    }),
    [theme],
  )

  const summary = visits
    .map((visit) => `${visit.seniorName}: ${formatVisits(visit.visitCount)}`)
    .join(', ')

  return (
    <div className="chart-frame">
      <Bar data={data} options={options} role="img" aria-label={`Bar chart of visits per senior. ${summary}.`} />
    </div>
  )
}

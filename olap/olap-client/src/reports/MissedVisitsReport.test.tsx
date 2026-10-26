import { act, render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { MissedVisitsMonth } from '../api/reports'
import { MISSED_VISITS_PRINT_SIZE, PRINT_PIXEL_RATIO } from '../chartPrintSize'
import { lastChart } from '../test/chartMock'
import { mockApi } from '../test/mockApi'
import { MissedVisitsReport } from './MissedVisitsReport'

const months: MissedVisitsMonth[] = [
  { month: '2025-10', visitCount: 120, missedCount: 9, missedPercent: 7.5 },
  { month: '2025-11', visitCount: 131, missedCount: 12, missedPercent: 9.2 },
  { month: '2025-12', visitCount: 1250, missedCount: 210, missedPercent: 16.8 },
]

function renderReport() {
  render(<MissedVisitsReport title="Missed visits by month" />)
  return screen.findByRole('img', { name: /line chart of successful and missed visits/i })
}

describe('Missed visits report (Query 1)', () => {
  beforeEach(() => {
    // Only Date is faked, so the "current month" is fixed while promises and user events run normally.
    vi.useFakeTimers({ toFake: ['Date'] })
    vi.setSystemTime(new Date(2026, 9, 13, 10, 30))
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('loads Query 1 from the configured OlapApi URL', async () => {
    vi.stubEnv('VITE_API_BASE_URL', 'http://api.test/')
    const fetchMock = mockApi(Response.json(months))

    await renderReport()

    expect(fetchMock).toHaveBeenCalledWith('http://api.test/api/reports/missed-visits-by-month', expect.anything())
  })

  it('calls the API on its own origin when no URL is configured', async () => {
    vi.stubEnv('VITE_API_BASE_URL', undefined)
    const fetchMock = mockApi(Response.json(months))

    await renderReport()

    expect(fetchMock).toHaveBeenCalledWith('/api/reports/missed-visits-by-month', expect.anything())
  })

  it('charts successful and missed visits per month', async () => {
    mockApi(Response.json(months))

    const chartImage = await renderReport()

    expect(lastChart.props?.data.labels).toEqual(['2025-10', '2025-11', '2025-12'])
    expect(lastChart.props?.data.datasets.map((dataset) => [dataset.label, dataset.data])).toEqual([
      ['Successful', [111, 119, 1040]],
      ['Missed', [9, 12, 210]],
    ])
    expect(chartImage).toHaveAccessibleName(
      'Line chart of successful and missed visits per month, October 2025 to December 2025. ' +
        'Between 7.5% and 16.8% of visits were missed each month. The table below lists every month.',
    )
  })

  it('lists the same months in the table', async () => {
    mockApi(Response.json(months))
    await renderReport()

    const table = screen.getByRole('table', { name: 'Visits, successful visits and missed visits per month' })
    const [header, ...rows] = within(table).getAllByRole('row')

    expect(within(header).getAllByRole('columnheader').map((cell) => cell.textContent)).toEqual([
      'Month',
      'Visits',
      'Successful',
      'Missed',
      'Missed %',
    ])
    expect(rows.map((row) => [...(row as HTMLTableRowElement).cells].map((cell) => cell.textContent))).toEqual([
      ['October 2025', '120', '111', '9', '7.5%'],
      ['November 2025', '131', '119', '12', '9.2%'],
      ['December 2025', '1,250', '1,040', '210', '16.8%'],
    ])
  })

  it('uses short month names in the table on a phone', async () => {
    const matchMedia = window.matchMedia
    vi.spyOn(window, 'matchMedia').mockImplementation((query) => ({
      ...matchMedia(query),
      matches: query === '(max-width: 600px)',
    }))
    mockApi(Response.json(months))
    await renderReport()

    expect(screen.getAllByRole('rowheader').map((cell) => cell.textContent)).toEqual([
      'Oct 2025',
      'Nov 2025',
      'Dec 2025',
    ])
  })

  it('marks the current month as still in progress', async () => {
    mockApi(Response.json([...months, { month: '2026-10', visitCount: 45, missedCount: 2, missedPercent: 4.4 }]))
    await renderReport()

    expect(screen.getByRole('rowheader', { name: 'October 2026 (to date)' })).toBeInTheDocument()
    expect(screen.getByRole('rowheader', { name: 'October 2025' })).toBeInTheDocument()
  })

  it('shows the period the report covers and when it was generated', async () => {
    mockApi(Response.json(months))
    await renderReport()

    expect(screen.getByText(/^October 2025 to December 2025/)).toHaveTextContent(
      'October 2025 to December 2025 · Generated Oct 13, 2026, 10:30 a.m.',
    )
  })

  it('prints the report', async () => {
    const print = vi.spyOn(window, 'print').mockImplementation(() => {})
    mockApi(Response.json(months))
    await renderReport()

    await userEvent.click(screen.getByRole('button', { name: 'Print' }))

    expect(print).toHaveBeenCalledOnce()
  })

  it('redraws the chart at print size while printing, then restores it', async () => {
    mockApi(Response.json(months))
    await renderReport()

    act(() => {
      window.dispatchEvent(new Event('beforeprint'))
    })
    expect(lastChart.instance.resize).toHaveBeenLastCalledWith(
      MISSED_VISITS_PRINT_SIZE.width,
      MISSED_VISITS_PRINT_SIZE.height,
    )
    expect(lastChart.instance.options.devicePixelRatio).toBe(PRINT_PIXEL_RATIO)

    act(() => {
      window.dispatchEvent(new Event('afterprint'))
    })
    expect(lastChart.instance.resize).toHaveBeenLastCalledWith()
    expect(lastChart.instance.options.devicePixelRatio).toBeUndefined()
  })

  it('only allows printing once there is a report to print', async () => {
    mockApi(Response.json(months))
    render(<MissedVisitsReport title="Missed visits by month" />)

    expect(screen.getByRole('status')).toHaveTextContent('Loading report...')
    expect(screen.getByRole('button', { name: 'Print' })).toBeDisabled()

    await screen.findByRole('img', { name: /line chart/i })
    expect(screen.getByRole('button', { name: 'Print' })).toBeEnabled()
  })

  it('reports an API error and recovers on retry', async () => {
    mockApi(new Response(null, { status: 500 }), Response.json(months))
    render(<MissedVisitsReport title="Missed visits by month" />)

    expect(await screen.findByRole('alert')).toHaveTextContent(
      "Couldn't load the reportThe report service returned an error (HTTP 500). Try again in a moment.",
    )
    expect(screen.getByRole('button', { name: 'Print' })).toBeDisabled()

    await userEvent.click(screen.getByRole('button', { name: 'Try again' }))

    expect(await screen.findByRole('img', { name: /line chart/i })).toBeInTheDocument()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('says so when the API is unreachable', async () => {
    mockApi(new TypeError('Failed to fetch'))
    render(<MissedVisitsReport title="Missed visits by month" />)

    expect(await screen.findByRole('alert')).toHaveTextContent("The report service can't be reached.")
  })

  it('says so when there are no visits yet', async () => {
    mockApi(Response.json([]))
    render(<MissedVisitsReport title="Missed visits by month" />)

    expect(await screen.findByText('No visits recorded yet.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Print' })).toBeDisabled()
  })
})

import { act, render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import type { CaregiverLateness } from '../api/reports'
import { LATE_CHECK_INS_PRINT_SIZE, PRINT_PIXEL_RATIO } from '../chartPrintSize'
import { lastChart } from '../test/chartMock'
import { mockApi } from '../test/mockApi'
import { LateCheckInsReport } from './LateCheckInsReport'

// As the API returns them: most late first.
const caregivers: CaregiverLateness[] = [
  { caregiverName: 'Aisha Khan', avgMinutesLate: 52.3 },
  { caregiverName: 'Kenji Sato', avgMinutesLate: 48 },
  { caregiverName: 'Daniel Kim', avgMinutesLate: 11.5 },
]

function renderReport() {
  render(<LateCheckInsReport title="Late check-ins by caregiver" />)
  return screen.findByRole('img', { name: /bar chart of average minutes late/i })
}

describe('Late check-ins report (Query 3, OLAP-2)', () => {
  it('loads Query 3 from the API', async () => {
    const fetchMock = mockApi(Response.json(caregivers))

    await renderReport()

    expect(fetchMock).toHaveBeenCalledWith('/api/reports/caregiver-lateness', expect.anything())
  })

  it('charts one bar per caregiver, in the order the API returns them', async () => {
    mockApi(Response.json(caregivers))

    const chartImage = await renderReport()

    expect(lastChart.props?.data.labels).toEqual(['Aisha Khan', 'Kenji Sato', 'Daniel Kim'])
    expect(lastChart.props?.data.datasets.map((dataset) => [dataset.label, dataset.data])).toEqual([
      ['Average minutes late', [52.3, 48, 11.5]],
    ])
    expect(chartImage).toHaveAccessibleName(
      'Bar chart of average minutes late for 3 caregivers with late check-ins. ' +
        'Aisha Khan is the latest, at 52.3 minutes on average. The table view lists every caregiver.',
    )
  })

  it('shows how many caregivers have late check-ins', async () => {
    mockApi(Response.json(caregivers))
    await renderReport()

    expect(screen.getByText(/^3 caregivers with late check-ins/)).toBeInTheDocument()
  })

  it('switches to a table of the same values', async () => {
    mockApi(Response.json(caregivers))
    await renderReport()

    await userEvent.click(screen.getByRole('button', { name: 'Table' }))

    expect(screen.getByRole('button', { name: 'Table' })).toHaveAttribute('aria-pressed', 'true')
    expect(screen.queryByRole('img', { name: /bar chart/i })).not.toBeInTheDocument()
    const table = screen.getByRole('table', { name: 'Average minutes late per caregiver' })
    const rows = within(table).getAllByRole('row').slice(1)
    expect(rows.map((row) => [...(row as HTMLTableRowElement).cells].map((cell) => cell.textContent))).toEqual([
      ['Aisha Khan', '52.3'],
      ['Kenji Sato', '48.0'],
      ['Daniel Kim', '11.5'],
    ])

    await userEvent.click(screen.getByRole('button', { name: 'Chart' }))
    expect(screen.getByRole('img', { name: /bar chart/i })).toBeInTheDocument()
  })

  it('redraws the bar chart at its print size while printing', async () => {
    mockApi(Response.json(caregivers))
    await renderReport()

    act(() => {
      window.dispatchEvent(new Event('beforeprint'))
    })

    expect(lastChart.instance.resize).toHaveBeenLastCalledWith(
      LATE_CHECK_INS_PRINT_SIZE.width,
      LATE_CHECK_INS_PRINT_SIZE.height,
    )
    expect(lastChart.instance.options.devicePixelRatio).toBe(PRINT_PIXEL_RATIO)
  })

  it('says so when nobody has been late', async () => {
    mockApi(Response.json([]))
    render(<LateCheckInsReport title="Late check-ins by caregiver" />)

    expect(await screen.findByText('No late check-ins recorded yet.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Print' })).toBeDisabled()
  })

  it('reports an API error', async () => {
    mockApi(new Response(null, { status: 500 }))
    render(<LateCheckInsReport title="Late check-ins by caregiver" />)

    expect(await screen.findByRole('alert')).toHaveTextContent('The report service returned an error (HTTP 500).')
  })
})

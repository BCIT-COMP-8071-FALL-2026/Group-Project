import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { SeniorOutreach } from '../api/reports'
import { mockApi } from '../test/mockApi'
import { OutreachReport } from './OutreachReport'

// As the API returns them: most missed first.
const seniors: SeniorOutreach[] = [
  { seniorName: 'Dorothy Fleming', missedCount: 4, serviceTypes: 'Bathing, Doctor, Home visit', livesAlone: true, ageBand: 'Under 65' },
  { seniorName: 'Arthur Lambert', missedCount: 3, serviceTypes: 'Bathing, Home visit', livesAlone: false, ageBand: '85+' },
]

function renderReport() {
  render(<OutreachReport title="Seniors to follow up with" />)
  return screen.findByRole('table', { name: 'Seniors with 3 or more missed visits in the last 90 days' })
}

describe('Outreach report (Queries 2 and 4, OLAP-3)', () => {
  it('loads Query 2/4 from the API', async () => {
    const fetchMock = mockApi(Response.json(seniors))

    await renderReport()

    expect(fetchMock).toHaveBeenCalledWith('/api/reports/senior-outreach', expect.anything())
  })

  it('lists each senior with their missed visits, appointment types, living situation and age band', async () => {
    mockApi(Response.json(seniors))

    const table = await renderReport()

    const [header, ...rows] = within(table).getAllByRole('row')
    expect(within(header).getAllByRole('columnheader').map((cell) => cell.textContent)).toEqual([
      'Senior',
      'Missed visits',
      'Appointment types',
      'Lives alone',
      'Age band',
    ])
    expect(rows.map((row) => [...(row as HTMLTableRowElement).cells].map((cell) => cell.textContent))).toEqual([
      ['Dorothy Fleming', '4', 'Bathing, Doctor, Home visit', 'Yes', 'Under 65'],
      ['Arthur Lambert', '3', 'Bathing, Home visit', 'No', '85+'],
    ])
  })

  it('shows how many seniors need outreach', async () => {
    mockApi(Response.json(seniors))
    await renderReport()

    expect(screen.getByText(/^2 seniors with 3 or more missed visits/)).toBeInTheDocument()
  })

  it('says so when no senior needs outreach', async () => {
    mockApi(Response.json([]))
    render(<OutreachReport title="Seniors to follow up with" />)

    expect(await screen.findByText('No seniors have 3 or more missed visits in the last 90 days.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Print' })).toBeDisabled()
  })

  it('reports an unreachable API', async () => {
    mockApi(new TypeError('Failed to fetch'))
    render(<OutreachReport title="Seniors to follow up with" />)

    expect(await screen.findByRole('alert')).toHaveTextContent("The report service can't be reached.")
  })
})

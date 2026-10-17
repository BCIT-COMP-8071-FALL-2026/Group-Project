import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { ComponentProps } from 'react'
import type { Bar } from 'react-chartjs-2'
import { describe, expect, it, vi } from 'vitest'
import App from './App'

// jsdom can't draw on a canvas, so capture what the chart is given instead.
const barProps = vi.hoisted(() => ({ current: undefined as ComponentProps<typeof Bar> | undefined }))
vi.mock('react-chartjs-2', () => ({
  Bar: (props: ComponentProps<typeof Bar>) => {
    barProps.current = props
    return <canvas role="img" aria-label={props['aria-label']} />
  },
}))

const sampleVisits = [
  { id: 1, seniorName: 'Mary', visitCount: 5 },
  { id: 2, seniorName: 'John', visitCount: 8 },
  { id: 3, seniorName: 'Sara', visitCount: 1 },
]

function mockApi(...responses: Response[]) {
  const fetchMock = vi.fn<typeof fetch>()
  for (const response of responses) fetchMock.mockResolvedValueOnce(response)
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

describe('App', () => {
  it('charts the visits returned by the Web API', async () => {
    const fetchMock = mockApi(Response.json(sampleVisits))

    render(<App />)

    const chart = await screen.findByRole('img', { name: /bar chart of visits per senior/i })
    expect(chart).toHaveAccessibleName(
      'Bar chart of visits per senior. Mary: 5 visits, John: 8 visits, Sara: 1 visit.',
    )
    expect(fetchMock).toHaveBeenCalledWith('/api/senior-visits', expect.anything())
    expect(barProps.current?.data.labels).toEqual(['Mary', 'John', 'Sara'])
    expect(barProps.current?.data.datasets[0].data).toEqual([5, 8, 1])
  })

  it('shows the same rows in the table view', async () => {
    mockApi(Response.json(sampleVisits))
    render(<App />)
    await screen.findByRole('img', { name: /bar chart/i })

    await userEvent.click(screen.getByRole('button', { name: 'Table' }))

    expect(screen.getByRole('button', { name: 'Table' })).toHaveAttribute('aria-pressed', 'true')
    const rows = screen.getAllByRole('row').slice(1)
    expect(rows.map((row) => row.textContent)).toEqual(['Mary5', 'John8', 'Sara1'])
  })

  it('reports an API failure and recovers on retry', async () => {
    mockApi(new Response(null, { status: 500 }), Response.json(sampleVisits))
    render(<App />)

    expect(await screen.findByRole('alert')).toHaveTextContent(
      "Couldn't load visitsGET /api/senior-visits failed with status 500",
    )

    await userEvent.click(screen.getByRole('button', { name: 'Try again' }))

    expect(await screen.findByRole('img', { name: /bar chart/i })).toBeInTheDocument()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('says so when there are no visits yet', async () => {
    mockApi(Response.json([]))
    render(<App />)

    expect(await screen.findByText('No visits recorded yet.')).toBeInTheDocument()
  })
})

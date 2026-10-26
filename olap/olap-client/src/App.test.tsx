import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import App from './App'

// Each report fetches its own rows; an empty list is enough to show which report is open.
function mockEmptyApi() {
  const fetchMock = vi.fn<typeof fetch>(async () => Response.json([]))
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function navLinks() {
  return within(screen.getByRole('navigation', { name: 'Reports' })).getAllByRole('link')
}

describe('App navigation', () => {
  it('lists the three reports and opens Missed visits by default', async () => {
    mockEmptyApi()
    render(<App />)

    expect(navLinks().map((link) => [link.textContent, link.getAttribute('href')])).toEqual([
      ['Missed visits', '#/missed-visits'],
      ['Late check-ins', '#/late-check-ins'],
      ['Outreach list', '#/outreach'],
    ])
    expect(screen.getByRole('link', { name: 'Missed visits' })).toHaveAttribute('aria-current', 'page')
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Missed visits by month')
    expect(document.title).toBe('Missed visits by month · Senior Care Analytics')
    expect(await screen.findByText('No visits recorded yet.')).toBeInTheDocument()
  })

  it('opens each report from the navigation', async () => {
    const fetchMock = mockEmptyApi()
    render(<App />)

    await userEvent.click(screen.getByRole('link', { name: 'Late check-ins' }))

    expect(await screen.findByRole('heading', { level: 1, name: 'Late check-ins by caregiver' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Late check-ins' })).toHaveAttribute('aria-current', 'page')
    expect(screen.getByRole('link', { name: 'Missed visits' })).not.toHaveAttribute('aria-current')
    expect(document.title).toBe('Late check-ins by caregiver · Senior Care Analytics')
    expect(fetchMock).toHaveBeenLastCalledWith('/api/reports/caregiver-lateness', expect.anything())

    await userEvent.click(screen.getByRole('link', { name: 'Outreach list' }))

    expect(await screen.findByRole('heading', { level: 1, name: 'Seniors to follow up with' })).toBeInTheDocument()
    expect(fetchMock).toHaveBeenLastCalledWith('/api/reports/senior-outreach', expect.anything())
  })

  it('opens the report in the address, so a refreshed or shared link keeps its page', async () => {
    mockEmptyApi()
    window.history.replaceState(null, '', '/#/outreach')

    render(<App />)

    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Seniors to follow up with')
    expect(screen.getByRole('link', { name: 'Outreach list' })).toHaveAttribute('aria-current', 'page')
    await screen.findByText('No seniors have 3 or more missed visits in the last 90 days.')
  })

  it('falls back to Missed visits for an unknown address', async () => {
    mockEmptyApi()
    window.history.replaceState(null, '', '/#/no-such-report')

    render(<App />)

    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Missed visits by month')
    await screen.findByText('No visits recorded yet.')
  })
})

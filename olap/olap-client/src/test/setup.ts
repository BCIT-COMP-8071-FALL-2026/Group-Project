import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach, beforeEach, vi } from 'vitest'
import { lastChart } from './chartMock'

// Every test file gets canvas-free charts (see chartMock.ts).
vi.mock('react-chartjs-2', async () => {
  const { MockChart } = await import('./MockChart')
  return { Line: MockChart, Bar: MockChart }
})

beforeEach(() => {
  lastChart.props = undefined
  lastChart.instance.options = {}
  lastChart.instance.resize = vi.fn()
})

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
  vi.unstubAllGlobals()
  vi.unstubAllEnvs()
  // Back to the home page, so a report route doesn't leak into the next test.
  window.history.replaceState(null, '', '/')
})

// jsdom has no matchMedia; the chart theme hook needs it.
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string): MediaQueryList =>
    ({
      matches: false,
      media: query,
      onchange: null,
      addEventListener: () => {},
      removeEventListener: () => {},
      addListener: () => {},
      removeListener: () => {},
      dispatchEvent: () => false,
    }) as MediaQueryList,
})

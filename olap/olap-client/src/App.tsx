import { ChartLegend } from './components/ChartLegend'
import { AlertIcon, ChartIcon, PrinterIcon } from './components/Icons'
import { MissedVisitsChart } from './components/MissedVisitsChart'
import { MissedVisitsTable } from './components/MissedVisitsTable'
import { formatDateTime, formatMonthRange } from './format'
import { useMissedVisitsByMonth, type MissedVisitsState } from './hooks/useMissedVisitsByMonth'

export default function App() {
  const { state, retry } = useMissedVisitsByMonth()
  const canPrint = state.status === 'ready' && state.months.length > 0

  return (
    <div className="app">
      <aside className="sidebar card">
        <div className="brand">
          <span className="brand-mark">
            <ChartIcon />
          </span>
          <span className="brand-name">Analytics</span>
        </div>
        <nav aria-label="Reports">
          <ul className="nav-list">
            <li>
              <a className="nav-item" href="/" aria-current="page">
                Missed visits
              </a>
            </li>
          </ul>
        </nav>
      </aside>

      <main className="report card">
        <header className="report-header">
          <div>
            <p className="report-eyebrow">Senior care analytics</p>
            <h1>Missed visits by month</h1>
            {state.status === 'ready' && state.months.length > 0 && (
              <p className="report-meta">
                {formatMonthRange(state.months[0].month, state.months[state.months.length - 1].month)}
                <span aria-hidden="true"> · </span>
                Generated {formatDateTime(state.loadedAt)}
              </p>
            )}
          </div>
          <button type="button" className="button button-primary" onClick={() => window.print()} disabled={!canPrint}>
            <PrinterIcon />
            Print
          </button>
        </header>

        <ReportBody state={state} onRetry={retry} />
      </main>
    </div>
  )
}

function ReportBody({ state, onRetry }: { state: MissedVisitsState; onRetry: () => void }) {
  switch (state.status) {
    case 'loading':
      return (
        <p className="status" role="status">
          Loading report...
        </p>
      )
    case 'error':
      return (
        <div className="status" role="alert">
          <p className="status-title">
            <AlertIcon />
            Couldn't load the report
          </p>
          <p className="status-detail">{state.message} Try again in a moment.</p>
          <button type="button" className="button button-secondary" onClick={onRetry}>
            Try again
          </button>
        </div>
      )
    case 'ready':
      if (state.months.length === 0) {
        return <p className="status">No visits recorded yet.</p>
      }
      return (
        <>
          <section className="report-section" aria-labelledby="chart-heading">
            <div className="section-header">
              <h2 id="chart-heading">Visits per month</h2>
              <ChartLegend />
            </div>
            <MissedVisitsChart months={state.months} />
          </section>

          <section className="report-section" aria-labelledby="table-heading">
            <h2 id="table-heading">Monthly breakdown</h2>
            <MissedVisitsTable months={state.months} />
          </section>

          <p className="report-note">
            A visit is missed when nobody checked in within 60 minutes of its scheduled start. Cancelled visits, and
            visits whose 60 minutes haven't passed yet, are not counted.
          </p>
        </>
      )
  }
}

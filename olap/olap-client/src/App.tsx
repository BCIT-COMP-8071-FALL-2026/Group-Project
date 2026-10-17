import { useState } from 'react'
import { SENIOR_VISITS_URL } from './api/seniorVisits'
import { VisitsChart } from './components/VisitsChart'
import { VisitsTable } from './components/VisitsTable'
import { useSeniorVisits, type SeniorVisitsState } from './hooks/useSeniorVisits'

type View = 'chart' | 'table'

export default function App() {
  const { state, retry } = useSeniorVisits()
  const [view, setView] = useState<View>('chart')

  return (
    <main className="page">
      <header className="page-header">
        <p className="eyebrow">OLAP sample</p>
        <h1>Senior care analytics</h1>
        <p className="lede">React and Chart.js reading JSON from the ASP.NET Web API.</p>
      </header>

      <section className="card" aria-labelledby="visits-title">
        <div className="card-header">
          <div>
            <h2 id="visits-title">Visits per senior</h2>
            <p className="card-subtitle">
              Total recorded visits from <code>GET {SENIOR_VISITS_URL}</code>
            </p>
          </div>
          <div className="view-toggle" role="group" aria-label="Display as">
            <button type="button" aria-pressed={view === 'chart'} onClick={() => setView('chart')}>
              Chart
            </button>
            <button type="button" aria-pressed={view === 'table'} onClick={() => setView('table')}>
              Table
            </button>
          </div>
        </div>

        <div className="card-body">
          <VisitsBody state={state} view={view} onRetry={retry} />
        </div>
      </section>
    </main>
  )
}

function VisitsBody({ state, view, onRetry }: { state: SeniorVisitsState; view: View; onRetry: () => void }) {
  switch (state.status) {
    case 'loading':
      return (
        <p className="status" role="status">
          Loading visits...
        </p>
      )
    case 'error':
      return (
        <div className="status" role="alert">
          <p className="status-title">
            <svg className="status-icon" viewBox="0 0 16 16" aria-hidden="true">
              <path d="M8 1.5a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13ZM7.25 4.5h1.5v4.5h-1.5Zm0 6h1.5V12h-1.5Z" />
            </svg>
            Couldn't load visits
          </p>
          <p className="status-detail">{state.message}. Check that OlapApi is running.</p>
          <button type="button" className="button" onClick={onRetry}>
            Try again
          </button>
        </div>
      )
    case 'ready':
      if (state.visits.length === 0) {
        return <p className="status">No visits recorded yet.</p>
      }
      return view === 'chart' ? <VisitsChart visits={state.visits} /> : <VisitsTable visits={state.visits} />
  }
}

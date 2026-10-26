import { useEffect } from 'react'
import { ChartIcon } from './components/Icons'
import { useHashRoute } from './hooks/useHashRoute'
import { LateCheckInsReport } from './reports/LateCheckInsReport'
import { MissedVisitsReport } from './reports/MissedVisitsReport'
import { OutreachReport } from './reports/OutreachReport'

const REPORTS = [
  { path: 'missed-visits', label: 'Missed visits', title: 'Missed visits by month', Report: MissedVisitsReport },
  { path: 'late-check-ins', label: 'Late check-ins', title: 'Late check-ins by caregiver', Report: LateCheckInsReport },
  { path: 'outreach', label: 'Outreach list', title: 'Seniors to follow up with', Report: OutreachReport },
]

export default function App() {
  const route = useHashRoute()
  // The first report is the home page, and an unknown address falls back to it.
  const current = REPORTS.find((report) => report.path === route) ?? REPORTS[0]

  useEffect(() => {
    document.title = `${current.title} · Senior Care Analytics`
  }, [current])

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
            {REPORTS.map((report) => (
              <li key={report.path}>
                <a
                  className="nav-item"
                  href={`#/${report.path}`}
                  aria-current={report === current ? 'page' : undefined}
                >
                  {report.label}
                </a>
              </li>
            ))}
          </ul>
        </nav>
      </aside>

      {/* Keyed by path so switching reports starts each one fresh. */}
      <current.Report key={current.path} title={current.title} />
    </div>
  )
}

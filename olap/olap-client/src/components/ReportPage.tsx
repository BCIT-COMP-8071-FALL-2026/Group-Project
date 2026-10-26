import type { ReactNode } from 'react'
import { formatDateTime } from '../format'
import type { ReportState } from '../hooks/useReport'
import { AlertIcon, PrinterIcon } from './Icons'

interface ReportPageProps<T> {
  title: string
  state: ReportState<T>
  onRetry: () => void
  /** Shown instead of the report when the API returns no rows. */
  emptyMessage: string
  /** What the rows cover, shown under the title before the time the report was generated. */
  summary: (rows: T[]) => string
  /** How the numbers are worked out, shown (and printed) under the report. */
  note: string
  children: (rows: T[]) => ReactNode
}

/** The frame every report shares: title, Print button (OLAP-4), and the loading, error and empty states. */
export function ReportPage<T>({ title, state, onRetry, emptyMessage, summary, note, children }: ReportPageProps<T>) {
  const hasRows = state.status === 'ready' && state.rows.length > 0

  return (
    <main className="report card">
      <header className="report-header">
        <div>
          <p className="report-eyebrow">Senior care analytics</p>
          <h1>{title}</h1>
          {state.status === 'ready' && state.rows.length > 0 && (
            <p className="report-meta">
              {summary(state.rows)}
              <span aria-hidden="true"> · </span>
              Generated {formatDateTime(state.loadedAt)}
            </p>
          )}
        </div>
        <button type="button" className="button button-primary" onClick={() => window.print()} disabled={!hasRows}>
          <PrinterIcon />
          Print
        </button>
      </header>

      <ReportBody state={state} onRetry={onRetry} emptyMessage={emptyMessage} note={note}>
        {children}
      </ReportBody>
    </main>
  )
}

function ReportBody<T>({
  state,
  onRetry,
  emptyMessage,
  note,
  children,
}: Pick<ReportPageProps<T>, 'state' | 'onRetry' | 'emptyMessage' | 'note' | 'children'>) {
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
      if (state.rows.length === 0) {
        return <p className="status">{emptyMessage}</p>
      }
      return (
        <>
          {children(state.rows)}
          <p className="report-note">{note}</p>
        </>
      )
  }
}

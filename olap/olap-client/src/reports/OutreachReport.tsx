import { fetchSeniorOutreach } from '../api/reports'
import { OutreachTable } from '../components/OutreachTable'
import { ReportPage } from '../components/ReportPage'
import { formatCount } from '../format'
import { useReport } from '../hooks/useReport'

/** Queries 2 and 4 (OLAP-3): seniors with 3 or more missed visits in the last 90 days. */
export function OutreachReport({ title }: { title: string }) {
  const { state, retry } = useReport(fetchSeniorOutreach)

  return (
    <ReportPage
      title={title}
      state={state}
      onRetry={retry}
      emptyMessage="No seniors have 3 or more missed visits in the last 90 days."
      summary={(seniors) =>
        `${formatCount(seniors.length)} ${seniors.length === 1 ? 'senior' : 'seniors'} with 3 or more missed visits`
      }
      note={
        'Missed visits are counted over the 90 days up to the most recent visit, and age bands are worked out at ' +
        'that date. A visit is missed when nobody checked in within 60 minutes of its scheduled start; cancelled ' +
        'visits are not counted.'
      }
    >
      {(seniors) => <OutreachTable seniors={seniors} />}
    </ReportPage>
  )
}

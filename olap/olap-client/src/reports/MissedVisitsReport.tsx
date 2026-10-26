import { fetchMissedVisitsByMonth } from '../api/reports'
import { ChartLegend } from '../components/ChartLegend'
import { MissedVisitsChart } from '../components/MissedVisitsChart'
import { MissedVisitsTable } from '../components/MissedVisitsTable'
import { ReportPage } from '../components/ReportPage'
import { formatMonthRange } from '../format'
import { useReport } from '../hooks/useReport'

/** Query 1 (OLAP-1): successful and missed visits per month, as a line chart and a table. */
export function MissedVisitsReport({ title }: { title: string }) {
  const { state, retry } = useReport(fetchMissedVisitsByMonth)

  return (
    <ReportPage
      title={title}
      state={state}
      onRetry={retry}
      emptyMessage="No visits recorded yet."
      summary={(months) => formatMonthRange(months[0].month, months[months.length - 1].month)}
      note={
        'A visit is missed when nobody checked in within 60 minutes of its scheduled start. Cancelled visits, ' +
        "and visits whose 60 minutes haven't passed yet, are not counted."
      }
    >
      {(months) => (
        <>
          <section className="report-section" aria-labelledby="chart-heading">
            <div className="section-header">
              <h2 id="chart-heading">Visits per month</h2>
              <ChartLegend />
            </div>
            <MissedVisitsChart months={months} />
          </section>

          <section className="report-section" aria-labelledby="table-heading">
            <h2 id="table-heading">Monthly breakdown</h2>
            <MissedVisitsTable months={months} />
          </section>
        </>
      )}
    </ReportPage>
  )
}

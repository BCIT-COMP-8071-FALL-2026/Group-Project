import { useState } from 'react'
import { fetchCaregiverLateness } from '../api/reports'
import { LateCheckInsChart } from '../components/LateCheckInsChart'
import { LateCheckInsTable } from '../components/LateCheckInsTable'
import { ReportPage } from '../components/ReportPage'
import { ViewToggle, type View } from '../components/ViewToggle'
import { formatCount } from '../format'
import { useReport } from '../hooks/useReport'

/** Query 3 (OLAP-2): average minutes late per caregiver, as a bar chart or the same data as a table. */
export function LateCheckInsReport({ title }: { title: string }) {
  const { state, retry } = useReport(fetchCaregiverLateness)
  const [view, setView] = useState<View>('chart')

  return (
    <ReportPage
      title={title}
      state={state}
      onRetry={retry}
      emptyMessage="No late check-ins recorded yet."
      summary={(caregivers) =>
        `${formatCount(caregivers.length)} ${caregivers.length === 1 ? 'caregiver' : 'caregivers'} with late check-ins`
      }
      note={
        'A check-in is late when it is more than 10 minutes after the scheduled start but within 60 minutes; ' +
        "after that, the visit counts as missed. Each average covers that caregiver's late check-ins only, and " +
        'caregivers who have never been late are not shown.'
      }
    >
      {(caregivers) => (
        <section className="report-section" aria-labelledby="lateness-heading">
          <div className="section-header">
            <h2 id="lateness-heading">Average minutes late per caregiver</h2>
            <ViewToggle value={view} onChange={setView} />
          </div>
          {view === 'chart' ? (
            <LateCheckInsChart caregivers={caregivers} />
          ) : (
            <LateCheckInsTable caregivers={caregivers} />
          )}
        </section>
      )}
    </ReportPage>
  )
}

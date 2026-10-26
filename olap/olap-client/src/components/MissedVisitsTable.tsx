import type { MissedVisitsMonth } from '../api/reports'
import { formatCount, formatMonth, formatMonthShort, formatPercent, isCurrentMonth } from '../format'
import { useMediaQuery } from '../hooks/useMediaQuery'

export function MissedVisitsTable({ months }: { months: MissedVisitsMonth[] }) {
  // On a phone, "September 2026" would wrap and double the row height.
  const monthLabel = useMediaQuery('(max-width: 600px)') ? formatMonthShort : formatMonth

  return (
    <table className="report-table">
      <caption className="visually-hidden">Visits, successful visits and missed visits per month</caption>
      <thead>
        <tr>
          <th scope="col">Month</th>
          <th scope="col" className="num">
            Visits
          </th>
          <th scope="col" className="num">
            Successful
          </th>
          <th scope="col" className="num">
            Missed
          </th>
          <th scope="col" className="num">
            Missed %
          </th>
        </tr>
      </thead>
      <tbody>
        {months.map((month) => (
          <tr key={month.month}>
            <th scope="row">
              <span className="table-month">{monthLabel(month.month)}</span>
              {isCurrentMonth(month.month) && (
                <>
                  {' '}
                  <span className="table-note">(to date)</span>
                </>
              )}
            </th>
            <td className="num">{formatCount(month.visitCount)}</td>
            <td className="num">{formatCount(month.visitCount - month.missedCount)}</td>
            <td className="num">{formatCount(month.missedCount)}</td>
            <td className="num">{formatPercent(month.missedPercent)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

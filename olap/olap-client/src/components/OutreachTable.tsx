import type { SeniorOutreach } from '../api/reports'
import { formatCount, formatYesNo } from '../format'

/** OLAP-3: seniors with 3 or more missed visits (Query 4), with lives alone and age band (Query 2). */
export function OutreachTable({ seniors }: { seniors: SeniorOutreach[] }) {
  return (
    <table className="report-table outreach-table">
      <caption className="visually-hidden">Seniors with 3 or more missed visits in the last 90 days</caption>
      <thead>
        <tr>
          <th scope="col">Senior</th>
          <th scope="col" className="num">
            Missed visits
          </th>
          <th scope="col">Appointment types</th>
          <th scope="col">Lives alone</th>
          <th scope="col">Age band</th>
        </tr>
      </thead>
      <tbody>
        {seniors.map((senior, index) => (
          // Two seniors can share a name, so the key also uses the row's position.
          <tr key={`${index}-${senior.seniorName}`}>
            <th scope="row">{senior.seniorName}</th>
            <td className="num">{formatCount(senior.missedCount)}</td>
            <td>{senior.serviceTypes}</td>
            <td>{formatYesNo(senior.livesAlone)}</td>
            <td className="nowrap">{senior.ageBand}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

import type { SeniorVisit } from '../api/seniorVisits'

export function VisitsTable({ visits }: { visits: SeniorVisit[] }) {
  return (
    <table className="visits-table">
      <caption className="visually-hidden">Visits per senior</caption>
      <thead>
        <tr>
          <th scope="col">Senior</th>
          <th scope="col" className="num">
            Visits
          </th>
        </tr>
      </thead>
      <tbody>
        {visits.map((visit) => (
          <tr key={visit.id}>
            <th scope="row">{visit.seniorName}</th>
            <td className="num">{visit.visitCount.toLocaleString()}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

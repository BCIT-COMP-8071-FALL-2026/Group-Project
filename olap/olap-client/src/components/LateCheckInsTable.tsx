import type { CaregiverLateness } from '../api/reports'
import { formatMinutes } from '../format'

export function LateCheckInsTable({ caregivers }: { caregivers: CaregiverLateness[] }) {
  return (
    <table className="report-table">
      <caption className="visually-hidden">Average minutes late per caregiver</caption>
      <thead>
        <tr>
          <th scope="col">Caregiver</th>
          <th scope="col" className="num">
            Average minutes late
          </th>
        </tr>
      </thead>
      <tbody>
        {caregivers.map((caregiver, index) => (
          // Two caregivers can share a name, so the key also uses the row's position.
          <tr key={`${index}-${caregiver.caregiverName}`}>
            <th scope="row">{caregiver.caregiverName}</th>
            <td className="num">{formatMinutes(caregiver.avgMinutesLate)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

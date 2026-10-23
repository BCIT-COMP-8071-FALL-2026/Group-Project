export function ChartLegend() {
  return (
    <ul className="legend" aria-label="Legend">
      <li>
        <span className="legend-key legend-key-successful" aria-hidden="true" />
        Successful
      </li>
      <li>
        <span className="legend-key legend-key-missed" aria-hidden="true" />
        Missed
      </li>
    </ul>
  )
}

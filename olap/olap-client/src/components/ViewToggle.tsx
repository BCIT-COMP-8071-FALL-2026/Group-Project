export type View = 'chart' | 'table'

/** Switches a report between its chart and the same data as a table. */
export function ViewToggle({ value, onChange }: { value: View; onChange: (view: View) => void }) {
  return (
    <div className="view-toggle" role="group" aria-label="Show as">
      <button type="button" aria-pressed={value === 'chart'} onClick={() => onChange('chart')}>
        Chart
      </button>
      <button type="button" aria-pressed={value === 'table'} onClick={() => onChange('table')}>
        Table
      </button>
    </div>
  )
}

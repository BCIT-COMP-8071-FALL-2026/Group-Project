import { useEffect, useImperativeHandle, type Ref } from 'react'
import { lastChart, type MockChartProps } from './chartMock'

/** Stands in for react-chartjs-2's Line and Bar in tests (see chartMock.ts). */
export function MockChart({ ref, ...props }: MockChartProps & { ref?: Ref<unknown> }) {
  useImperativeHandle(ref, () => lastChart.instance)
  useEffect(() => {
    lastChart.props = props
  })
  return <canvas role="img" aria-label={props['aria-label']} />
}

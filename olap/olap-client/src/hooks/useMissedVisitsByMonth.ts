import { useCallback, useEffect, useState } from 'react'
import { fetchMissedVisitsByMonth, type MissedVisitsMonth } from '../api/missedVisits'

export type MissedVisitsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; months: MissedVisitsMonth[]; loadedAt: Date }

export function useMissedVisitsByMonth() {
  const [state, setState] = useState<MissedVisitsState>({ status: 'loading' })
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    fetchMissedVisitsByMonth(controller.signal)
      .then((months) => setState({ status: 'ready', months, loadedAt: new Date() }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) return
        setState({ status: 'error', message: error instanceof Error ? error.message : String(error) })
      })

    return () => controller.abort()
  }, [attempt])

  const retry = useCallback(() => {
    setState({ status: 'loading' })
    setAttempt((n) => n + 1)
  }, [])

  return { state, retry }
}

import { useCallback, useEffect, useState } from 'react'
import { fetchSeniorVisits, type SeniorVisit } from '../api/seniorVisits'

export type SeniorVisitsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; visits: SeniorVisit[] }

export function useSeniorVisits() {
  const [state, setState] = useState<SeniorVisitsState>({ status: 'loading' })
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    fetchSeniorVisits(controller.signal)
      .then((visits) => setState({ status: 'ready', visits }))
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

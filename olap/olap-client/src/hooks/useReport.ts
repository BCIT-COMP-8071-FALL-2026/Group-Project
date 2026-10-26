import { useCallback, useEffect, useState } from 'react'

export type ReportState<T> =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; rows: T[]; loadedAt: Date }

/** Loads a report's rows, with a retry for when the API call fails. `fetchRows` must be a stable function. */
export function useReport<T>(fetchRows: (signal: AbortSignal) => Promise<T[]>) {
  const [state, setState] = useState<ReportState<T>>({ status: 'loading' })
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    fetchRows(controller.signal)
      .then((rows) => setState({ status: 'ready', rows, loadedAt: new Date() }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) return
        setState({ status: 'error', message: error instanceof Error ? error.message : String(error) })
      })

    return () => controller.abort()
  }, [fetchRows, attempt])

  const retry = useCallback(() => {
    setState({ status: 'loading' })
    setAttempt((n) => n + 1)
  }, [])

  return { state, retry }
}

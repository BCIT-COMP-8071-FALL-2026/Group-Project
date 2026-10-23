import { apiUrl } from '../config'

/** One month of Query 1 (see docs/Query1-Contract.md). */
export interface MissedVisitsMonth {
  /** YYYY-MM, e.g. 2025-10 */
  month: string
  /** Visits in the month, excluding cancelled and not yet due */
  visitCount: number
  missedCount: number
  /** 0 to 100, rounded to 1 decimal place */
  missedPercent: number
}

export const MISSED_VISITS_PATH = '/api/reports/missed-visits-by-month'

export async function fetchMissedVisitsByMonth(signal?: AbortSignal): Promise<MissedVisitsMonth[]> {
  let response: Response
  try {
    response = await fetch(apiUrl(MISSED_VISITS_PATH), { signal, headers: { Accept: 'application/json' } })
  } catch (error) {
    if (signal?.aborted) throw error
    // The API is down, unreachable, or doesn't allow this origin (CORS).
    throw new Error("The report service can't be reached.", { cause: error })
  }

  if (!response.ok) {
    throw new Error(`The report service returned an error (HTTP ${response.status}).`)
  }
  return (await response.json()) as MissedVisitsMonth[]
}

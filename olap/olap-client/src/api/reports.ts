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

/** One caregiver in Query 3 (olap/sql/procs/CaregiverLateness.sql). */
export interface CaregiverLateness {
  caregiverName: string
  /** Average over the caregiver's Late visits only, rounded to 1 decimal place */
  avgMinutesLate: number
}

/** One senior in Query 2/4 (olap/sql/procs/SeniorOutreach.sql). */
export interface SeniorOutreach {
  seniorName: string
  /** Missed visits in the 90 days before the anchor date; always 3 or more */
  missedCount: number
  /** Service names of the missed visits, comma-separated and sorted, e.g. "Bathing, Doctor" */
  serviceTypes: string
  livesAlone: boolean
  /** Under 65, 65-74, 75-84 or 85+, at the anchor date */
  ageBand: string
}

export const REPORT_PATHS = {
  missedVisits: '/api/reports/missed-visits-by-month',
  caregiverLateness: '/api/reports/caregiver-lateness',
  seniorOutreach: '/api/reports/senior-outreach',
} as const

async function fetchReport<T>(path: string, signal?: AbortSignal): Promise<T[]> {
  let response: Response
  try {
    response = await fetch(apiUrl(path), { signal, headers: { Accept: 'application/json' } })
  } catch (error) {
    if (signal?.aborted) throw error
    // The API is down, unreachable, or doesn't allow this origin (CORS).
    throw new Error("The report service can't be reached.", { cause: error })
  }

  if (!response.ok) {
    throw new Error(`The report service returned an error (HTTP ${response.status}).`)
  }
  return (await response.json()) as T[]
}

export const fetchMissedVisitsByMonth = (signal?: AbortSignal) =>
  fetchReport<MissedVisitsMonth>(REPORT_PATHS.missedVisits, signal)

export const fetchCaregiverLateness = (signal?: AbortSignal) =>
  fetchReport<CaregiverLateness>(REPORT_PATHS.caregiverLateness, signal)

export const fetchSeniorOutreach = (signal?: AbortSignal) =>
  fetchReport<SeniorOutreach>(REPORT_PATHS.seniorOutreach, signal)

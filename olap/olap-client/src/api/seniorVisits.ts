export interface SeniorVisit {
  id: number
  seniorName: string
  visitCount: number
}

export const SENIOR_VISITS_URL = '/api/senior-visits'

export async function fetchSeniorVisits(signal?: AbortSignal): Promise<SeniorVisit[]> {
  const response = await fetch(SENIOR_VISITS_URL, { signal })
  if (!response.ok) {
    throw new Error(`GET ${SENIOR_VISITS_URL} failed with status ${response.status}`)
  }
  return (await response.json()) as SeniorVisit[]
}

import { vi } from 'vitest'

/** Replaces fetch with one that returns (or rejects with) each of `responses` in turn. */
export function mockApi(...responses: (Response | Error)[]) {
  const fetchMock = vi.fn<typeof fetch>()
  for (const response of responses) {
    if (response instanceof Error) fetchMock.mockRejectedValueOnce(response)
    else fetchMock.mockResolvedValueOnce(response)
  }
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

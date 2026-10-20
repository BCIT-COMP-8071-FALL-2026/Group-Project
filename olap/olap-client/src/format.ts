export function formatVisits(count: number): string {
  return `${count.toLocaleString()} ${count === 1 ? 'visit' : 'visits'}`
}

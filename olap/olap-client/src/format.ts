// One locale for every label, so the screen, the printout and the tests agree.
const locale = 'en-CA'

const shortMonth = new Intl.DateTimeFormat(locale, { month: 'short', timeZone: 'UTC' })
const shortMonthYear = new Intl.DateTimeFormat(locale, { month: 'short', year: 'numeric', timeZone: 'UTC' })
const longMonth = new Intl.DateTimeFormat(locale, { month: 'long', year: 'numeric', timeZone: 'UTC' })
const dateTime = new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' })
const count = new Intl.NumberFormat(locale)
const oneDecimal = new Intl.NumberFormat(locale, { minimumFractionDigits: 1, maximumFractionDigits: 1 })

/** A yyyy-MM month as a UTC date, so formatting never shifts it into the previous month. */
function monthDate(month: string): Date {
  const [year, monthNumber] = month.split('-').map(Number)
  return new Date(Date.UTC(year, monthNumber - 1, 1))
}

/** "October 2025" */
export function formatMonth(month: string): string {
  return longMonth.format(monthDate(month))
}

/** "Oct 2025" */
export function formatMonthShort(month: string): string {
  return shortMonthYear.format(monthDate(month))
}

/**
 * "Oct", with the year underneath on the first label and on every January. Labels stay this narrow so that
 * a printed chart can show every month; the current month is marked on the line and in the table instead.
 */
export function formatAxisMonth(month: string, index: number): string | string[] {
  const date = monthDate(month)
  const label = shortMonth.format(date)
  return index === 0 || date.getUTCMonth() === 0 ? [label, String(date.getUTCFullYear())] : label
}

/** "October 2025 to October 2026" */
export function formatMonthRange(first: string, last: string): string {
  return first === last ? formatMonth(first) : `${formatMonth(first)} to ${formatMonth(last)}`
}

export function formatCount(value: number): string {
  return count.format(value)
}

/** 7.5 -> "7.5%" */
export function formatPercent(value: number): string {
  return `${oneDecimal.format(value)}%`
}

/** 45.25 -> "45.3" (minutes, always one decimal place) */
export function formatMinutes(value: number): string {
  return oneDecimal.format(value)
}

/** "Yes" or "No" */
export function formatYesNo(value: boolean): string {
  return value ? 'Yes' : 'No'
}

export function formatDateTime(date: Date): string {
  return dateTime.format(date)
}

/** Whether a yyyy-MM month is the current calendar month, i.e. its numbers are still growing. */
export function isCurrentMonth(month: string, now = new Date()): boolean {
  return month === `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`
}

// One locale for every label, so the screen, the printout and the tests agree.
const locale = 'en-CA'

const shortMonth = new Intl.DateTimeFormat(locale, { month: 'short', timeZone: 'UTC' })
const shortMonthYear = new Intl.DateTimeFormat(locale, { month: 'short', year: 'numeric', timeZone: 'UTC' })
const longMonth = new Intl.DateTimeFormat(locale, { month: 'long', year: 'numeric', timeZone: 'UTC' })
const dateTime = new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' })
const count = new Intl.NumberFormat(locale)
const percent = new Intl.NumberFormat(locale, { minimumFractionDigits: 1, maximumFractionDigits: 1 })

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

/** "Oct", with the year underneath on the first label and on every January, and "to date" under the current month. */
export function formatAxisMonth(month: string, index: number, now = new Date()): string | string[] {
  const date = monthDate(month)
  const lines = [shortMonth.format(date)]
  if (index === 0 || date.getUTCMonth() === 0) lines.push(String(date.getUTCFullYear()))
  if (isCurrentMonth(month, now)) lines.push('to date')
  return lines.length === 1 ? lines[0] : lines
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
  return `${percent.format(value)}%`
}

export function formatDateTime(date: Date): string {
  return dateTime.format(date)
}

/**
 * Whether a yyyy-MM month is the current month, i.e. its numbers are still growing. The API groups visits
 * by UTC month (docs/Query1-Contract.md), so this uses UTC too; local time picks the wrong month for the
 * hours around midnight at the end of a month.
 */
export function isCurrentMonth(month: string, now = new Date()): boolean {
  return month === `${now.getUTCFullYear()}-${String(now.getUTCMonth() + 1).padStart(2, '0')}`
}

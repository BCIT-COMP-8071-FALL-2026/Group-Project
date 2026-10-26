import { describe, expect, it } from 'vitest'
import { formatAxisMonth, formatMonth, formatMonthRange, formatMonthShort, formatPercent, isCurrentMonth } from './format'

describe('format', () => {
  it('names a month without shifting it across time zones', () => {
    expect(formatMonth('2025-10')).toBe('October 2025')
    expect(formatMonth('2026-01')).toBe('January 2026')
    expect(formatMonthShort('2026-09')).toBe('Sep 2026')
  })

  it('adds the year to the first axis label and to each January', () => {
    expect(['2025-11', '2025-12', '2026-01', '2026-02'].map(formatAxisMonth)).toEqual([
      ['Nov', '2025'],
      'Dec',
      ['Jan', '2026'],
      'Feb',
    ])
  })

  it('describes a range of months', () => {
    expect(formatMonthRange('2025-10', '2026-09')).toBe('October 2025 to September 2026')
    expect(formatMonthRange('2026-03', '2026-03')).toBe('March 2026')
  })

  it('shows percentages with one decimal', () => {
    expect([0, 7.5, 9.16, 100].map(formatPercent)).toEqual(['0.0%', '7.5%', '9.2%', '100.0%'])
  })

  it('knows which month is the current one', () => {
    const now = new Date(2026, 9, 13)
    expect(isCurrentMonth('2026-10', now)).toBe(true)
    expect(isCurrentMonth('2026-09', now)).toBe(false)
    expect(isCurrentMonth('2025-10', now)).toBe(false)
  })
})

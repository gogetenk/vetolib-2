'use client'

import { useRef } from 'react'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { cn } from '@/lib/utils'

// ─── Types ────────────────────────────────────────────────────────────────────

interface WeekNavigatorProps {
  /** The Sunday starting the currently displayed week (YYYY-MM-DD, Asia/Dubai local date) */
  weekStart: string
  /** The currently selected day (YYYY-MM-DD) */
  selectedDay?: string | null
  /** Called with the selected day's YYYY-MM-DD string */
  onDaySelect: (day: string) => void
  /** Called when user navigates to a different week; receives new weekStart (YYYY-MM-DD) */
  onWeekChange: (newWeekStart: string) => void
  /**
   * Days that are marked as closed by the clinic (0=Sun…6=Sat).
   * Defaults to [5, 6] (Fri/Sat — UAE weekend).
   */
  closedDays?: number[]
}

// ─── Constants ────────────────────────────────────────────────────────────────

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'] as const

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Parse YYYY-MM-DD as UTC midnight (avoids local-timezone shift) */
function parseDate(yyyyMmDd: string): Date {
  const [y, m, d] = yyyyMmDd.split('-').map(Number)
  return new Date(Date.UTC(y, m - 1, d))
}

/** Format UTC date back to YYYY-MM-DD */
function formatDate(d: Date): string {
  const y = d.getUTCFullYear()
  const m = String(d.getUTCMonth() + 1).padStart(2, '0')
  const dd = String(d.getUTCDate()).padStart(2, '0')
  return `${y}-${m}-${dd}`
}

/** Add n days (UTC) */
function addDays(dateStr: string, n: number): string {
  const d = parseDate(dateStr)
  d.setUTCDate(d.getUTCDate() + n)
  return formatDate(d)
}

/** Get today's date in Asia/Dubai as YYYY-MM-DD */
function getTodayDubai(): string {
  return new Date().toLocaleDateString('en-CA', { timeZone: 'Asia/Dubai' }) // en-CA = YYYY-MM-DD format
}

/** Format week range header: "Mar 15 – Mar 21, 2026" */
function formatWeekRange(weekStart: string): string {
  const start = parseDate(weekStart)
  const end = new Date(start.getTime() + 6 * 86400000)

  const startStr = start.toLocaleDateString('en-AE', {
    month: 'short',
    day: 'numeric',
    timeZone: 'UTC',
  })
  const endStr = end.toLocaleDateString('en-AE', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    timeZone: 'UTC',
  })

  return `${startStr} – ${endStr}`
}

/** Get the Sunday of the current week in Asia/Dubai */
function getCurrentWeekStart(): string {
  const today = getTodayDubai()
  const d = parseDate(today)
  const dow = d.getUTCDay() // 0=Sun
  d.setUTCDate(d.getUTCDate() - dow)
  return formatDate(d)
}

// ─── Component ────────────────────────────────────────────────────────────────

export function WeekNavigator({
  weekStart,
  selectedDay,
  onDaySelect,
  onWeekChange,
  closedDays = [5, 6],
}: WeekNavigatorProps) {
  const today = getTodayDubai()
  const currentWeekStart = getCurrentWeekStart()

  // +4 weeks from now is the maximum future week
  const maxWeekStart = addDays(currentWeekStart, 28)

  const isPrevDisabled = weekStart <= currentWeekStart
  const isNextDisabled = weekStart >= maxWeekStart

  function handlePrev() {
    if (!isPrevDisabled) {
      onWeekChange(addDays(weekStart, -7))
    }
  }

  function handleNext() {
    if (!isNextDisabled) {
      onWeekChange(addDays(weekStart, 7))
    }
  }

  // Build the 7 days of the current week
  const days = Array.from({ length: 7 }, (_, i) => {
    const dateStr = addDays(weekStart, i)
    const date = parseDate(dateStr)
    const dow = date.getUTCDay()
    return {
      dateStr,
      dayName: DAY_NAMES[dow],
      dayNum: date.getUTCDate(),
      dow,
    }
  })

  // ─── Touch swipe support ────────────────────────────────────────────────────

  const touchStartX = useRef<number | null>(null)
  const SWIPE_THRESHOLD = 50

  function handleTouchStart(e: React.TouchEvent) {
    touchStartX.current = e.touches[0].clientX
  }

  function handleTouchEnd(e: React.TouchEvent) {
    if (touchStartX.current === null) return
    const dx = e.changedTouches[0].clientX - touchStartX.current
    touchStartX.current = null

    if (dx > SWIPE_THRESHOLD && !isPrevDisabled) {
      handlePrev()
    } else if (dx < -SWIPE_THRESHOLD && !isNextDisabled) {
      handleNext()
    }
  }

  return (
    <div
      className="select-none"
      data-testid="week-navigator"
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
    >
      {/* Header: date range + arrows */}
      <div className="flex items-center justify-between mb-3">
        <button
          type="button"
          onClick={handlePrev}
          disabled={isPrevDisabled}
          data-testid="week-prev-btn"
          aria-label="Previous week"
          className={cn(
            'flex items-center justify-center w-9 h-9 rounded-full transition-colors',
            isPrevDisabled
              ? 'text-muted-foreground/40 cursor-not-allowed'
              : 'text-muted-foreground hover:bg-[#f4f6f9] cursor-pointer'
          )}
        >
          <ChevronLeft className="h-5 w-5" />
        </button>

        <span
          className="text-sm font-semibold text-[#061e44]"
          data-testid="week-range-label"
        >
          {formatWeekRange(weekStart)}
        </span>

        <button
          type="button"
          onClick={handleNext}
          disabled={isNextDisabled}
          data-testid="week-next-btn"
          aria-label="Next week"
          className={cn(
            'flex items-center justify-center w-9 h-9 rounded-full transition-colors',
            isNextDisabled
              ? 'text-muted-foreground/40 cursor-not-allowed'
              : 'text-muted-foreground hover:bg-[#f4f6f9] cursor-pointer'
          )}
        >
          <ChevronRight className="h-5 w-5" />
        </button>
      </div>

      {/* Day tabs */}
      <div
        className="grid grid-cols-7 gap-1"
        role="tablist"
        aria-label="Select a day"
        data-testid="week-days-row"
      >
        {days.map(({ dateStr, dayName, dayNum, dow }) => {
          const isClosed = closedDays.includes(dow)
          const isPast = dateStr < today
          const isDisabled = isClosed || isPast
          const isSelected = selectedDay === dateStr
          const isToday = dateStr === today

          return (
            <button
              key={dateStr}
              type="button"
              role="tab"
              aria-selected={isSelected}
              disabled={isDisabled}
              onClick={() => !isDisabled && onDaySelect(dateStr)}
              data-testid={`week-day-${dateStr}`}
              aria-label={`${dayName} ${dayNum}${isClosed ? ' (closed)' : isPast ? ' (past)' : ''}`}
              className={cn(
                // Base — 44px tall for touch
                'flex flex-col items-center justify-center min-h-[44px] rounded-xl px-1 py-1.5 transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#303ef5]',
                isDisabled
                  ? 'cursor-not-allowed opacity-40'
                  : 'cursor-pointer',
                isSelected
                  ? 'bg-[#303ef5] text-white shadow-sm'
                  : isToday && !isDisabled
                  ? 'bg-[#eef2fd]/50 border border-[#303ef5]/40 text-[#2530c4]'
                  : isDisabled
                  ? 'bg-[#f4f6f9] text-muted-foreground'
                  : 'hover:bg-[#f4f6f9] text-[#061e44]'
              )}
            >
              <span className="text-[10px] font-medium uppercase tracking-wide leading-none">
                {dayName}
              </span>
              <span className="text-sm font-bold leading-none mt-0.5">
                {dayNum}
              </span>
              {isToday && (
                <span
                  className={cn(
                    'mt-0.5 w-1 h-1 rounded-full',
                    isSelected ? 'bg-white' : 'bg-[#303ef5]'
                  )}
                  aria-hidden="true"
                />
              )}
            </button>
          )
        })}
      </div>
    </div>
  )
}

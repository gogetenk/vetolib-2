'use client'

import { useEffect, useReducer, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { Sparkles, RefreshCw } from 'lucide-react'
import { cn } from '@/lib/utils'
import { suggestSlots } from '@/lib/api/booking'
import type { SlotSuggestionDto } from '@/lib/api/booking'

// ─── Types ────────────────────────────────────────────────────────────────────

interface RecommendedSlotsProps {
  /** YYYY-MM-DD — the search window start for suggestions */
  fromDate: string
  /** YYYY-MM-DD — the search window end (defaults to +14 days from fromDate) */
  toDate?: string
  /** Pre-filter by vet (optional — if the user already chose a vet) */
  vetId?: string
  /** Reason / chief complaint for context */
  reason?: string
  /** ISO datetime of the currently selected slot start */
  selectedSlotStart?: string | null
  /** Called when user clicks a suggestion chip */
  onSelect: (slot: SlotSuggestionDto) => void
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

function formatSuggestionLabel(slot: SlotSuggestionDto): string {
  const date = new Date(slot.startsAt).toLocaleDateString('en-AE', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    timeZone: 'Asia/Dubai',
  })
  const time = new Date(slot.startsAt).toLocaleTimeString('en-AE', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
    timeZone: 'Asia/Dubai',
  })
  return `${date} · ${time}`
}

function addDays(dateStr: string, n: number): string {
  const [y, m, d] = dateStr.split('-').map(Number)
  const date = new Date(Date.UTC(y, m - 1, d + n))
  return date.toLocaleDateString('en-CA', { timeZone: 'UTC' })
}

// ─── Component ────────────────────────────────────────────────────────────────

export function RecommendedSlots({
  fromDate,
  toDate,
  vetId,
  reason,
  selectedSlotStart,
  onSelect,
}: RecommendedSlotsProps) {
  type FetchState = {
    suggestions: SlotSuggestionDto[]
    isLoading: boolean
    hasError: boolean
  }
  type FetchAction =
    | { type: 'loading' }
    | { type: 'success'; payload: SlotSuggestionDto[] }
    | { type: 'error' }

  function fetchReducer(_state: FetchState, action: FetchAction): FetchState {
    switch (action.type) {
      case 'loading':
        return { suggestions: [], isLoading: true, hasError: false }
      case 'success':
        return { suggestions: action.payload, isLoading: false, hasError: false }
      case 'error':
        return { suggestions: [], isLoading: false, hasError: true }
    }
  }

  const t = useTranslations('portal.booking.recommendedSlots')

  const [{ suggestions, isLoading, hasError }, dispatch] = useReducer(fetchReducer, {
    suggestions: [],
    isLoading: true,
    hasError: false,
  })

  const resolvedToDate = toDate ?? addDays(fromDate, 14)
  const [retryCount, setRetryCount] = useReducer((c: number) => c + 1, 0)

  const handleRetry = useCallback(() => {
    dispatch({ type: 'loading' })
    setRetryCount()
  }, [])

  useEffect(() => {
    let cancelled = false

    suggestSlots({
      from: fromDate,
      to: resolvedToDate,
      vetId,
      reason,
    })
      .then((data) => {
        if (!cancelled) dispatch({ type: 'success', payload: data.slice(0, 3) })
      })
      .catch(() => {
        if (!cancelled) dispatch({ type: 'error' })
      })

    return () => { cancelled = true }
  }, [fromDate, resolvedToDate, vetId, reason, retryCount])

  // Loading skeletons
  if (isLoading) {
    return (
      <div
        className="flex flex-wrap gap-2"
        data-testid="recommended-slots-loading"
        aria-busy="true"
        aria-label="Loading recommended slots"
      >
        {[1, 2, 3].map((i) => (
          <div
            key={i}
            className="h-9 w-44 rounded-full bg-muted animate-pulse"
            data-testid={`recommended-slot-skeleton-${i}`}
          />
        ))}
      </div>
    )
  }

  // Error state — show message with retry
  if (hasError) {
    return (
      <div
        className="flex items-center gap-2 text-xs text-destructive py-1"
        data-testid="recommended-slots-error"
      >
        <span>{t('load_error')}</span>
        <button
          type="button"
          onClick={handleRetry}
          data-testid="recommended-slots-retry"
          className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:text-primary/80 underline underline-offset-2 transition-colors"
        >
          <RefreshCw className="h-3 w-3" aria-hidden="true" />
          Retry
        </button>
      </div>
    )
  }

  // Empty state — no suggestions available
  if (suggestions.length === 0) {
    return (
      <div
        className="text-xs text-muted-foreground italic py-1"
        data-testid="recommended-slots-empty"
      >
        {t('empty')}
      </div>
    )
  }

  return (
    <div
      className="space-y-2"
      data-testid="recommended-slots"
    >
      {/* Section label */}
      <div className="flex items-center gap-1.5 text-xs font-medium text-primary/90">
        <Sparkles className="h-3.5 w-3.5" aria-hidden="true" />
        <span>{t('title')}</span>
      </div>

      {/* Chips */}
      <div className="flex flex-wrap gap-2" role="list" aria-label="Recommended appointment slots">
        {suggestions.map((slot, idx) => {
          const isSelected = selectedSlotStart === slot.startsAt

          return (
            <button
              key={slot.startsAt}
              type="button"
              onClick={() => onSelect(slot)}
              data-testid={`recommended-slot-${idx}`}
              aria-pressed={isSelected}
              aria-label={`${slot.vetName} — ${formatSuggestionLabel(slot)}`}
              className={cn(
                // Base chip — touch-friendly min height
                'inline-flex flex-col items-start justify-center px-3 py-2 rounded-full border text-xs font-medium transition-all min-h-[44px] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-1 cursor-pointer',
                isSelected
                  ? 'bg-primary border-primary text-white shadow-sm'
                  : 'bg-white border-primary/20 text-primary/90 hover:bg-primary/5 hover:border-primary/60'
              )}
            >
              {/* Vet name */}
              <span
                className="font-semibold leading-none mb-0.5"
                data-testid={`recommended-slot-vet-${idx}`}
              >
                {slot.vetName}
              </span>
              {/* Date + time */}
              <span
                className={cn('leading-none', isSelected ? 'text-primary-foreground' : 'text-muted-foreground')}
                data-testid={`recommended-slot-datetime-${idx}`}
              >
                {formatSuggestionLabel(slot)}
              </span>
            </button>
          )
        })}
      </div>
    </div>
  )
}

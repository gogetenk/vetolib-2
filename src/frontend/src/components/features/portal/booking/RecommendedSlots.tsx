'use client'

import { useEffect, useReducer } from 'react'
import { Sparkles } from 'lucide-react'
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
    | { type: 'success'; payload: SlotSuggestionDto[] }
    | { type: 'error' }

  function fetchReducer(_state: FetchState, action: FetchAction): FetchState {
    switch (action.type) {
      case 'success':
        return { suggestions: action.payload, isLoading: false, hasError: false }
      case 'error':
        return { suggestions: [], isLoading: false, hasError: true }
    }
  }

  const [{ suggestions, isLoading, hasError }, dispatch] = useReducer(fetchReducer, {
    suggestions: [],
    isLoading: true,
    hasError: false,
  })

  const resolvedToDate = toDate ?? addDays(fromDate, 14)

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
  }, [fromDate, resolvedToDate, vetId, reason])

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
            className="h-9 w-44 rounded-full bg-gray-200 animate-pulse"
            data-testid={`recommended-slot-skeleton-${i}`}
          />
        ))}
      </div>
    )
  }

  // Error state — silent fail, no chips shown
  if (hasError || suggestions.length === 0) {
    return (
      <div
        className="text-xs text-gray-400 italic py-1"
        data-testid="recommended-slots-empty"
      >
        No suggestions available right now.
      </div>
    )
  }

  return (
    <div
      className="space-y-2"
      data-testid="recommended-slots"
    >
      {/* Section label */}
      <div className="flex items-center gap-1.5 text-xs font-medium text-emerald-700">
        <Sparkles className="h-3.5 w-3.5" aria-hidden="true" />
        <span>Recommended slots</span>
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
                'inline-flex flex-col items-start justify-center px-3 py-2 rounded-full border text-xs font-medium transition-all min-h-[44px] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-1 cursor-pointer',
                isSelected
                  ? 'bg-emerald-600 border-emerald-600 text-white shadow-sm'
                  : 'bg-white border-emerald-200 text-emerald-700 hover:bg-emerald-50 hover:border-emerald-400'
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
                className={cn('leading-none', isSelected ? 'text-emerald-100' : 'text-gray-500')}
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

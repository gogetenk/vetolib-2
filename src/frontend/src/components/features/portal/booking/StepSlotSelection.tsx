'use client'

import { useEffect, useReducer } from 'react'
import { useTranslations } from 'next-intl'
import { getWeekSlots } from '@/lib/api/booking'
import { WeekNavigator } from './WeekNavigator'
import { SlotGrid } from './SlotGrid'
import { RecommendedSlots } from './RecommendedSlots'
import type { BookingSlot, BookingDay, SlotSuggestionDto } from '@/lib/api/booking'

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Get today's date in Asia/Dubai as YYYY-MM-DD */
function getTodayDubai(): string {
  return new Date().toLocaleDateString('en-CA', { timeZone: 'Asia/Dubai' })
}

/** Get the Sunday of the current week in Asia/Dubai */
function getCurrentWeekStart(): string {
  const today = getTodayDubai()
  const [y, m, d] = today.split('-').map(Number)
  const date = new Date(Date.UTC(y, m - 1, d))
  const dow = date.getUTCDay()
  date.setUTCDate(date.getUTCDate() - dow)
  const yr = date.getUTCFullYear()
  const mo = String(date.getUTCMonth() + 1).padStart(2, '0')
  const dy = String(date.getUTCDate()).padStart(2, '0')
  return `${yr}-${mo}-${dy}`
}

// ─── Types ────────────────────────────────────────────────────────────────────

interface StepSlotSelectionProps {
  selectedSlot: BookingSlot | null
  vetId: string | null
  reason: string
  onSlotSelect: (slot: BookingSlot) => void
}

// ─── Component ────────────────────────────────────────────────────────────────

export function StepSlotSelection({
  selectedSlot,
  vetId,
  reason,
  onSlotSelect,
}: StepSlotSelectionProps) {
  const t = useTranslations('portal.booking.wizard.slotSection')

  type SlotState = {
    weekStart: string
    selectedDay: string | null
    weekDays: BookingDay[]
    isLoading: boolean
    hasError: boolean
  }
  type SlotAction =
    | { type: 'CHANGE_WEEK'; weekStart: string }
    | { type: 'SELECT_DAY'; day: string }
    | { type: 'FETCH_SUCCESS'; days: BookingDay[] }
    | { type: 'FETCH_ERROR' }
    | { type: 'FETCH_START' }

  function slotReducer(state: SlotState, action: SlotAction): SlotState {
    switch (action.type) {
      case 'CHANGE_WEEK':
        return { ...state, weekStart: action.weekStart, selectedDay: null, isLoading: true, hasError: false }
      case 'FETCH_START':
        return { ...state, isLoading: true, hasError: false }
      case 'SELECT_DAY':
        return { ...state, selectedDay: action.day }
      case 'FETCH_SUCCESS':
        return { ...state, weekDays: action.days, isLoading: false }
      case 'FETCH_ERROR':
        return { ...state, hasError: true, isLoading: false }
    }
  }

  const [state, dispatch] = useReducer(slotReducer, {
    weekStart: getCurrentWeekStart(),
    selectedDay: null,
    weekDays: [],
    isLoading: true,
    hasError: false,
  })

  const { weekStart, selectedDay, weekDays, isLoading, hasError } = state

  useEffect(() => {
    let cancelled = false

    getWeekSlots({ weekStart, vetId: vetId ?? undefined })
      .then((days) => {
        if (!cancelled) dispatch({ type: 'FETCH_SUCCESS', days })
      })
      .catch(() => {
        if (!cancelled) dispatch({ type: 'FETCH_ERROR' })
      })

    return () => { cancelled = true }
  }, [weekStart, vetId])

  function handleWeekChange(newWeekStart: string) {
    dispatch({ type: 'CHANGE_WEEK', weekStart: newWeekStart })
  }

  function handleDaySelect(day: string) {
    dispatch({ type: 'SELECT_DAY', day })
  }

  // Get slots for the selected day
  const dayData = weekDays.find((d) => d.date === selectedDay)
  const slotsForDay = dayData?.slots ?? []

  // Handle a suggestion click — we need to find the matching slot or synthesize one
  function handleSuggestionSelect(suggestion: SlotSuggestionDto) {
    const syntheticSlot: BookingSlot = {
      startsAt: suggestion.startsAt,
      endsAt: suggestion.endsAt,
      vetId: suggestion.vetId,
      vetName: suggestion.vetName,
      available: true,
    }
    onSlotSelect(syntheticSlot)
  }

  const today = getTodayDubai()

  return (
    <div className="space-y-6" data-testid="step-slot-selection">
      {/* Recommended slots */}
      <div data-testid="recommended-slots-section">
        <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">
          {t('suggestedTitle')}
        </p>
        <RecommendedSlots
          fromDate={today}
          vetId={vetId ?? undefined}
          reason={reason || undefined}
          selectedSlotStart={selectedSlot?.startsAt ?? null}
          onSelect={handleSuggestionSelect}
        />
      </div>

      {/* Divider */}
      <div className="relative">
        <div className="absolute inset-0 flex items-center" aria-hidden="true">
          <div className="w-full border-t border-gray-200" />
        </div>
        <div className="relative flex justify-center text-xs">
          <span className="bg-white px-2 text-gray-400">{t('browseTitle')}</span>
        </div>
      </div>

      {/* Week navigator */}
      <WeekNavigator
        weekStart={weekStart}
        selectedDay={selectedDay}
        onDaySelect={handleDaySelect}
        onWeekChange={handleWeekChange}
        closedDays={[5, 6]}
      />

      {/* Slot grid */}
      {selectedDay === null ? (
        <div
          className="flex items-center justify-center py-10 text-sm text-gray-400"
          data-testid="slot-selection-prompt"
        >
          {t('selectDayPrompt')}
        </div>
      ) : isLoading ? (
        <div
          className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-6 gap-2"
          data-testid="slot-grid-loading"
          aria-busy="true"
        >
          {Array.from({ length: 12 }, (_, i) => (
            <div
              key={i}
              className="h-11 rounded-lg bg-gray-200 animate-pulse"
              data-testid={`slot-skeleton-${i}`}
            />
          ))}
        </div>
      ) : hasError ? (
        <div
          className="text-sm text-red-600 text-center py-6"
          data-testid="slot-grid-error"
        >
          {t('loadError')}
        </div>
      ) : dayData?.closed ? (
        <div
          className="text-sm text-gray-500 text-center py-6"
          data-testid="slot-grid-closed"
        >
          {t('clinicClosed')}
        </div>
      ) : (
        <div data-testid="slot-grid-section">
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">
            {t('availableTimesTitle')}
          </p>
          <SlotGrid
            slots={slotsForDay}
            selectedSlotStart={selectedSlot?.startsAt ?? null}
            onSelect={onSlotSelect}
            hideVetName={vetId !== null}
          />
        </div>
      )}
    </div>
  )
}

'use client'

import { useEffect, useState, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { getAvailability } from '@/lib/api/booking'

interface StepSlotSelectionProps {
  clinicSlug: string
  consultationTypeId: string
  veterinarianId: string | null
  selectedDate: string | null
  selectedTime: string | null
  onSlotSelect: (date: string, time: string) => void
}

// UAE work week: Sun=0, Mon=1, Tue=2, Wed=3, Thu=4
const UAE_WORK_DAYS = [0, 1, 2, 3, 4]

function getWorkingDays(startDate: Date, count: number): Date[] {
  const days: Date[] = []
  const current = new Date(startDate)
  current.setHours(0, 0, 0, 0)
  while (days.length < count) {
    if (UAE_WORK_DAYS.includes(current.getDay())) {
      days.push(new Date(current))
    }
    current.setDate(current.getDate() + 1)
  }
  return days
}

function toDateString(d: Date): string {
  return d.toISOString().split('T')[0]
}

function formatTimeDisplay(time: string): string {
  const [hour, minute] = time.split(':').map(Number)
  const ampm = hour >= 12 ? 'PM' : 'AM'
  const h = hour % 12 || 12
  return `${h}:${String(minute).padStart(2, '0')} ${ampm}`
}

export function StepSlotSelection({
  clinicSlug,
  consultationTypeId,
  veterinarianId,
  selectedDate,
  selectedTime,
  onSlotSelect,
}: StepSlotSelectionProps) {
  const t = useTranslations('portal.booking.wizard.slot_selection')

  const [weekOffset, setWeekOffset] = useState(0)
  const [activeDay, setActiveDay] = useState<string | null>(null)
  const [slots, setSlots] = useState<{ time: string; isAvailable: boolean }[]>([])
  const [loading, setLoading] = useState(false)

  const today = new Date()
  const startDate = new Date(today)
  startDate.setDate(today.getDate() + weekOffset * 5)
  const workDays = getWorkingDays(startDate, 5)

  // Initialize active day
  useEffect(() => {
    const firstDay = toDateString(workDays[0])
    if (selectedDate && weekOffset === 0) {
      setActiveDay(selectedDate)
    } else {
      setActiveDay(firstDay)
    }
  }, [weekOffset]) // eslint-disable-line react-hooks/exhaustive-deps

  const loadSlots = useCallback(
    async (date: string) => {
      setLoading(true)
      setSlots([])
      try {
        const data = await getAvailability(clinicSlug, {
          date,
          consultationTypeId,
          veterinarianId,
        })
        setSlots(data.slots)
      } catch {
        setSlots([])
      } finally {
        setLoading(false)
      }
    },
    [clinicSlug, consultationTypeId, veterinarianId]
  )

  useEffect(() => {
    if (activeDay) loadSlots(activeDay)
  }, [activeDay, loadSlots])

  const availableCount = slots.filter(s => s.isAvailable).length

  return (
    <div data-testid="slot-selection-step" className="space-y-4">
      {/* Day tabs */}
      <div className="flex items-center gap-1">
        <button
          type="button"
          onClick={() => setWeekOffset(o => Math.max(0, o - 1))}
          disabled={weekOffset === 0}
          className="p-1.5 rounded-md hover:bg-muted disabled:opacity-30 disabled:cursor-not-allowed flex-shrink-0"
          aria-label="Previous period"
        >
          <ChevronLeft className="h-4 w-4" />
        </button>

        <div data-testid="day-tabs" className="flex-1 flex gap-1 overflow-x-auto">
          {workDays.map(day => {
            const dateStr = toDateString(day)
            const isActive = dateStr === activeDay
            const isSelected = dateStr === selectedDate

            return (
              <button
                key={dateStr}
                data-testid={`day-tab-${dateStr}`}
                type="button"
                onClick={() => setActiveDay(dateStr)}
                className={[
                  'flex-1 flex flex-col items-center px-2 py-2 rounded-lg text-xs border transition-all min-w-[52px]',
                  'focus:outline-none focus:ring-2 focus:ring-primary',
                  isSelected
                    ? 'border-primary bg-primary text-primary-foreground font-semibold'
                    : isActive
                    ? 'border-primary/50 bg-primary/10 text-primary font-medium'
                    : 'border-border hover:border-primary/30 hover:bg-muted',
                ].join(' ')}
              >
                <span className="font-medium">
                  {day.toLocaleDateString('en-AE', { weekday: 'short', timeZone: 'Asia/Dubai' })}
                </span>
                <span className="text-[10px] opacity-80 mt-0.5">
                  {day.toLocaleDateString('en-AE', { day: 'numeric', month: 'short', timeZone: 'Asia/Dubai' })}
                </span>
              </button>
            )
          })}
        </div>

        <button
          type="button"
          onClick={() => setWeekOffset(o => o + 1)}
          className="p-1.5 rounded-md hover:bg-muted flex-shrink-0"
          aria-label="Next period"
        >
          <ChevronRight className="h-4 w-4" />
        </button>
      </div>

      {/* Slots */}
      {loading ? (
        <div className="space-y-2">
          <p className="text-sm text-muted-foreground animate-pulse">{t('loading')}</p>
          <div className="grid grid-cols-3 sm:grid-cols-4 gap-2">
            {[1, 2, 3, 4, 5, 6].map(i => (
              <div key={i} className="h-10 rounded-md bg-muted animate-pulse" />
            ))}
          </div>
        </div>
      ) : availableCount === 0 ? (
        <p className="text-sm text-muted-foreground py-6 text-center">{t('no_slots')}</p>
      ) : (
        <div data-testid="slots-grid" className="grid grid-cols-3 sm:grid-cols-4 gap-2">
          {slots.map(slot => {
            const isSelected = activeDay === selectedDate && slot.time === selectedTime
            return (
              <button
                key={slot.time}
                data-testid={`slot-btn-${activeDay}-${slot.time}`}
                type="button"
                disabled={!slot.isAvailable}
                onClick={() => {
                  if (slot.isAvailable && activeDay) {
                    onSlotSelect(activeDay, slot.time)
                  }
                }}
                className={[
                  'rounded-md border px-2 py-2 text-xs font-medium transition-all',
                  'focus:outline-none focus:ring-2 focus:ring-primary',
                  !slot.isAvailable
                    ? 'border-border bg-muted text-muted-foreground cursor-not-allowed opacity-40'
                    : isSelected
                    ? 'border-primary bg-primary text-primary-foreground'
                    : 'border-border hover:border-primary hover:bg-primary/5',
                ].join(' ')}
              >
                {formatTimeDisplay(slot.time)}
              </button>
            )
          })}
        </div>
      )}

      {selectedDate && selectedTime && (
        <div
          data-testid="selected-slot-summary"
          className="rounded-lg border border-primary/30 bg-primary/5 px-4 py-3"
        >
          <p className="text-sm font-medium text-primary">
            {new Date(selectedDate + 'T00:00:00').toLocaleDateString('en-AE', {
              weekday: 'long',
              month: 'long',
              day: 'numeric',
              timeZone: 'Asia/Dubai',
            })}{' '}
            at {formatTimeDisplay(selectedTime)}
          </p>
        </div>
      )}
    </div>
  )
}

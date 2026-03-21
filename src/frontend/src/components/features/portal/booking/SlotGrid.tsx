'use client'

import { useTranslations } from 'next-intl'
import { Check } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { BookingSlot } from '@/lib/api/booking'

// ─── Types ────────────────────────────────────────────────────────────────────

interface SlotGridProps {
  slots: BookingSlot[]
  /** ISO string of the currently selected slot start */
  selectedSlotStart?: string | null
  /** Called with the selected slot when user clicks an available slot */
  onSelect: (slot: BookingSlot) => void
  /**
   * When true, the vet name is hidden per slot (user already chose a vet).
   * When false/undefined, the vet name is shown on each slot chip.
   */
  hideVetName?: boolean
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

function formatSlotTime(isoStr: string): string {
  return new Date(isoStr).toLocaleTimeString('en-AE', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
    timeZone: 'Asia/Dubai',
  })
}

// ─── Component ────────────────────────────────────────────────────────────────

export function SlotGrid({
  slots,
  selectedSlotStart,
  onSelect,
  hideVetName = false,
}: SlotGridProps) {
  const t = useTranslations('portal.booking.slotGrid')

  if (slots.length === 0) {
    return (
      <div
        className="flex flex-col items-center justify-center py-12 text-muted-foreground"
        data-testid="slot-grid-empty"
      >
        <span className="text-sm font-medium">{t('noSlots')}</span>
      </div>
    )
  }

  return (
    <div
      className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-6 gap-2"
      data-testid="slot-grid"
    >
      {slots.map((slot) => {
        const isSelected = selectedSlotStart === slot.startsAt
        const isAvailable = slot.available

        return (
          <button
            key={slot.startsAt}
            type="button"
            disabled={!isAvailable}
            onClick={() => isAvailable && onSelect(slot)}
            data-testid={`slot-${slot.startsAt}`}
            aria-pressed={isSelected}
            aria-label={`${formatSlotTime(slot.startsAt)}${!hideVetName ? ` with ${slot.vetName}` : ''} — ${isAvailable ? 'available' : 'unavailable'}`}
            className={cn(
              // Base — touch-friendly minimum 44x44px
              'relative flex flex-col items-center justify-center rounded-xl px-1 py-2 min-h-[44px] text-xs font-medium transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-1',
              isAvailable
                ? isSelected
                  ? // Selected state
                    'bg-primary/5 border-2 border-primary text-primary/90 shadow-md scale-[1.05]'
                  : // Available unselected
                    'bg-primary/5 border border-primary/20 text-primary/90 hover:bg-primary/10 hover:border-primary/60 hover:shadow-sm hover:scale-[1.03] cursor-pointer'
                : // Unavailable
                  'bg-muted border border-border/80 text-muted-foreground cursor-not-allowed opacity-60'
            )}
          >
            {/* Check icon for selected slot */}
            {isSelected && (
              <Check
                className="absolute top-0.5 right-0.5 h-3 w-3 text-primary"
                data-testid={`slot-check-${slot.startsAt}`}
                aria-hidden="true"
              />
            )}

            {/* Time */}
            <span data-testid={`slot-time-${slot.startsAt}`}>
              {formatSlotTime(slot.startsAt)}
            </span>

            {/* Vet name — shown only when no vet preference set */}
            {!hideVetName && (
              <span
                className="mt-0.5 text-[10px] leading-tight text-center text-muted-foreground truncate w-full text-center"
                data-testid={`slot-vet-${slot.startsAt}`}
              >
                {slot.vetName.replace('Dr. ', '')}
              </span>
            )}
          </button>
        )
      })}
    </div>
  )
}

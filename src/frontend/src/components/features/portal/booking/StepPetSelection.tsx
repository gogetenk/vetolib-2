'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { PawPrint, Check } from 'lucide-react'
import { cn } from '@/lib/utils'
import { listPortalBookingPets } from '@/lib/api/booking'
import type { BookingPetDto } from '@/lib/api/booking'

// ─── Types ────────────────────────────────────────────────────────────────────

interface StepPetSelectionProps {
  selectedPetId: string | null
  onSelect: (pet: BookingPetDto) => void
}

// ─── Component ────────────────────────────────────────────────────────────────

export function StepPetSelection({ selectedPetId, onSelect }: StepPetSelectionProps) {
  const t = useTranslations('portal.booking.wizard.petSection')
  const [pets, setPets] = useState<BookingPetDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [hasError, setHasError] = useState(false)

  useEffect(() => {
    let cancelled = false

    listPortalBookingPets()
      .then((data) => {
        if (cancelled) return
        setPets(data)
        setIsLoading(false)
        // Auto-select if only one pet
        if (data.length === 1 && !selectedPetId) {
          onSelect(data[0])
        }
      })
      .catch(() => {
        if (!cancelled) {
          setHasError(true)
          setIsLoading(false)
        }
      })

    return () => { cancelled = true }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  if (isLoading) {
    return (
      <div
        className="grid grid-cols-1 sm:grid-cols-2 gap-3"
        data-testid="step-pet-selection-loading"
        aria-busy="true"
      >
        {[1, 2].map((i) => (
          <div
            key={i}
            className="flex items-center gap-4 rounded-xl border-2 border-border/50 p-4"
            data-testid={`pet-card-skeleton-${i}`}
          >
            <div className="h-12 w-12 rounded-full bg-muted animate-pulse" />
            <div className="flex-1 space-y-2">
              <div className="h-4 w-24 rounded bg-muted animate-pulse" />
              <div className="h-3 w-32 rounded bg-muted animate-pulse" />
              <div className="h-3 w-16 rounded bg-muted animate-pulse" />
            </div>
          </div>
        ))}
      </div>
    )
  }

  if (hasError) {
    return (
      <div
        className="text-sm text-red-600 text-center py-8"
        data-testid="step-pet-selection-error"
      >
        {t('loadError')}
      </div>
    )
  }

  if (pets.length === 0) {
    return (
      <div
        className="text-sm text-muted-foreground text-center py-8"
        data-testid="step-pet-selection-empty"
      >
        {t('noPets')}
      </div>
    )
  }

  return (
    <div
      className="grid grid-cols-1 sm:grid-cols-2 gap-3"
      data-testid="step-pet-selection"
      role="listbox"
      aria-label={t('selectPet')}
    >
      {pets.map((pet) => {
        const isSelected = selectedPetId === pet.id

        return (
          <button
            key={pet.id}
            type="button"
            role="option"
            onClick={() => onSelect(pet)}
            data-testid={`pet-card-${pet.id}`}
            aria-selected={isSelected}
            aria-label={`${pet.name}, ${pet.species}, ${pet.breed}, ${pet.ageYears} year${pet.ageYears !== 1 ? 's' : ''} old`}
            className={cn(
              'relative flex items-center gap-4 rounded-xl border-2 p-4 text-left transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-1 cursor-pointer',
              isSelected
                ? 'border-primary bg-primary/5 shadow-md scale-[1.02]'
                : 'border-border/80 bg-white hover:border-primary/40 hover:bg-primary/5 hover:shadow-sm'
            )}
          >
            {/* Icon */}
            <div
              className={cn(
                'flex h-12 w-12 shrink-0 items-center justify-center rounded-full',
                isSelected ? 'bg-primary/10' : 'bg-muted'
              )}
              aria-hidden="true"
            >
              <PawPrint
                className={cn('h-6 w-6', isSelected ? 'text-primary' : 'text-muted-foreground')}
              />
            </div>

            {/* Info */}
            <div className="min-w-0 flex-1">
              <p
                className={cn('font-semibold truncate', isSelected ? 'text-foreground' : 'text-foreground')}
                data-testid={`pet-card-name-${pet.id}`}
              >
                {pet.name}
              </p>
              <p
                className="text-xs text-muted-foreground truncate mt-0.5"
                data-testid={`pet-card-details-${pet.id}`}
              >
                {pet.species} · {pet.breed}
              </p>
              <p className="text-xs text-muted-foreground mt-0.5" data-testid={`pet-card-age-${pet.id}`}>
                {pet.ageYears} year{pet.ageYears !== 1 ? 's' : ''} old
              </p>
            </div>

            {/* Selected check */}
            <div
              className={cn(
                'absolute top-2 right-2 flex h-5 w-5 items-center justify-center rounded-full transition-all duration-300',
                isSelected
                  ? 'bg-primary scale-100 opacity-100'
                  : 'bg-transparent scale-0 opacity-0'
              )}
              data-testid={isSelected ? `pet-card-check-${pet.id}` : undefined}
              aria-hidden="true"
            >
              <Check className="h-3 w-3 text-white" />
            </div>
          </button>
        )
      })}
    </div>
  )
}

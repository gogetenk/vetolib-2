'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { PawPrint, Check } from 'lucide-react'
import { cn } from '@/lib/utils'
import { listPortalPets } from '@/lib/api/booking'
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

    listPortalPets()
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
            className="h-24 rounded-xl bg-gray-200 animate-pulse"
            data-testid={`pet-card-skeleton-${i}`}
          />
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
        className="text-sm text-gray-500 text-center py-8"
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
              'relative flex items-center gap-4 rounded-xl border-2 p-4 text-left transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-1 cursor-pointer',
              isSelected
                ? 'border-emerald-500 bg-emerald-50 shadow-sm'
                : 'border-gray-200 bg-white hover:border-emerald-300 hover:bg-emerald-50/30'
            )}
          >
            {/* Icon */}
            <div
              className={cn(
                'flex h-12 w-12 shrink-0 items-center justify-center rounded-full',
                isSelected ? 'bg-emerald-100' : 'bg-gray-100'
              )}
              aria-hidden="true"
            >
              <PawPrint
                className={cn('h-6 w-6', isSelected ? 'text-emerald-600' : 'text-gray-500')}
              />
            </div>

            {/* Info */}
            <div className="min-w-0 flex-1">
              <p
                className={cn('font-semibold truncate', isSelected ? 'text-emerald-800' : 'text-gray-900')}
                data-testid={`pet-card-name-${pet.id}`}
              >
                {pet.name}
              </p>
              <p
                className="text-xs text-gray-500 truncate mt-0.5"
                data-testid={`pet-card-details-${pet.id}`}
              >
                {pet.species} · {pet.breed}
              </p>
              <p className="text-xs text-gray-400 mt-0.5" data-testid={`pet-card-age-${pet.id}`}>
                {pet.ageYears} year{pet.ageYears !== 1 ? 's' : ''} old
              </p>
            </div>

            {/* Selected check */}
            {isSelected && (
              <Check
                className="absolute top-2 right-2 h-4 w-4 text-emerald-600"
                data-testid={`pet-card-check-${pet.id}`}
                aria-hidden="true"
              />
            )}
          </button>
        )
      })}
    </div>
  )
}

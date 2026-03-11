'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { CheckCircle2, PawPrint } from 'lucide-react'
import { listPortalPets } from '@/lib/api/portal'
import type { PortalPetDto } from '@/lib/api/messaging-types'

interface StepPetSelectionProps {
  selectedPetId: string | null
  onSelect: (petId: string, petName: string) => void
}

export function StepPetSelection({ selectedPetId, onSelect }: StepPetSelectionProps) {
  const t = useTranslations('portal.booking.wizard.pet_selection')
  const [pets, setPets] = useState<PortalPetDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [autoSelected, setAutoSelected] = useState(false)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    listPortalPets()
      .then(data => {
        if (cancelled) return
        setPets(data)
        if (data.length === 1 && !selectedPetId) {
          onSelect(data[0].id, data[0].name)
          setAutoSelected(true)
        }
      })
      .catch(() => {
        if (!cancelled) setError('Failed to load pets')
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => { cancelled = true }
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  if (loading) {
    return (
      <div data-testid="pet-selection-step" className="space-y-3">
        <p className="text-sm text-muted-foreground animate-pulse">{t('loading')}</p>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          {[1, 2].map(i => (
            <div key={i} className="h-24 rounded-lg bg-muted animate-pulse" />
          ))}
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div data-testid="pet-selection-step" className="text-destructive text-sm">
        {error}
      </div>
    )
  }

  return (
    <div data-testid="pet-selection-step" className="space-y-4">
      {pets.length === 0 ? (
        <p className="text-sm text-muted-foreground">{t('no_pets')}</p>
      ) : (
        <>
          {autoSelected && pets.length === 1 && (
            <p className="text-xs text-muted-foreground italic">{t('pre_selected')}</p>
          )}
          <div data-testid="pet-cards-list" className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {pets.map(pet => {
              const isSelected = pet.id === selectedPetId
              return (
                <button
                  key={pet.id}
                  data-testid={`pet-card-${pet.id}`}
                  type="button"
                  onClick={() => onSelect(pet.id, pet.name)}
                  className={[
                    'relative flex items-start gap-3 rounded-lg border p-4 text-left transition-all',
                    'hover:border-primary hover:bg-primary/5 focus:outline-none focus:ring-2 focus:ring-primary',
                    isSelected
                      ? 'border-primary bg-primary/10'
                      : 'border-border bg-card',
                  ].join(' ')}
                >
                  <div className="mt-0.5 rounded-full bg-muted p-2">
                    <PawPrint className="h-4 w-4 text-muted-foreground" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-medium text-sm truncate">{pet.name}</p>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      {pet.species}{pet.breed ? ` · ${pet.breed}` : ''}
                    </p>
                    {pet.ageYears > 0 && (
                      <p className="text-xs text-muted-foreground">
                        {pet.ageYears} yr{pet.ageYears !== 1 ? 's' : ''}
                      </p>
                    )}
                  </div>
                  {isSelected && (
                    <CheckCircle2
                      data-testid={`pet-selected-icon-${pet.id}`}
                      className="h-5 w-5 text-primary flex-shrink-0"
                    />
                  )}
                </button>
              )
            })}
          </div>
        </>
      )}
      <button
        data-testid="add-new-pet-btn"
        type="button"
        disabled
        className="text-sm text-muted-foreground underline underline-offset-2 opacity-50 cursor-not-allowed"
      >
        {t('add_pet')}
      </button>
    </div>
  )
}

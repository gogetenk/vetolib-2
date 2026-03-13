'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { Clock, ChevronDown, Check } from 'lucide-react'
import { cn } from '@/lib/utils'
import { listPortalConsultationTypes, listPortalVeterinarians } from '@/lib/api/booking'
import type { ConsultationTypeDto, VeterinarianDto } from '@/lib/api/booking'

// ─── Types ────────────────────────────────────────────────────────────────────

interface StepConsultationTypeProps {
  selectedTypeId: string | null
  selectedVetId: string | null
  reason: string
  onTypeSelect: (type: ConsultationTypeDto) => void
  onVetChange: (vetId: string | null) => void
  onReasonChange: (reason: string) => void
  /** Called once vets are loaded — lets the wizard cache the full vet list for name resolution */
  onVetsCached?: (vets: VeterinarianDto[]) => void
}

// ─── Component ────────────────────────────────────────────────────────────────

export function StepConsultationType({
  selectedTypeId,
  selectedVetId,
  reason,
  onTypeSelect,
  onVetChange,
  onReasonChange,
  onVetsCached,
}: StepConsultationTypeProps) {
  const t = useTranslations('portal.booking.wizard.consultationSection')
  const [types, setTypes] = useState<ConsultationTypeDto[]>([])
  const [vets, setVets] = useState<VeterinarianDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [hasError, setHasError] = useState(false)

  useEffect(() => {
    let cancelled = false

    Promise.all([listPortalConsultationTypes(), listPortalVeterinarians()])
      .then(([typesData, vetsData]) => {
        if (cancelled) return
        setTypes(typesData)
        setVets(vetsData)
        setIsLoading(false)
        onVetsCached?.(vetsData)
      })
      .catch(() => {
        if (!cancelled) {
          setHasError(true)
          setIsLoading(false)
        }
      })

    return () => { cancelled = true }
  }, [])

  if (isLoading) {
    return (
      <div
        className="space-y-3"
        data-testid="step-consultation-type-loading"
        aria-busy="true"
      >
        {[1, 2, 3].map((i) => (
          <div
            key={i}
            className="h-20 rounded-xl bg-gray-200 animate-pulse"
            data-testid={`consultation-type-skeleton-${i}`}
          />
        ))}
      </div>
    )
  }

  if (hasError) {
    return (
      <div
        className="text-sm text-red-600 text-center py-8"
        data-testid="step-consultation-type-error"
      >
        {t('loadError')}
      </div>
    )
  }

  return (
    <div className="space-y-6" data-testid="step-consultation-type">
      {/* Consultation type cards */}
      <div>
        <p className="text-sm font-medium text-gray-700 mb-2">{t('typeLabel')}</p>
        <div
          className="grid grid-cols-1 sm:grid-cols-2 gap-3"
          role="listbox"
          aria-label="Select consultation type"
          data-testid="consultation-type-grid"
        >
          {types.map((type) => {
            const isSelected = selectedTypeId === type.id

            return (
              <button
                key={type.id}
                type="button"
                role="option"
                onClick={() => onTypeSelect(type)}
                data-testid={`consultation-type-card-${type.id}`}
                aria-selected={isSelected}
                aria-label={`${type.name} — ${type.durationMinutes} minutes`}
                className={cn(
                  'relative flex flex-col items-start gap-1 rounded-xl border-2 p-4 text-left transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-1 cursor-pointer',
                  isSelected
                    ? 'border-emerald-500 bg-emerald-50 shadow-md scale-[1.02]'
                    : 'border-gray-200 bg-white hover:border-emerald-300 hover:bg-emerald-50/30 hover:shadow-sm'
                )}
              >
                <div
                  className={cn(
                    'absolute top-2 right-2 flex h-5 w-5 items-center justify-center rounded-full transition-all duration-300',
                    isSelected
                      ? 'bg-emerald-500 scale-100 opacity-100'
                      : 'bg-transparent scale-0 opacity-0'
                  )}
                  data-testid={isSelected ? `consultation-type-check-${type.id}` : undefined}
                  aria-hidden="true"
                >
                  <Check className="h-3 w-3 text-white" />
                </div>
                <p
                  className={cn('font-semibold text-sm', isSelected ? 'text-emerald-800' : 'text-gray-900')}
                  data-testid={`consultation-type-name-${type.id}`}
                >
                  {type.name}
                </p>
                <p
                  className="text-xs text-gray-500 line-clamp-2"
                  data-testid={`consultation-type-description-${type.id}`}
                >
                  {type.description}
                </p>
                <div className="flex items-center gap-1 mt-1">
                  <Clock className="h-3 w-3 text-gray-400" aria-hidden="true" />
                  <span className="text-xs text-gray-400" data-testid={`consultation-type-duration-${type.id}`}>
                    {type.durationMinutes} min
                  </span>
                </div>
              </button>
            )
          })}
        </div>
      </div>

      {/* Vet preference (optional) */}
      <div>
        <label
          htmlFor="vet-preference"
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          {t('vetLabel')}{' '}
          <span className="font-normal text-gray-400">{t('vetOptional')}</span>
        </label>
        <div className="relative">
          <select
            id="vet-preference"
            value={selectedVetId ?? ''}
            onChange={(e) => onVetChange(e.target.value || null)}
            data-testid="vet-preference-select"
            aria-label="Select a preferred veterinarian"
            className="w-full appearance-none rounded-lg border border-gray-300 bg-white px-3 py-2.5 pr-9 text-sm text-gray-900 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
          >
            <option value="">{t('vetNoPreference')}</option>
            {vets.map((vet) => (
              <option key={vet.id} value={vet.id} data-testid={`vet-option-${vet.id}`}>
                {vet.name}
              </option>
            ))}
          </select>
          <ChevronDown
            className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400"
            aria-hidden="true"
          />
        </div>
      </div>

      {/* Reason textarea */}
      <div>
        <label
          htmlFor="consultation-reason"
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          {t('reasonLabel')}{' '}
          <span className="font-normal text-gray-400">{t('reasonOptional')}</span>
        </label>
        <textarea
          id="consultation-reason"
          value={reason}
          onChange={(e) => onReasonChange(e.target.value)}
          placeholder={t('reasonPlaceholder')}
          rows={3}
          maxLength={500}
          data-testid="consultation-reason-textarea"
          aria-label="Reason for visit"
          className="w-full resize-none rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
        />
        <p className="mt-1 text-xs text-gray-400 text-right" aria-live="polite">
          {reason.length}/500
        </p>
      </div>
    </div>
  )
}

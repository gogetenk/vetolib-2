'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { AlertTriangle, Clock } from 'lucide-react'
import { getConsultationTypes, getVeterinarians } from '@/lib/api/booking'
import type { ConsultationTypeDto, VeterinarianDto } from '@/lib/api/booking-types'

interface StepConsultationTypeProps {
  clinicSlug: string
  selectedTypeId: string | null
  selectedVetId: string | null
  reason: string
  onTypeSelect: (typeId: string) => void
  onVetSelect: (vetId: string | null) => void
  onReasonChange: (reason: string) => void
}

const MAX_REASON_CHARS = 500

export function StepConsultationType({
  clinicSlug,
  selectedTypeId,
  selectedVetId,
  reason,
  onTypeSelect,
  onVetSelect,
  onReasonChange,
}: StepConsultationTypeProps) {
  const t = useTranslations('portal.booking.wizard.consultation_type')
  const [types, setTypes] = useState<ConsultationTypeDto[]>([])
  const [vets, setVets] = useState<VeterinarianDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    Promise.all([getConsultationTypes(clinicSlug), getVeterinarians(clinicSlug)])
      .then(([typesData, vetsData]) => {
        if (cancelled) return
        setTypes(
          typesData
            .filter(ct => ct.isActive)
            .sort((a, b) => a.sortOrder - b.sortOrder)
        )
        setVets(vetsData)
      })
      .catch(() => {
        if (!cancelled) setError('Failed to load consultation types')
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => { cancelled = true }
  }, [clinicSlug])

  const selectedType = types.find(ct => ct.id === selectedTypeId)
  const isEmergency = selectedType?.name?.toLowerCase().includes('emergency') ?? false

  if (loading) {
    return (
      <div data-testid="consultation-type-step" className="space-y-3">
        <p className="text-sm text-muted-foreground animate-pulse">{t('loading')}</p>
        {[1, 2, 3].map(i => (
          <div key={i} className="h-16 rounded-lg bg-muted animate-pulse" />
        ))}
      </div>
    )
  }

  if (error) {
    return (
      <div data-testid="consultation-type-step" className="text-destructive text-sm">
        {error}
      </div>
    )
  }

  return (
    <div data-testid="consultation-type-step" className="space-y-5">
      <div data-testid="consultation-type-list" className="space-y-2">
        {types.map(type => {
          const isSelected = type.id === selectedTypeId
          return (
            <button
              key={type.id}
              data-testid={`consultation-type-card-${type.id}`}
              type="button"
              onClick={() => onTypeSelect(type.id)}
              className={[
                'w-full flex items-center gap-3 rounded-lg border p-4 text-left transition-all',
                'hover:border-primary hover:bg-primary/5 focus:outline-none focus:ring-2 focus:ring-primary',
                isSelected
                  ? 'border-primary bg-primary/10'
                  : 'border-border bg-card',
              ].join(' ')}
            >
              <div
                className="w-3 h-3 rounded-full flex-shrink-0"
                style={{ backgroundColor: type.colorHex }}
              />
              <div className="flex-1 min-w-0">
                <p className="font-medium text-sm">{type.name}</p>
                {type.description && (
                  <p className="text-xs text-muted-foreground mt-0.5 line-clamp-1">
                    {type.description}
                  </p>
                )}
              </div>
              <div className="flex flex-col items-end gap-1 flex-shrink-0">
                <span className="text-xs text-muted-foreground flex items-center gap-1">
                  <Clock className="h-3 w-3" />
                  {t('duration', { minutes: type.durationMinutes })}
                </span>
                <span className="text-xs font-medium text-foreground">
                  {t('price', { price: type.price })}
                </span>
              </div>
            </button>
          )
        })}
      </div>

      {isEmergency && (
        <div
          data-testid="emergency-warning"
          className="flex items-start gap-2 rounded-lg border border-destructive/50 bg-destructive/10 p-3"
        >
          <AlertTriangle className="h-4 w-4 text-destructive mt-0.5 flex-shrink-0" />
          <p className="text-sm text-destructive">{t('emergency_warning')}</p>
        </div>
      )}

      {selectedTypeId && (
        <div className="space-y-2">
          <label
            htmlFor="vet-preference-select"
            className="text-sm font-medium text-foreground"
          >
            {t('vet_label')}
          </label>
          <select
            id="vet-preference-select"
            data-testid="vet-preference-select"
            value={selectedVetId ?? ''}
            onChange={e => onVetSelect(e.target.value || null)}
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <option value="">{t('vet_no_preference')}</option>
            {vets.map(vet => (
              <option key={vet.id} value={vet.id}>
                {vet.name}
                {vet.speciality ? ` · ${vet.speciality}` : ''}
              </option>
            ))}
          </select>
        </div>
      )}

      {selectedTypeId && (
        <div className="space-y-2">
          <label
            htmlFor="reason-textarea"
            className="text-sm font-medium text-foreground"
          >
            {t('reason_label')}
          </label>
          <textarea
            id="reason-textarea"
            data-testid="reason-textarea"
            value={reason}
            onChange={e => {
              if (e.target.value.length <= MAX_REASON_CHARS) {
                onReasonChange(e.target.value)
              }
            }}
            placeholder={t('reason_placeholder')}
            rows={3}
            maxLength={MAX_REASON_CHARS}
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-primary"
          />
          <p className="text-xs text-muted-foreground text-right">
            {t('reason_max_chars', { count: reason.length })}
          </p>
        </div>
      )}
    </div>
  )
}

'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { User, Calendar, Receipt, Stethoscope, Pill, AlertTriangle, Syringe } from 'lucide-react'
import { Skeleton } from '@/components/ui/skeleton'
import { Badge } from '@/components/ui/badge'
import { apiGet } from '@/lib/api/client'
import type { PatientContextDto } from '@/lib/api/messaging-types'

interface PatientContextPanelProps {
  patientId: string | null
  patientName: string | null
  role: string
  compact?: boolean
}

function formatDate(iso: string): string {
  return new Intl.DateTimeFormat('en-AE', {
    timeZone: 'Asia/Dubai',
    dateStyle: 'medium',
  }).format(new Date(iso))
}

export function PatientContextPanel({
  patientId,
  patientName,
  role,
  compact = false,
}: PatientContextPanelProps) {
  const t = useTranslations('messaging')
  const [context, setContext] = useState<PatientContextDto | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const isVetOrAdmin = role === 'VET' || role === 'ADMIN'

  useEffect(() => {
    if (!patientId) return
    setIsLoading(true)
    apiGet<PatientContextDto>(`/api/v1/messaging/patients/${patientId}/context`)
      .then(setContext)
      .catch(() => {
        // Silently ignore — context panel is supplemental
        setContext(null)
      })
      .finally(() => setIsLoading(false))
  }, [patientId])

  if (!patientId) {
    return (
      <div
        className="p-4 border-b"
        data-testid="patient-context-panel"
        data-patient-linked="false"
      >
        <div className="flex items-center gap-2 mb-2">
          <User className="h-4 w-4 text-muted-foreground" aria-hidden />
          <h3 className="text-sm font-semibold text-muted-foreground">
            {t('patient_context_title')}
          </h3>
        </div>
        <p className="text-sm text-muted-foreground italic" data-testid="no-patient-linked">
          {t('no_patient_linked')}
        </p>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="p-4 border-b space-y-2" data-testid="patient-context-panel">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-4 w-3/4" />
        <Skeleton className="h-4 w-1/2" />
      </div>
    )
  }

  return (
    <div
      className="p-4 border-b"
      data-testid="patient-context-panel"
      data-patient-linked="true"
    >
      <div className="flex items-center gap-2 mb-3">
        <User className="h-4 w-4 text-blue-500" aria-hidden />
        <h3 className="text-sm font-semibold">{t('patient_context_title')}</h3>
      </div>

      {context ? (
        <div className="space-y-2 text-sm" data-testid="patient-context-details">
          {/* Pet name + species */}
          <div className="flex items-center justify-between">
            <span className="font-medium" data-testid="patient-name">{context.patientName}</span>
            <Badge variant="outline" className="text-xs" data-testid="patient-species">
              {context.species}
            </Badge>
          </div>

          {context.breed && (
            <p className="text-muted-foreground text-xs" data-testid="patient-breed">
              {context.breed} · {context.ageYears}y
            </p>
          )}

          {/* Last appointment */}
          {context.lastExaminationDate && (
            <div className="flex items-center gap-1.5 text-muted-foreground" data-testid="patient-last-exam">
              <Calendar className="h-3.5 w-3.5 flex-shrink-0" aria-hidden />
              <span className="text-xs">{t('last_exam')}: {formatDate(context.lastExaminationDate)}</span>
            </div>
          )}

          {/* Unpaid invoices */}
          {context.outstandingInvoicesAed > 0 && (
            <div className="flex items-center gap-1.5 text-amber-600" data-testid="patient-outstanding-invoices">
              <Receipt className="h-3.5 w-3.5 flex-shrink-0" aria-hidden />
              <span className="text-xs font-medium">
                {t('outstanding_invoices')}: {context.outstandingInvoicesAed.toLocaleString('en-AE')} AED
              </span>
            </div>
          )}

          {/* VET / ADMIN only */}
          {isVetOrAdmin && !compact && (
            <>
              {context.currentPrescriptions.length > 0 && (
                <div data-testid="patient-prescriptions">
                  <div className="flex items-center gap-1.5 text-muted-foreground mb-1">
                    <Pill className="h-3.5 w-3.5 flex-shrink-0" aria-hidden />
                    <span className="text-xs font-medium">{t('current_prescriptions')}</span>
                  </div>
                  <ul className="ml-5 space-y-0.5">
                    {context.currentPrescriptions.map((rx) => (
                      <li key={rx} className="text-xs text-muted-foreground list-disc">{rx}</li>
                    ))}
                  </ul>
                </div>
              )}

              {context.knownAllergies.length > 0 && (
                <div data-testid="patient-allergies">
                  <div className="flex items-center gap-1.5 text-red-600 mb-1">
                    <AlertTriangle className="h-3.5 w-3.5 flex-shrink-0" aria-hidden />
                    <span className="text-xs font-medium">{t('known_allergies')}</span>
                  </div>
                  <ul className="ml-5 space-y-0.5">
                    {context.knownAllergies.map((allergy) => (
                      <li key={allergy} className="text-xs text-red-700 list-disc">{allergy}</li>
                    ))}
                  </ul>
                </div>
              )}

              {context.vaccinationHistory.length > 0 && (
                <div data-testid="patient-vaccinations">
                  <div className="flex items-center gap-1.5 text-muted-foreground mb-1">
                    <Syringe className="h-3.5 w-3.5 flex-shrink-0" aria-hidden />
                    <span className="text-xs font-medium">{t('vaccinations')}</span>
                  </div>
                  <ul className="ml-5 space-y-0.5">
                    {context.vaccinationHistory.map((vax) => (
                      <li key={vax} className="text-xs text-muted-foreground list-disc">{vax}</li>
                    ))}
                  </ul>
                </div>
              )}

              <div data-testid="patient-medical-details">
                <div className="flex items-center gap-1.5 text-muted-foreground mb-1">
                  <Stethoscope className="h-3.5 w-3.5 flex-shrink-0" aria-hidden />
                  <span className="text-xs font-medium">{t('last_examination')}</span>
                </div>
                {context.lastExaminationDate ? (
                  <p className="text-xs text-muted-foreground ml-5">
                    {formatDate(context.lastExaminationDate)}
                  </p>
                ) : (
                  <p className="text-xs text-muted-foreground ml-5 italic">{t('no_records')}</p>
                )}
              </div>
            </>
          )}
        </div>
      ) : (
        /* Fallback: show patient name from conversation if context fetch failed */
        <div data-testid="patient-context-fallback">
          <p className="text-sm font-medium" data-testid="patient-name">{patientName}</p>
          <p className="text-xs text-muted-foreground mt-1 italic">{t('context_unavailable')}</p>
        </div>
      )}
    </div>
  )
}

'use client'

import { useEffect, useState } from 'react'
import { useTranslations, useLocale } from 'next-intl'
import { useParams } from 'next/navigation'
import { PawPrint, Syringe, Stethoscope, AlertTriangle } from 'lucide-react'
import { getSharedRecord } from '@/lib/api/record-sharing'
import type { SharedRecordDto } from '@/lib/api/record-sharing'

export function SharedRecordView() {
  const t = useTranslations('portal.shared_record')
  const locale = useLocale()
  const params = useParams<{ locale: string; token: string }>()
  const [record, setRecord] = useState<SharedRecordDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getSharedRecord(params.token)
      .then(setRecord)
      .catch((err: Error) => {
        setError(err.message || t('error_generic'))
      })
      .finally(() => setLoading(false))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params.token])

  if (loading) {
    return (
      <div className="min-h-screen bg-muted flex items-center justify-center" data-testid="shared-record-loading">
        <div className="space-y-4 w-full max-w-lg px-4">
          <div className="h-8 w-48 rounded bg-muted-foreground/10 animate-pulse" />
          <div className="h-40 rounded-xl bg-white animate-pulse" />
          <div className="h-32 rounded-xl bg-white animate-pulse" />
        </div>
      </div>
    )
  }

  if (error || !record) {
    return (
      <div className="min-h-screen bg-muted flex items-center justify-center" data-testid="shared-record-error">
        <div className="text-center space-y-3 px-4">
          <AlertTriangle className="w-12 h-12 text-muted-foreground mx-auto" />
          <p className="text-foreground font-medium" data-testid="shared-record-error-message">
            {error || t('not_found')}
          </p>
          <p className="text-sm text-muted-foreground">{t('error_hint')}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-muted" data-testid="shared-record-page">
      {/* Header */}
      <header className="bg-white border-b border-border/80 px-4 py-3 shadow-sm">
        <div className="max-w-lg mx-auto flex items-center gap-2">
          <div className="w-8 h-8 rounded-full bg-primary flex items-center justify-center text-white font-bold text-sm">
            {record.clinicName.charAt(0)}
          </div>
          <span className="font-semibold text-foreground text-sm" data-testid="shared-clinic-name">
            {record.clinicName}
          </span>
        </div>
      </header>

      <main className="max-w-lg mx-auto px-4 py-6 space-y-5">
        {/* Animal info */}
        <div className="flex items-center gap-3" data-testid="shared-animal-info">
          <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
            <PawPrint className="h-6 w-6 text-primary" />
          </div>
          <div>
            <h1 className="text-xl font-bold text-foreground" data-testid="shared-animal-name">
              {record.animalName}
            </h1>
            <p className="text-sm text-muted-foreground">
              {record.species} &middot; {record.breed} &middot; {t('age', { years: record.ageYears })}
            </p>
          </div>
        </div>

        {/* Vaccinations */}
        <div className="bg-white rounded-xl border border-border/80 p-4 space-y-3" data-testid="shared-vaccinations">
          <h2 className="text-sm font-semibold text-foreground flex items-center gap-2">
            <Syringe className="h-4 w-4 text-primary" />
            {t('vaccinations')}
          </h2>
          {record.vaccinations.length === 0 ? (
            <p className="text-xs text-muted-foreground">{t('no_vaccinations')}</p>
          ) : (
            <div className="space-y-2">
              {record.vaccinations.map((vax, i) => (
                <div
                  key={i}
                  className="flex items-center justify-between text-sm border-b border-border/50 last:border-0 pb-2 last:pb-0"
                  data-testid={`shared-vax-${i}`}
                >
                  <div>
                    <p className="font-medium text-foreground">{vax.name}</p>
                    <p className="text-xs text-muted-foreground">
                      {t('given', {
                        date: new Date(vax.date).toLocaleDateString(locale, {
                          day: 'numeric',
                          month: 'short',
                          year: 'numeric',
                          timeZone: 'Asia/Dubai',
                        }),
                      })}
                    </p>
                  </div>
                  {vax.nextDue && (
                    <span className="text-xs text-muted-foreground" data-testid={`shared-vax-due-${i}`}>
                      {t('next_due', {
                        date: new Date(vax.nextDue).toLocaleDateString(locale, {
                          day: 'numeric',
                          month: 'short',
                          year: 'numeric',
                          timeZone: 'Asia/Dubai',
                        }),
                      })}
                    </span>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Recent consultations */}
        <div className="bg-white rounded-xl border border-border/80 p-4 space-y-3" data-testid="shared-consultations">
          <h2 className="text-sm font-semibold text-foreground flex items-center gap-2">
            <Stethoscope className="h-4 w-4 text-primary" />
            {t('consultations')}
          </h2>
          {record.recentConsultations.length === 0 ? (
            <p className="text-xs text-muted-foreground">{t('no_consultations')}</p>
          ) : (
            <div className="space-y-3">
              {record.recentConsultations.map((consult, i) => (
                <div
                  key={i}
                  className="border-b border-border/50 last:border-0 pb-3 last:pb-0"
                  data-testid={`shared-consult-${i}`}
                >
                  <div className="flex items-center justify-between mb-1">
                    <p className="font-medium text-sm text-foreground">{consult.reason}</p>
                    <span className="text-xs text-muted-foreground">
                      {new Date(consult.date).toLocaleDateString(locale, {
                        day: 'numeric',
                        month: 'short',
                        year: 'numeric',
                        timeZone: 'Asia/Dubai',
                      })}
                    </span>
                  </div>
                  <p className="text-xs text-muted-foreground mb-1">{consult.veterinarian}</p>
                  <p className="text-xs text-foreground/80">{consult.notes}</p>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer notice */}
        <p className="text-xs text-muted-foreground text-center" data-testid="shared-record-notice">
          {t('notice')}
        </p>
      </main>
    </div>
  )
}

'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { AlertTriangle } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { recordConsent } from '@/lib/api/portal'

const CONSENT_VERSION = '1.0'

export function ConsentScreen() {
  const t = useTranslations('portal.consent')
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const [accepted, setAccepted] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleContinue() {
    if (!accepted) {
      setError(t('must_accept'))
      return
    }
    setError(null)
    setIsSubmitting(true)
    try {
      await recordConsent({ consentVersion: CONSENT_VERSION })
      // Only mark consent in session storage AFTER API confirms it was recorded
      sessionStorage.setItem('portal_consent_given', 'true')
      router.push(`/${params.locale}/portal/${params.clinicSlug}/new`)
    } catch {
      // API failed — do NOT set sessionStorage, consent is not recorded
      sessionStorage.removeItem('portal_consent_given')
      setError(t('consent_failed'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="space-y-6" data-testid="consent-screen">
      <div>
        <h1 className="text-[22px] font-bold text-foreground" data-testid="consent-title">
          {t('title')}
        </h1>
        <p className="mt-1 text-[13px] text-muted-foreground">{t('intro')}</p>
      </div>

      {/* Emergency warning */}
      <div
        className="flex items-start gap-3 rounded-xl border border-amber-300 bg-amber-50 p-4"
        data-testid="consent-emergency-warning"
        role="alert"
      >
        <AlertTriangle className="h-5 w-5 text-amber-600 mt-0.5 flex-shrink-0" />
        <div className="text-sm text-amber-800">
          <p className="font-semibold">{t('emergency_title') ?? 'Emergency?'}</p>
          <p className="mt-0.5">
            {t('emergency_body') ?? 'If your pet is experiencing a medical emergency, please call the clinic directly instead of using this messaging service.'}
          </p>
        </div>
      </div>

      <div className="bg-white rounded-xl border border-border/80 p-4 text-[13px] text-foreground whitespace-pre-line leading-relaxed shadow-sm">
        {t('terms_body')}
      </div>

      <div className="flex items-start gap-3">
        <input
          id="consent-checkbox"
          type="checkbox"
          checked={accepted}
          onChange={(e) => {
            setAccepted(e.target.checked)
            if (error) setError(null)
          }}
          data-testid="consent-checkbox"
          className="mt-0.5 h-4 w-4 rounded border-border/80 text-primary focus:ring-primary cursor-pointer"
        />
        <label
          htmlFor="consent-checkbox"
          className="text-[13px] text-foreground cursor-pointer select-none"
          data-testid="consent-label"
        >
          {t('accept_label')}
        </label>
      </div>

      {error && (
        <p className="text-sm text-red-600" data-testid="consent-error">
          {error}
        </p>
      )}

      <Button
        onClick={handleContinue}
        disabled={!accepted || isSubmitting}
        data-testid="accept-consent-btn"
        className="w-full bg-primary hover:bg-primary/90 text-white font-semibold rounded-xl shadow-sm"
      >
        {isSubmitting ? '...' : t('continue')}
      </Button>
    </div>
  )
}

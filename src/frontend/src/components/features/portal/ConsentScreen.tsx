'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
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
      // Mark consent given in session storage
      sessionStorage.setItem('portal_consent_given', 'true')
      router.push(`/${params.locale}/portal/${params.clinicSlug}/new`)
    } catch {
      setError(t('must_accept'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="space-y-6" data-testid="consent-screen">
      <div>
        <h1 className="text-xl font-bold text-gray-900" data-testid="consent-title">
          {t('title')}
        </h1>
        <p className="mt-1 text-sm text-gray-600">{t('intro')}</p>
      </div>

      <div className="bg-white rounded-lg border border-gray-200 p-4 text-sm text-gray-700 whitespace-pre-line leading-relaxed">
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
          className="mt-0.5 h-4 w-4 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500 cursor-pointer"
        />
        <label
          htmlFor="consent-checkbox"
          className="text-sm text-gray-700 cursor-pointer select-none"
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
        disabled={isSubmitting}
        data-testid="accept-consent-btn"
        className="w-full bg-emerald-600 hover:bg-emerald-700 text-white"
      >
        {isSubmitting ? '...' : t('continue')}
      </Button>
    </div>
  )
}

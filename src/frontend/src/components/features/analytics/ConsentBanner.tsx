'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { Button } from '@/components/ui/button'
import { posthog, isPostHogAvailable } from '@/lib/posthog'

function hasNoStoredConsent() {
  if (typeof window === 'undefined') return false
  return localStorage.getItem('analytics_consent') === null
}

export function ConsentBanner() {
  const t = useTranslations('analytics.consent_banner')
  const [visible, setVisible] = useState(hasNoStoredConsent)

  function handleAccept() {
    localStorage.setItem('analytics_consent', 'granted')
    if (isPostHogAvailable()) {
      posthog.opt_in_capturing()
    }
    setVisible(false)
  }

  function handleDecline() {
    localStorage.setItem('analytics_consent', 'denied')
    if (isPostHogAvailable()) {
      posthog.opt_out_capturing()
    }
    setVisible(false)
  }

  if (!visible) return null

  return (
    <>
      {/* Spacer to prevent content overlap when banner is visible */}
      <div className="h-20 sm:h-16" aria-hidden="true" />
      <div
        data-testid="consent-banner"
        className="fixed bottom-0 left-0 right-0 z-[9999] border-t bg-background shadow-lg animate-in slide-in-from-bottom-full fade-in-0 duration-300 ease-out"
      >
        <div className="container mx-auto flex flex-col gap-3 px-4 py-4 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-sm text-muted-foreground">{t('message')}</p>
          <div className="flex shrink-0 gap-2">
            <Button
              variant="outline"
              size="sm"
              data-testid="consent-decline"
              onClick={handleDecline}
              className="transition-all duration-200 ease-in-out hover:scale-[1.02] active:scale-[0.98]"
            >
              {t('decline')}
            </Button>
            <Button
              size="sm"
              data-testid="consent-accept"
              onClick={handleAccept}
              className="transition-all duration-200 ease-in-out hover:scale-[1.02] active:scale-[0.98]"
            >
              {t('accept')}
            </Button>
          </div>
        </div>
      </div>
    </>
  )
}

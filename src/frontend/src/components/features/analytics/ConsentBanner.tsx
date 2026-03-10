'use client'

import { useState, useEffect } from 'react'
import { useTranslations } from 'next-intl'
import { Button } from '@/components/ui/button'
import { posthog, isPostHogAvailable } from '@/lib/posthog'

export function ConsentBanner() {
  const t = useTranslations('analytics.consent_banner')
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    const stored = localStorage.getItem('analytics_consent')
    if (stored === null) {
      setVisible(true)
    }
  }, [])

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
    <div
      data-testid="consent-banner"
      className="fixed bottom-0 left-0 right-0 z-50 border-t bg-background shadow-lg"
    >
      <div className="container mx-auto flex flex-col gap-3 px-4 py-4 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-sm text-muted-foreground">{t('message')}</p>
        <div className="flex shrink-0 gap-2">
          <Button
            variant="outline"
            size="sm"
            data-testid="consent-decline"
            onClick={handleDecline}
          >
            {t('decline')}
          </Button>
          <Button
            size="sm"
            data-testid="consent-accept"
            onClick={handleAccept}
          >
            {t('accept')}
          </Button>
        </div>
      </div>
    </div>
  )
}

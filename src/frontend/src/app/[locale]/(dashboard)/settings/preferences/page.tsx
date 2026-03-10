'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { Switch } from '@/components/ui/switch'
import { Label } from '@/components/ui/label'
import { posthog, isPostHogAvailable } from '@/lib/posthog'

export default function PreferencesPage() {
  const t = useTranslations('analytics.settings')
  const [analyticsEnabled, setAnalyticsEnabled] = useState(false)

  useEffect(() => {
    const stored = localStorage.getItem('analytics_consent')
    setAnalyticsEnabled(stored === 'granted')
  }, [])

  function handleToggle(enabled: boolean) {
    if (enabled) {
      localStorage.setItem('analytics_consent', 'granted')
      if (isPostHogAvailable()) {
        posthog.opt_in_capturing()
      }
    } else {
      localStorage.setItem('analytics_consent', 'denied')
      if (isPostHogAvailable()) {
        posthog.opt_out_capturing()
      }
    }
    setAnalyticsEnabled(enabled)
  }

  return (
    <div className="space-y-6" data-testid="preferences-page">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('title')}</h1>
        <p className="text-muted-foreground text-sm mt-1">{t('description')}</p>
      </div>

      <div className="rounded-lg border p-4">
        <div className="flex items-center justify-between gap-4">
          <Label
            htmlFor="analytics-consent-toggle"
            className="flex flex-col gap-1 cursor-pointer"
          >
            <span className="font-medium">{t('toggle_label')}</span>
          </Label>
          <Switch
            id="analytics-consent-toggle"
            data-testid="analytics-consent-toggle"
            checked={analyticsEnabled}
            onCheckedChange={handleToggle}
          />
        </div>
      </div>
    </div>
  )
}

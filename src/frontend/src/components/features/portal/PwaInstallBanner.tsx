'use client'

import { useEffect, useState, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { Download, X } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface BeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>
}

const DISMISSED_KEY = 'vetara-pwa-install-dismissed'

export function PwaInstallBanner() {
  const t = useTranslations('portal.pwa')
  const [deferredPrompt, setDeferredPrompt] = useState<BeforeInstallPromptEvent | null>(null)
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    // Don't show if already installed or previously dismissed this session
    if (window.matchMedia('(display-mode: standalone)').matches) return
    if (sessionStorage.getItem(DISMISSED_KEY)) return

    function onBeforeInstall(e: Event) {
      e.preventDefault()
      setDeferredPrompt(e as BeforeInstallPromptEvent)
      setVisible(true)
    }

    window.addEventListener('beforeinstallprompt', onBeforeInstall)
    return () => window.removeEventListener('beforeinstallprompt', onBeforeInstall)
  }, [])

  const handleInstall = useCallback(async () => {
    if (!deferredPrompt) return
    await deferredPrompt.prompt()
    const { outcome } = await deferredPrompt.userChoice
    if (outcome === 'accepted') {
      setVisible(false)
    }
    setDeferredPrompt(null)
  }, [deferredPrompt])

  const handleDismiss = useCallback(() => {
    setVisible(false)
    setDeferredPrompt(null)
    sessionStorage.setItem(DISMISSED_KEY, '1')
  }, [])

  if (!visible) return null

  return (
    <div
      className="fixed bottom-16 left-4 right-4 md:bottom-4 md:left-auto md:right-4 md:max-w-sm z-50 bg-white border border-border shadow-lg rounded-xl p-4 flex items-start gap-3"
      role="banner"
      data-testid="pwa-install-banner"
    >
      <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
        <Download className="h-5 w-5 text-primary" />
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-foreground" data-testid="pwa-install-title">
          {t('install_title')}
        </p>
        <p className="text-xs text-muted-foreground mt-0.5">
          {t('install_description')}
        </p>
        <Button
          size="sm"
          className="mt-2"
          onClick={handleInstall}
          data-testid="pwa-install-button"
        >
          {t('install_button')}
        </Button>
      </div>
      <button
        onClick={handleDismiss}
        className="flex-shrink-0 text-muted-foreground hover:text-foreground p-1"
        aria-label={t('dismiss')}
        data-testid="pwa-install-dismiss"
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  )
}

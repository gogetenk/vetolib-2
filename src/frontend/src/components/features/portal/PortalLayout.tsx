'use client'

import { useTranslations } from 'next-intl'
import { usePathname, useRouter } from 'next/navigation'
import { Globe } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface PortalLayoutProps {
  children: React.ReactNode
  clinicName?: string
}

export function PortalLayout({ children, clinicName = 'Desert Paws Clinic' }: PortalLayoutProps) {
  const t = useTranslations('portal.header')
  const pathname = usePathname()
  const router = useRouter()

  function toggleLanguage() {
    // Switch between /en/portal/... and /ar/portal/...
    if (pathname.startsWith('/ar/')) {
      router.push(pathname.replace('/ar/', '/en/'))
    } else {
      router.push(pathname.replace(/^\/[a-z]{2}\//, '/ar/'))
    }
  }

  const isAr = pathname.startsWith('/ar/')

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Portal header */}
      <header
        className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between sticky top-0 z-10 shadow-sm"
        data-testid="portal-header"
      >
        <div className="flex items-center gap-2">
          <div
            className="w-8 h-8 rounded-full bg-emerald-600 flex items-center justify-center text-white font-bold text-sm flex-shrink-0"
            aria-hidden="true"
          >
            {clinicName.charAt(0)}
          </div>
          <span className="font-semibold text-gray-900 text-sm sm:text-base" data-testid="portal-clinic-name">
            {clinicName}
          </span>
        </div>

        <Button
          variant="ghost"
          size="sm"
          onClick={toggleLanguage}
          data-testid="portal-language-toggle"
          className="flex items-center gap-1 text-gray-600"
        >
          <Globe className="h-4 w-4" />
          <span className="text-xs">{isAr ? 'English' : 'عربي'}</span>
          <span className="sr-only">{t('language')}</span>
        </Button>
      </header>

      {/* Main content */}
      <main className="flex-1 px-4 py-6 max-w-2xl mx-auto w-full">
        {children}
      </main>
    </div>
  )
}

'use client'

import { useTranslations } from 'next-intl'
import { usePathname, useRouter, useParams } from 'next/navigation'
import { Globe, MessageCircle, CalendarDays, PawPrint, User } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface PortalLayoutProps {
  children: React.ReactNode
  clinicName?: string
}

interface NavItem {
  labelKey: string
  icon: React.ReactNode
  path: string
}

export function PortalLayout({ children, clinicName = 'Desert Paws Clinic' }: PortalLayoutProps) {
  const t = useTranslations('portal.header')
  const tNav = useTranslations('portal.nav')
  const pathname = usePathname()
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()

  const basePath = `/${params.locale}/portal/${params.clinicSlug}`

  const navItems: NavItem[] = [
    { labelKey: 'conversations', icon: <MessageCircle className="h-5 w-5" />, path: basePath },
    { labelKey: 'appointments', icon: <CalendarDays className="h-5 w-5" />, path: `${basePath}/book` },
    { labelKey: 'my_pets', icon: <PawPrint className="h-5 w-5" />, path: `${basePath}/pets` },
    { labelKey: 'profile', icon: <User className="h-5 w-5" />, path: `${basePath}/profile` },
  ]

  function toggleLanguage() {
    if (pathname.startsWith('/ar/')) {
      router.push(pathname.replace('/ar/', '/en/'))
    } else {
      router.push(pathname.replace(/^\/[a-z]{2}\//, '/ar/'))
    }
  }

  const isAr = pathname.startsWith('/ar/')

  function isActive(itemPath: string) {
    if (itemPath === basePath) {
      return pathname === basePath || pathname.startsWith(`${basePath}/conversations`)
    }
    return pathname.startsWith(itemPath)
  }

  return (
    <div className="min-h-screen bg-[#f4f6f9] flex flex-col">
      {/* Portal header */}
      <header
        className="bg-white border-b border-border/80 px-4 py-3 flex items-center justify-between sticky top-0 z-10 shadow-sm"
        data-testid="portal-header"
      >
        <div className="flex items-center gap-2">
          <div
            className="w-8 h-8 rounded-full bg-[#303ef5] flex items-center justify-center text-white font-bold text-sm flex-shrink-0"
            aria-hidden="true"
          >
            {clinicName.charAt(0)}
          </div>
          <span className="font-semibold text-[#061e44] text-sm sm:text-base" data-testid="portal-clinic-name">
            {clinicName}
          </span>
        </div>

        <Button
          variant="ghost"
          size="sm"
          onClick={toggleLanguage}
          data-testid="portal-language-toggle"
          className="flex items-center gap-1 text-muted-foreground hover:text-[#061e44]"
        >
          <Globe className="h-4 w-4" />
          <span className="text-xs">{isAr ? 'English' : '\u0639\u0631\u0628\u064a'}</span>
          <span className="sr-only">{t('language')}</span>
        </Button>
      </header>

      <div className="flex flex-1">
        {/* Desktop sidebar navigation */}
        <aside
          className="hidden md:flex flex-col w-56 bg-white border-e border-border/80 py-4 px-2"
          data-testid="portal-sidebar"
        >
          <nav className="space-y-1">
            {navItems.map((item) => (
              <button
                key={item.labelKey}
                onClick={() => router.push(item.path)}
                data-testid={`portal-nav-${item.labelKey}`}
                className={cn(
                  'flex items-center gap-3 w-full rounded-xl px-3 py-2.5 text-[13px] font-semibold transition-all duration-200 min-h-[44px]',
                  isActive(item.path)
                    ? 'bg-[#eef2fd] text-[#303ef5]'
                    : 'text-muted-foreground hover:bg-[#f4f6f9] hover:text-[#061e44]'
                )}
              >
                {item.icon}
                {tNav(item.labelKey)}
              </button>
            ))}
          </nav>
        </aside>

        {/* Main content */}
        <main className="flex-1 px-4 py-6 max-w-2xl mx-auto w-full pb-20 md:pb-6">
          {children}
        </main>
      </div>

      {/* Mobile bottom tab bar */}
      <nav
        className="md:hidden fixed bottom-0 left-0 right-0 bg-white border-t border-border/80 z-10 flex justify-around py-1"
        data-testid="portal-bottom-tabs"
      >
        {navItems.map((item) => (
          <button
            key={item.labelKey}
            onClick={() => router.push(item.path)}
            data-testid={`portal-tab-${item.labelKey}`}
            className={cn(
              'flex flex-col items-center gap-0.5 py-2 px-3 min-w-[64px] min-h-[44px] text-xs font-medium transition-colors',
              isActive(item.path)
                ? 'text-[#303ef5]'
                : 'text-muted-foreground'
            )}
          >
            {item.icon}
            <span className="truncate">{tNav(item.labelKey)}</span>
          </button>
        ))}
      </nav>
    </div>
  )
}

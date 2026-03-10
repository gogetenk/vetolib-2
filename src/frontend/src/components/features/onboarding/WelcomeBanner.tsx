'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useTranslations } from 'next-intl'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useOnboarding } from '@/hooks/use-onboarding'
import type { UserRole } from '@/hooks/use-role'

// Veterinary clinic illustration — simple SVG placeholder
function VetIllustration() {
  return (
    <svg
      aria-hidden="true"
      width="80"
      height="80"
      viewBox="0 0 80 80"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className="shrink-0 opacity-80"
    >
      {/* Paw print outline */}
      <circle cx="40" cy="45" r="18" fill="#dbeafe" stroke="#3b82f6" strokeWidth="2" />
      <circle cx="28" cy="28" r="7" fill="#dbeafe" stroke="#3b82f6" strokeWidth="2" />
      <circle cx="40" cy="23" r="7" fill="#dbeafe" stroke="#3b82f6" strokeWidth="2" />
      <circle cx="52" cy="28" r="7" fill="#dbeafe" stroke="#3b82f6" strokeWidth="2" />
      {/* Cross / medical symbol */}
      <rect x="36" y="38" width="8" height="14" rx="2" fill="#3b82f6" />
      <rect x="33" y="41" width="14" height="8" rx="2" fill="#3b82f6" />
    </svg>
  )
}

interface WelcomeBannerProps {
  role: UserRole
  clinicName?: string
}

type RoleKey = 'admin' | 'vet' | 'receptionist' | 'assistant'

function getRoleKey(role: UserRole): RoleKey {
  switch (role.toUpperCase()) {
    case 'ADMIN': return 'admin'
    case 'VET': return 'vet'
    case 'RECEPTIONIST': return 'receptionist'
    case 'ASSISTANT': return 'assistant'
    default: return 'vet'
  }
}

export function WelcomeBanner({ role, clinicName }: WelcomeBannerProps) {
  const t = useTranslations('onboarding.banner')
  const router = useRouter()
  const { state, dismissBanner } = useOnboarding()
  const [dismissed, setDismissed] = useState(false)
  const [animatingOut, setAnimatingOut] = useState(false)

  // Derive visibility from server state + local dismissal
  const visible = !dismissed && state !== null && state.welcomeBannerVisible

  if (!visible && !animatingOut) return null

  const roleKey = getRoleKey(role)

  function handleCta() {
    if (role.toUpperCase() === 'ADMIN' || role.toUpperCase() === 'VET') {
      // Smooth scroll to setup checklist
      const el = document.getElementById('setup-checklist')
      if (el) {
        el.scrollIntoView({ behavior: 'smooth' })
      }
    } else if (role.toUpperCase() === 'RECEPTIONIST') {
      router.push('/appointments/new')
    } else {
      // ASSISTANT
      router.push('/patients')
    }
  }

  function handleDismiss() {
    setAnimatingOut(true)
    // Allow 200ms fade-out before hiding
    setTimeout(async () => {
      setDismissed(true)
      setAnimatingOut(false)
      try {
        await dismissBanner()
      } catch {
        // Fail silently — banner is already hidden locally
      }
    }, 200)
  }

  return (
    <div
      data-testid="welcome-banner"
      role="banner"
      className={[
        'relative flex items-center gap-4 rounded-xl border border-blue-200 bg-gradient-to-r from-blue-50 to-indigo-50 p-5 shadow-sm',
        'transition-opacity duration-200',
        animatingOut ? 'opacity-0' : 'opacity-100',
      ].join(' ')}
    >
      {/* Illustration */}
      <VetIllustration />

      {/* Text content */}
      <div className="flex min-w-0 flex-1 flex-col gap-2">
        <p
          data-testid="welcome-banner-greeting"
          className="text-base font-semibold text-gray-900"
        >
          {t(`${roleKey}.greeting`, { clinicName: clinicName ?? '' })}
        </p>

        <Button
          data-testid="welcome-banner-cta"
          size="sm"
          variant="default"
          className="w-fit"
          onClick={handleCta}
        >
          {t(`${roleKey}.cta`)}
        </Button>
      </div>

      {/* Dismiss button — top-right in LTR, top-left in RTL */}
      <button
        data-testid="welcome-banner-dismiss"
        aria-label="Dismiss welcome banner"
        onClick={handleDismiss}
        className="absolute end-3 top-3 rounded-full p-1 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  )
}

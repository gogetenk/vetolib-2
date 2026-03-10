'use client'

import { useState } from 'react'
import { StatsCards } from '@/components/features/dashboard/StatsCards'
import { TodayAppointments } from '@/components/features/dashboard/TodayAppointments'
import { RecentActivity } from '@/components/features/dashboard/RecentActivity'
import { AnalyticsSection } from '@/components/features/dashboard/AnalyticsSection'
import { WelcomeBanner } from '@/components/features/onboarding/WelcomeBanner'
import { useRole } from '@/hooks/use-role'
import { getStoredUser } from '@/lib/api/auth'

function getGreeting(locale: string): string {
  const hour = new Date().toLocaleString('en-AE', {
    hour: 'numeric',
    hour12: false,
    timeZone: 'Asia/Dubai',
  })
  const h = parseInt(hour)

  if (locale === 'ar') {
    if (h < 12) return 'صباح الخير'
    if (h < 18) return 'مساء الخير'
    return 'مساء النور'
  }

  if (h < 12) return 'Good morning'
  if (h < 18) return 'Good afternoon'
  return 'Good evening'
}

function formatDateStr(locale: string): string {
  return new Date().toLocaleDateString(locale === 'ar' ? 'ar-AE' : 'en-AE', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
    timeZone: 'Asia/Dubai',
  })
}

function getInitialUserName(): string {
  if (typeof window === 'undefined') return ''
  const user = getStoredUser()
  return user?.name || ''
}

function getInitialLocale(): string {
  if (typeof window === 'undefined') return 'en'
  return document.documentElement.lang || 'en'
}

export default function DashboardHomePage() {
  const role = useRole()
  const [userName] = useState<string>(getInitialUserName)
  const [locale] = useState<string>(getInitialLocale)

  // Extract clinicName from stored user for the welcome banner
  function getClinicName(): string {
    if (typeof window === 'undefined') return ''
    try {
      const token = localStorage.getItem('access_token')
      if (!token) return ''
      const payload = JSON.parse(atob(token.split('.')[1]))
      return (payload.clinicName as string) ?? ''
    } catch {
      return ''
    }
  }
  const [clinicName] = useState<string>(getClinicName)

  return (
    <div className="space-y-6" data-testid="dashboard-home">
      <WelcomeBanner role={role} clinicName={clinicName} />

      <div className="flex flex-col gap-1" data-testid="dashboard-greeting">
        <h1 className="text-2xl font-bold" data-testid="dashboard-greeting-title">
          {getGreeting(locale)}{userName ? `, ${userName}` : ''}
        </h1>
        <p className="text-sm text-muted-foreground capitalize" data-testid="dashboard-date">
          {formatDateStr(locale)}
        </p>
      </div>

      <StatsCards role={role as 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'} />

      <div id="setup-checklist" className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <TodayAppointments role={role as 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'} />
        <RecentActivity />
      </div>

      <AnalyticsSection />
    </div>
  )
}

'use client'

import { useState } from 'react'
import { StatsCards } from '@/components/features/dashboard/StatsCards'
import { TodayAppointments } from '@/components/features/dashboard/TodayAppointments'
import { RecentActivity } from '@/components/features/dashboard/RecentActivity'
import { useRole } from '@/hooks/use-role'

function parseUserName(token: string): string {
  try {
    const base64 = token.split('.')[1]
    const json = atob(base64.replace(/-/g, '+').replace(/_/g, '/'))
    const payload = JSON.parse(json) as Record<string, unknown>
    return (payload['name'] as string) || ''
  } catch {
    return ''
  }
}

function getGreeting(): string {
  const hour = new Date().toLocaleString('en-AE', {
    hour: 'numeric',
    hour12: false,
    timeZone: 'Asia/Dubai',
  })
  const h = parseInt(hour)
  if (h < 12) return 'Bonjour'
  if (h < 18) return 'Bon après-midi'
  return 'Bonsoir'
}

function formatDate(): string {
  return new Date().toLocaleDateString('fr-AE', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
    timeZone: 'Asia/Dubai',
  })
}

function getInitialUserName(): string {
  if (typeof window === 'undefined') return ''
  const token = localStorage.getItem('access_token')
  return token ? parseUserName(token) : ''
}

export default function DashboardHomePage() {
  const role = useRole()
  const [userName] = useState<string>(getInitialUserName)

  return (
    <div className="space-y-6" data-testid="dashboard-home">
      {/* Greeting header */}
      <div className="flex flex-col gap-1" data-testid="dashboard-greeting">
        <h1 className="text-2xl font-bold" data-testid="dashboard-greeting-title">
          {getGreeting()}{userName ? `, ${userName}` : ''} 👋
        </h1>
        <p className="text-sm text-muted-foreground capitalize" data-testid="dashboard-date">
          {formatDate()}
        </p>
      </div>

      {/* Stats cards — filtered by RBAC */}
      <StatsCards role={role as 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'} />

      {/* Two-column layout for appointments + activity */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <TodayAppointments role={role as 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'} />
        <RecentActivity />
      </div>
    </div>
  )
}

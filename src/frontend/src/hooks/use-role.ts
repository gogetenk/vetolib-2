'use client'
import { useState } from 'react'

export type UserRole = 'VET' | 'ASSISTANT' | 'RECEPTIONIST' | string

function parseRoleFromToken(token: string): UserRole {
  try {
    const base64 = token.split('.')[1]
    const json = atob(base64.replace(/-/g, '+').replace(/_/g, '/'))
    const payload = JSON.parse(json) as Record<string, unknown>
    return (
      (payload['role'] as string) ||
      (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as string) ||
      'VET'
    )
  } catch {
    return 'VET'
  }
}

function getInitialRole(): UserRole {
  if (typeof window === 'undefined') return 'VET'
  const token = localStorage.getItem('access_token')
  return token ? parseRoleFromToken(token) : 'VET'
}

export function useRole(): UserRole {
  const [role] = useState<UserRole>(getInitialRole)
  return role
}

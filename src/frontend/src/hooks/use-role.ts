'use client'
import { useEffect, useState } from 'react'

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

export function useRole(): UserRole {
  const [role, setRole] = useState<UserRole>('VET')

  useEffect(() => {
    const token = localStorage.getItem('access_token')
    if (token) {
      setRole(parseRoleFromToken(token))
    }
  }, [])

  return role
}

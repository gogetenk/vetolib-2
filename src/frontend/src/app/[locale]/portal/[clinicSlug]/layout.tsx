'use client'

import { useEffect } from 'react'
import { useSearchParams } from 'next/navigation'
import { PortalLayout } from '@/components/features/portal/PortalLayout'
import { setPortalToken } from '@/lib/api/portal'

interface Props {
  children: React.ReactNode
}

export default function PortalSlugLayout({ children }: Props) {
  const searchParams = useSearchParams()

  // Capture magic link token from query string on first load
  useEffect(() => {
    const token = searchParams.get('token')
    if (token) {
      setPortalToken(token)
    }
  }, [searchParams])

  return (
    <PortalLayout>
      {children}
    </PortalLayout>
  )
}

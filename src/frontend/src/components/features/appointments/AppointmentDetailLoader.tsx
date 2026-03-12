'use client'

import { useEffect, useState, useCallback } from 'react'
import { getAppointment } from '@/lib/api/appointments'
import type { AppointmentDto } from '@/lib/api/appointments'
import { ApiError } from '@/lib/api/client'
import { AppointmentDetail } from './AppointmentDetail'
import { Skeleton } from '@/components/ui/skeleton'
import { ErrorState } from '@/components/ui/error-state'
import { useTranslations } from 'next-intl'

type ErrorKind = 'not_found' | 'server_error' | 'network_error'

interface Props {
  id: string
}

export function AppointmentDetailLoader({ id }: Props) {
  const t = useTranslations('appointments.detail')
  const [appointment, setAppointment] = useState<AppointmentDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorKind, setErrorKind] = useState<ErrorKind | null>(null)

  const load = useCallback(() => {
    setIsLoading(true)
    setErrorKind(null)
    getAppointment(id)
      .then(setAppointment)
      .catch((err) => {
        if (err instanceof ApiError) {
          if (err.status === 404) {
            setErrorKind('not_found')
          } else {
            setErrorKind('server_error')
          }
        } else {
          setErrorKind('network_error')
        }
      })
      .finally(() => setIsLoading(false))
  }, [id])

  useEffect(() => {
    load()
  }, [load])

  if (isLoading) {
    return (
      <div className="space-y-3" data-testid="detail-loading">
        <Skeleton className="h-12 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (errorKind === 'not_found') {
    return (
      <p className="text-muted-foreground" data-testid="detail-not-found">
        {t('error_not_found')}
      </p>
    )
  }

  if (errorKind === 'server_error') {
    return (
      <ErrorState
        data-testid="detail-server-error"
        title={t('error_server_title')}
        description={t('error_server_description')}
        onRetry={load}
      />
    )
  }

  if (errorKind === 'network_error') {
    return (
      <ErrorState
        data-testid="detail-network-error"
        title={t('error_network_title')}
        description={t('error_network_description')}
        onRetry={load}
      />
    )
  }

  if (!appointment) {
    return (
      <p className="text-muted-foreground" data-testid="detail-not-found">
        {t('error_not_found')}
      </p>
    )
  }

  return <AppointmentDetail appointment={appointment} />
}

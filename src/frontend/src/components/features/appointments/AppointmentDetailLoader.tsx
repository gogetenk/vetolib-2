'use client'

import { useEffect, useReducer } from 'react'
import { getAppointment } from '@/lib/api/appointments'
import type { AppointmentDto } from '@/lib/api/appointments'
import { ApiError } from '@/lib/api/client'
import { AppointmentDetail } from './AppointmentDetail'
import { Skeleton } from '@/components/ui/skeleton'
import { useTranslations } from 'next-intl'

type ErrorKind = 'not_found' | 'server_error' | 'network_error'

type State = {
  appointment: AppointmentDto | null
  isLoading: boolean
  errorKind: ErrorKind | null
}

type Action =
  | { type: 'FETCH_START' }
  | { type: 'FETCH_SUCCESS'; data: AppointmentDto }
  | { type: 'FETCH_ERROR'; errorKind: ErrorKind }

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'FETCH_START':
      return { appointment: null, isLoading: true, errorKind: null }
    case 'FETCH_SUCCESS':
      return { appointment: action.data, isLoading: false, errorKind: null }
    case 'FETCH_ERROR':
      return { appointment: null, isLoading: false, errorKind: action.errorKind }
  }
}

interface Props {
  id: string
}

export function AppointmentDetailLoader({ id }: Props) {
  const t = useTranslations('appointments.detail')
  const [state, dispatch] = useReducer(reducer, { appointment: null, isLoading: true, errorKind: null })
  const [retryCount, retry] = useReducer((c: number) => c + 1, 0)

  useEffect(() => {
    let cancelled = false
    dispatch({ type: 'FETCH_START' })
    getAppointment(id)
      .then((data) => { if (!cancelled) dispatch({ type: 'FETCH_SUCCESS', data }) })
      .catch((err) => {
        if (cancelled) return
        const errorKind: ErrorKind = err instanceof ApiError
          ? (err.status === 404 ? 'not_found' : 'server_error')
          : 'network_error'
        dispatch({ type: 'FETCH_ERROR', errorKind })
      })
    return () => { cancelled = true }
  }, [id, retryCount])

  const { appointment, isLoading, errorKind } = state

  if (isLoading) {
    return (
      <div className="space-y-3" data-testid="detail-loading">
        <Skeleton className="h-12 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (errorKind === 'not_found' || (!errorKind && !appointment)) {
    return (
      <p className="text-muted-foreground" data-testid="detail-not-found">
        {t('error_not_found')}
      </p>
    )
  }

  if (errorKind === 'server_error') {
    return (
      <div data-testid="detail-server-error" className="text-center space-y-2">
        <p className="text-destructive font-medium">{t('error_server_title')}</p>
        <p className="text-muted-foreground text-sm">{t('error_server_description')}</p>
        <button onClick={retry} className="text-sm underline">{t('retry')}</button>
      </div>
    )
  }

  if (errorKind === 'network_error') {
    return (
      <div data-testid="detail-network-error" className="text-center space-y-2">
        <p className="text-destructive font-medium">{t('error_network_title')}</p>
        <p className="text-muted-foreground text-sm">{t('error_network_description')}</p>
        <button onClick={retry} className="text-sm underline">{t('retry')}</button>
      </div>
    )
  }

  return <AppointmentDetail appointment={appointment!} />
}

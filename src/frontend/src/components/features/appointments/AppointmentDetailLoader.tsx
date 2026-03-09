'use client'

import { useEffect, useState } from 'react'
import { getAppointment } from '@/lib/api/appointments'
import type { AppointmentDto } from '@/lib/api/appointments'
import { AppointmentDetail } from './AppointmentDetail'
import { Skeleton } from '@/components/ui/skeleton'

interface Props {
  id: string
}

export function AppointmentDetailLoader({ id }: Props) {
  const [appointment, setAppointment] = useState<AppointmentDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [notFound, setNotFound] = useState(false)

  useEffect(() => {
    getAppointment(id)
      .then(setAppointment)
      .catch(() => setNotFound(true))
      .finally(() => setIsLoading(false))
  }, [id])

  if (isLoading) {
    return (
      <div className="space-y-3" data-testid="detail-loading">
        <Skeleton className="h-12 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (notFound || !appointment) {
    return (
      <p className="text-muted-foreground" data-testid="detail-not-found">
        Appointment not found.
      </p>
    )
  }

  return <AppointmentDetail appointment={appointment} />
}

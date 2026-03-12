'use client'

import { Badge } from '@/components/ui/badge'
import type { AppointmentStatus } from '@/lib/api/appointments'
import { useTranslations } from 'next-intl'

const STATUS_VARIANTS: Record<
  AppointmentStatus,
  'default' | 'secondary' | 'destructive' | 'outline'
> = {
  SCHEDULED: 'secondary',
  CHECKED_IN: 'default',
  IN_PROGRESS: 'default',
  COMPLETED: 'outline',
  CANCELLED: 'destructive',
}

interface StatusBadgeProps {
  status: AppointmentStatus
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const t = useTranslations('appointments.status')
  const variant = STATUS_VARIANTS[status] ?? 'secondary'
  return (
    <Badge variant={variant} data-testid={`status-badge-${status.toLowerCase()}`}>
      {t(status)}
    </Badge>
  )
}

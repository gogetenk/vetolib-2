'use client'

import { Badge } from '@/components/ui/badge'
import type { AppointmentStatus } from '@/lib/api/appointments'
import { useTranslations } from 'next-intl'
import { cn } from '@/lib/utils'

const STATUS_STYLES: Record<AppointmentStatus, { variant: 'default' | 'secondary' | 'destructive' | 'outline'; className?: string }> = {
  SCHEDULED: { variant: 'outline', className: 'border-blue-300 text-blue-700 bg-blue-50' },
  CHECKED_IN: { variant: 'secondary', className: 'border-amber-300 text-amber-700 bg-amber-50' },
  IN_PROGRESS: { variant: 'default', className: 'border-green-300 text-green-700 bg-green-50' },
  COMPLETED: { variant: 'outline', className: 'text-stone-600 bg-stone-50' },
  CANCELLED: { variant: 'destructive' },
}

interface StatusBadgeProps {
  status: AppointmentStatus
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const t = useTranslations('appointments.status')
  const style = STATUS_STYLES[status] ?? { variant: 'secondary' as const }
  return (
    <Badge
      variant={style.variant}
      className={cn(style.className)}
      data-testid={`status-badge-${status.toLowerCase()}`}
    >
      {t(status)}
    </Badge>
  )
}

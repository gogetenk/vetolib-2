'use client'

import { Badge } from '@/components/ui/badge'
import type { AppointmentStatus } from '@/lib/api/appointments'
import { useTranslations } from 'next-intl'
import { cn } from '@/lib/utils'

const STATUS_STYLES: Record<AppointmentStatus, { variant: 'default' | 'secondary' | 'destructive' | 'outline'; className?: string }> = {
  SCHEDULED: { variant: 'outline', className: 'border-primary/30 text-primary bg-primary/10 rounded-md text-[10px] font-bold uppercase tracking-wider' },
  CHECKED_IN: { variant: 'secondary', className: 'border-amber-300 text-amber-700 bg-amber-50 rounded-md text-[10px] font-bold uppercase tracking-wider' },
  IN_PROGRESS: { variant: 'default', className: 'border-green-300 text-green-700 bg-green-50 rounded-md text-[10px] font-bold uppercase tracking-wider' },
  COMPLETED: { variant: 'outline', className: 'text-muted-foreground bg-muted rounded-md text-[10px] font-bold uppercase tracking-wider' },
  CANCELLED: { variant: 'destructive', className: 'rounded-md text-[10px] font-bold uppercase tracking-wider' },
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

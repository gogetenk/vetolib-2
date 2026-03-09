import { Badge } from '@/components/ui/badge'
import type { AppointmentStatus } from '@/lib/api/appointments'

const STATUS_CONFIG: Record<
  AppointmentStatus,
  { label: string; variant: 'default' | 'secondary' | 'destructive' | 'outline' }
> = {
  SCHEDULED: { label: 'Scheduled', variant: 'secondary' },
  CHECKED_IN: { label: 'Checked In', variant: 'default' },
  IN_PROGRESS: { label: 'In Progress', variant: 'default' },
  COMPLETED: { label: 'Completed', variant: 'outline' },
  CANCELLED: { label: 'Cancelled', variant: 'destructive' },
}

interface StatusBadgeProps {
  status: AppointmentStatus
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const config = STATUS_CONFIG[status] ?? { label: status, variant: 'secondary' as const }
  return (
    <Badge variant={config.variant} data-testid={`status-badge-${status.toLowerCase()}`}>
      {config.label}
    </Badge>
  )
}

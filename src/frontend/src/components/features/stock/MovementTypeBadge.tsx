import { Badge } from '@/components/ui/badge'
import type { FullMovementType } from '@/lib/api/stock'

const BADGE_CONFIG: Record<FullMovementType, { label: string; className: string }> = {
  INCOMING: {
    label: 'Incoming',
    className: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  },
  OUTGOING: {
    label: 'Outgoing',
    className: 'bg-blue-50 text-blue-700 border-blue-200',
  },
  ADJUSTMENT: {
    label: 'Adjustment',
    className: 'bg-amber-50 text-amber-700 border-amber-200',
  },
  LOSS: {
    label: 'Loss',
    className: 'bg-red-50 text-red-700 border-red-200',
  },
  RETURN: {
    label: 'Return',
    className: 'bg-purple-50 text-purple-700 border-purple-200',
  },
}

interface MovementTypeBadgeProps {
  type: FullMovementType
  translatedLabel?: string
}

export function MovementTypeBadge({ type, translatedLabel }: MovementTypeBadgeProps) {
  const config = BADGE_CONFIG[type] ?? BADGE_CONFIG.ADJUSTMENT

  return (
    <Badge
      className={`rounded-md text-[10px] font-bold uppercase tracking-wider border ${config.className}`}
      data-testid={`badge-movement-type-${type.toLowerCase()}`}
    >
      {translatedLabel ?? config.label}
    </Badge>
  )
}

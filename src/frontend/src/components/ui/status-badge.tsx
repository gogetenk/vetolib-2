import { cn } from '@/lib/utils'

const STATUS_COLORS: Record<string, string> = {
  // Invoice statuses
  draft: 'bg-amber-100 text-amber-700',
  sent: 'bg-blue-100 text-blue-700',
  paid: 'bg-emerald-100 text-emerald-700',
  overdue: 'bg-red-100 text-red-700',
  // Appointment statuses
  scheduled: 'bg-blue-100 text-blue-700',
  checked_in: 'bg-indigo-100 text-indigo-700',
  in_progress: 'bg-amber-100 text-amber-700',
  completed: 'bg-emerald-100 text-emerald-700',
  cancelled: 'bg-red-100 text-red-700',
  // Message statuses
  open: 'bg-emerald-100 text-emerald-700',
  resolved: 'bg-stone-100 text-stone-600',
  closed: 'bg-stone-100 text-stone-500',
  // Generic
  active: 'bg-emerald-100 text-emerald-700',
  inactive: 'bg-stone-100 text-stone-500',
  low_stock: 'bg-red-100 text-red-700',
  ok: 'bg-emerald-100 text-emerald-700',
  expiring_soon: 'bg-amber-100 text-amber-700',
}

interface StatusBadgeProps {
  status: string
  className?: string
  'data-testid'?: string
}

export function StatusBadge({ status, className, ...props }: StatusBadgeProps) {
  const normalizedStatus = status.toLowerCase().replace(/[\s-]/g, '_')
  const colorClass = STATUS_COLORS[normalizedStatus] ?? 'bg-stone-100 text-stone-600'
  return (
    <span
      className={cn('inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium', colorClass, className)}
      data-testid={props['data-testid']}
    >
      {status}
    </span>
  )
}

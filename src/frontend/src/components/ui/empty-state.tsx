import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import type { LucideIcon } from 'lucide-react'

interface EmptyStateProps {
  icon: LucideIcon
  title: string
  description?: string
  actionLabel?: string
  onAction?: () => void
  actionHref?: string
  className?: string
  'data-testid'?: string
}

export function EmptyState({ icon: Icon, title, description, actionLabel, onAction, actionHref, className, ...props }: EmptyStateProps) {
  return (
    <div className={cn('flex flex-col items-center justify-center py-16 text-center', className)} data-testid={props['data-testid']}>
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-stone-100 text-stone-400 mb-4">
        <Icon className="h-8 w-8" />
      </div>
      <h3 className="text-lg font-semibold text-stone-700">{title}</h3>
      {description && <p className="mt-1 max-w-sm text-sm text-stone-500">{description}</p>}
      {actionLabel && (onAction || actionHref) && (
        <Button
          className="mt-4 bg-primary hover:bg-primary/90 text-primary-foreground"
          onClick={onAction}
          data-testid={props['data-testid'] ? `${props['data-testid']}-action` : undefined}
        >
          {actionLabel}
        </Button>
      )}
    </div>
  )
}

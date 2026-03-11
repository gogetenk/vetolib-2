'use client'

import { AlertCircle } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface ErrorStateProps {
  title?: string
  description?: string
  onRetry?: () => void
  retryLabel?: string
  className?: string
  'data-testid'?: string
}

export function ErrorState({
  title = 'Something went wrong',
  description = 'An error occurred while loading data.',
  onRetry,
  retryLabel = 'Retry',
  className,
  'data-testid': testId = 'error-state',
}: ErrorStateProps) {
  return (
    <div
      data-testid={testId}
      className={cn(
        'flex flex-col items-center justify-center py-12 px-6 text-center gap-4',
        className
      )}
    >
      <AlertCircle
        className="h-12 w-12 text-destructive"
        aria-hidden="true"
        data-testid={`${testId}-icon`}
      />
      <div className="space-y-1 max-w-sm">
        <p
          className="text-sm font-semibold text-foreground"
          data-testid={`${testId}-title`}
        >
          {title}
        </p>
        <p
          className="text-sm text-muted-foreground"
          data-testid={`${testId}-description`}
        >
          {description}
        </p>
      </div>
      {onRetry && (
        <Button
          variant="outline"
          size="sm"
          onClick={onRetry}
          data-testid={`${testId}-retry-btn`}
        >
          {retryLabel}
        </Button>
      )}
    </div>
  )
}

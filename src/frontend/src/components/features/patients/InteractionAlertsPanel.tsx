'use client'

import { useState } from 'react'
import { AlertTriangle, AlertCircle, Info, ChevronDown, ChevronUp } from 'lucide-react'
import { Skeleton } from '@/components/ui/skeleton'
import type { InteractionAlert, AlertSeverity } from '@/lib/api/types'

// ─── Config ──────────────────────────────────────────────────────────────────

const MAX_VISIBLE = 5

function alertConfig(severity: AlertSeverity) {
  switch (severity) {
    case 'Critical':
      return {
        containerClass:
          'border border-red-300 bg-red-50 text-red-900 dark:border-red-800 dark:bg-red-950 dark:text-red-200',
        iconClass: 'text-red-600 dark:text-red-400 shrink-0',
        Icon: AlertTriangle,
      }
    case 'Moderate':
      return {
        containerClass:
          'border border-amber-300 bg-amber-50 text-amber-900 dark:border-amber-800 dark:bg-amber-950 dark:text-amber-200',
        iconClass: 'text-amber-600 dark:text-amber-400 shrink-0',
        Icon: AlertCircle,
      }
    case 'Info':
      return {
        containerClass:
          'border border-blue-200 bg-blue-50 text-blue-900 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-200',
        iconClass: 'text-blue-500 dark:text-blue-400 shrink-0',
        Icon: Info,
      }
  }
}

// ─── Props ───────────────────────────────────────────────────────────────────

interface InteractionAlertsPanelProps {
  alerts: InteractionAlert[]
  /** Show skeleton placeholders while preflight is loading */
  isLoading?: boolean
}

// ─── Component ───────────────────────────────────────────────────────────────

export function InteractionAlertsPanel({
  alerts,
  isLoading = false,
}: InteractionAlertsPanelProps) {
  const [expanded, setExpanded] = useState(false)

  // ── Skeleton state ──────────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="space-y-2" data-testid="interaction-alerts-loading">
        <Skeleton className="h-12 w-full rounded-md" />
        <Skeleton className="h-12 w-full rounded-md" />
      </div>
    )
  }

  if (alerts.length === 0) return null

  const visible = expanded ? alerts : alerts.slice(0, MAX_VISIBLE)
  const hiddenCount = alerts.length - MAX_VISIBLE

  return (
    <div className="space-y-2" data-testid="interaction-alerts-panel">
      {visible.map((alert, index) => {
        const { containerClass, iconClass, Icon } = alertConfig(alert.severity)
        return (
          <div
            key={index}
            className={`flex items-start gap-3 rounded-md p-3 text-sm ${containerClass}`}
            data-testid={`interaction-alert-${alert.severity.toLowerCase()}-${index}`}
            role="alert"
          >
            <Icon className={`mt-0.5 h-4 w-4 ${iconClass}`} aria-hidden="true" />
            <div className="flex-1 min-w-0">
              <span className="font-semibold capitalize">{alert.severity}: </span>
              {alert.message}
            </div>
          </div>
        )
      })}

      {alerts.length > MAX_VISIBLE && (
        <button
          type="button"
          data-testid="interaction-alerts-expand"
          onClick={() => setExpanded(prev => !prev)}
          className="flex items-center gap-1 text-xs text-muted-foreground underline underline-offset-2 hover:text-foreground transition-colors"
          aria-expanded={expanded}
        >
          {expanded ? (
            <>
              <ChevronUp className="h-3 w-3" />
              Show fewer alerts
            </>
          ) : (
            <>
              <ChevronDown className="h-3 w-3" />
              {hiddenCount} more alert{hiddenCount !== 1 ? 's' : ''}
            </>
          )}
        </button>
      )}
    </div>
  )
}

'use client'

import { useEffect, useState, useCallback } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { getRecentActivity, type ActivityDto, type ActivityType } from '@/lib/api/dashboard'
import { ErrorState } from '@/components/ui/error-state'
import { LtrText } from '@/components/ui/ltr-text'
import { useTranslations } from 'next-intl'

const ACTIVITY_ICONS: Record<ActivityType, string> = {
  APPOINTMENT: '\uD83D\uDCC5',
  MEDICAL: '\uD83D\uDC8A',
  BILLING: '\uD83D\uDCB0',
  MESSAGE: '\uD83D\uDCAC',
}

const ACTIVITY_BORDER_COLORS: Record<ActivityType, string> = {
  APPOINTMENT: 'border-s-blue-400',
  MEDICAL: 'border-s-success',
  BILLING: 'border-s-amber-400',
  MESSAGE: 'border-s-violet-400',
}

function formatRelativeTime(isoDate: string): string {
  const diffMs = Date.now() - new Date(isoDate).getTime()
  const diffMins = Math.floor(diffMs / 60_000)
  if (diffMins < 60) return `${diffMins} min ago`
  const diffHours = Math.floor(diffMins / 60)
  if (diffHours < 24) return `${diffHours}h ago`
  return new Date(isoDate).toLocaleDateString('en-AE', { timeZone: 'Asia/Dubai' })
}

function formatTime(isoDate: string): string {
  return new Date(isoDate).toLocaleTimeString('en-AE', {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Dubai',
  })
}

export function RecentActivity() {
  const t = useTranslations('dashboard.recent_activity')
  const tErr = useTranslations('dashboard.errors')
  const [activities, setActivities] = useState<ActivityDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      const result = await getRecentActivity()
      setActivities(result)
      setError(null)
    } catch {
      setError('Failed to load recent activity')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  return (
    <Card className="border-border/80 shadow-sm" data-testid="recent-activity-card">
      <CardHeader className="pb-2">
        <CardTitle className="text-[15px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-4 bg-primary rounded-full"></span>
          {t('title')}
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        {loading ? (
          <div className="space-y-3 p-4" data-testid="recent-activity-loading">
            {[1, 2, 3, 4, 5].map((i) => (
              <Skeleton key={i} className="h-8 w-full" />
            ))}
          </div>
        ) : error ? (
          <ErrorState
            data-testid="recent-activity-error"
            title={tErr('load_activity')}
            description={error}
            onRetry={load}
          />
        ) : activities.length === 0 ? (
          <p
            className="p-4 text-[13px] text-muted-foreground"
            data-testid="recent-activity-empty"
          >
            {t('empty')}
          </p>
        ) : (
          <ul data-testid="recent-activity-list" className="divide-y divide-border/30">
            {activities.map((activity) => (
              <li
                key={activity.id}
                data-testid={`activity-item-${activity.id}`}
                className={`flex items-start gap-3 px-4 py-3 border-s-2 ${ACTIVITY_BORDER_COLORS[activity.type]}`}
              >
                <span
                  className="text-base mt-0.5 shrink-0"
                  data-testid={`activity-icon-${activity.id}`}
                  aria-label={activity.type}
                >
                  {ACTIVITY_ICONS[activity.type]}
                </span>
                <div className="flex-1 min-w-0">
                  <p
                    className="text-[13px] text-foreground font-medium truncate"
                    data-testid={`activity-message-${activity.id}`}
                  >
                    {activity.message}
                  </p>
                  <p
                    className="text-[12px] text-muted-foreground mt-0.5"
                    data-testid={`activity-time-${activity.id}`}
                    title={new Date(activity.occurredAt).toLocaleString('en-AE', { timeZone: 'Asia/Dubai' })}
                  >
                    <LtrText>{formatTime(activity.occurredAt)}</LtrText> · {formatRelativeTime(activity.occurredAt)}
                  </p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}

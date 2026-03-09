'use client'

import { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { getRecentActivity, type ActivityDto, type ActivityType } from '@/lib/api/dashboard'

const ACTIVITY_ICONS: Record<ActivityType, string> = {
  APPOINTMENT: '📅',
  MEDICAL: '💊',
  BILLING: '💰',
}

function formatRelativeTime(isoDate: string): string {
  const diffMs = Date.now() - new Date(isoDate).getTime()
  const diffMins = Math.floor(diffMs / 60_000)
  if (diffMins < 60) return `il y a ${diffMins} min`
  const diffHours = Math.floor(diffMins / 60)
  if (diffHours < 24) return `il y a ${diffHours}h`
  return new Date(isoDate).toLocaleDateString('fr-AE', { timeZone: 'Asia/Dubai' })
}

function formatTime(isoDate: string): string {
  return new Date(isoDate).toLocaleTimeString('fr-AE', {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Dubai',
  })
}

export function RecentActivity() {
  const [activities, setActivities] = useState<ActivityDto[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getRecentActivity()
      .then(setActivities)
      .catch(() => {
        // silently fail — dashboard is non-critical
      })
      .finally(() => setLoading(false))
  }, [])

  return (
    <Card data-testid="recent-activity-card">
      <CardHeader className="pb-2">
        <CardTitle className="text-base font-semibold">Activité récente</CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        {loading ? (
          <div className="space-y-3 p-4">
            {[1, 2, 3, 4, 5].map((i) => (
              <Skeleton key={i} className="h-8 w-full" />
            ))}
          </div>
        ) : activities.length === 0 ? (
          <p
            className="p-4 text-sm text-muted-foreground"
            data-testid="recent-activity-empty"
          >
            Aucune activité récente
          </p>
        ) : (
          <ul data-testid="recent-activity-list" className="divide-y">
            {activities.map((activity) => (
              <li
                key={activity.id}
                data-testid={`activity-item-${activity.id}`}
                className="flex items-start gap-3 px-4 py-3"
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
                    className="text-sm truncate"
                    data-testid={`activity-message-${activity.id}`}
                  >
                    {activity.message}
                  </p>
                  <p
                    className="text-xs text-muted-foreground mt-0.5"
                    data-testid={`activity-time-${activity.id}`}
                    title={new Date(activity.occurredAt).toLocaleString('fr-AE', { timeZone: 'Asia/Dubai' })}
                  >
                    {formatTime(activity.occurredAt)} · {formatRelativeTime(activity.occurredAt)}
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

'use client'

import { useEffect, useState, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { AlertTriangle, CheckCircle, XCircle, CalendarPlus, ShieldAlert, Info } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { getHealthAlerts, acknowledgeAlert, convertAlertToAppointment } from '@/lib/api/health-alerts'
import type { HealthAlertDto, AlertSeverity } from '@/lib/api/health-alerts'
import { DismissAlertDialog } from '@/components/features/dashboard/DismissAlertDialog'
import { toast } from 'sonner'
import { useTranslations } from 'next-intl'

function severityIcon(severity: AlertSeverity) {
  switch (severity) {
    case 'High':
      return <AlertTriangle className="h-4 w-4 text-red-500" />
    case 'Medium':
      return <ShieldAlert className="h-4 w-4 text-amber-500" />
    case 'Low':
      return <Info className="h-4 w-4 text-blue-500" />
  }
}

function severityBadgeVariant(severity: AlertSeverity): 'destructive' | 'default' | 'secondary' {
  switch (severity) {
    case 'High':
      return 'destructive'
    case 'Medium':
      return 'default'
    case 'Low':
      return 'secondary'
  }
}

export function HealthAlertPanel() {
  const t = useTranslations('health_alerts')
  const router = useRouter()
  const [alerts, setAlerts] = useState<HealthAlertDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [dismissingAlert, setDismissingAlert] = useState<HealthAlertDto | null>(null)

  useEffect(() => {
    let cancelled = false
    getHealthAlerts()
      .then((data) => { if (!cancelled) setAlerts(data) })
      .catch(() => {
        if (!cancelled) {
          setAlerts([])
          toast.error(t('errors.load_failed'))
        }
      })
      .finally(() => { if (!cancelled) setIsLoading(false) })
    return () => { cancelled = true }
  }, [t])

  const handleAcknowledge = useCallback(async (alert: HealthAlertDto) => {
    try {
      await acknowledgeAlert(alert.id)
      setAlerts(prev => prev.map(a => a.id === alert.id ? { ...a, status: 'Acknowledged' as const } : a))
      toast.success(t('dismiss_dialog.success', { patientName: alert.patientName }))
    } catch {
      toast.error(t('errors.acknowledge_failed'))
    }
  }, [t])

  const handleConvertToAppointment = useCallback(async (alert: HealthAlertDto) => {
    try {
      const preFill = await convertAlertToAppointment(alert.id)
      router.push(`/appointments/new?patientId=${preFill.patientId}&reason=${encodeURIComponent(preFill.reason)}`)
    } catch {
      toast.error(t('errors.prepare_appointment_failed'))
    }
  }, [router, t])

  const handleDismissConfirm = useCallback(() => {
    if (!dismissingAlert) return
    setAlerts(prev => prev.filter(a => a.id !== dismissingAlert.id))
    setDismissingAlert(null)
  }, [dismissingAlert])

  const totalCount = alerts.length
  const grouped: Record<AlertSeverity, HealthAlertDto[]> = {
    High: alerts.filter(a => a.severity === 'High'),
    Medium: alerts.filter(a => a.severity === 'Medium'),
    Low: alerts.filter(a => a.severity === 'Low'),
  }

  if (isLoading) {
    return (
      <div data-testid="health-alerts-loading" className="bg-white border border-border/80 rounded-xl shadow-sm p-6">
        <div className="h-6 w-48 rounded bg-muted animate-pulse mb-4" />
        <div className="space-y-3">
          <div className="h-16 rounded bg-muted animate-pulse" />
          <div className="h-16 rounded bg-muted animate-pulse" />
        </div>
      </div>
    )
  }

  if (totalCount === 0) {
    return (
      <div
        data-testid="health-alerts-panel"
        className="bg-white border border-border/80 rounded-xl shadow-sm p-6"
      >
        <div className="flex items-center gap-2 mb-2">
          <ShieldAlert className="h-5 w-5 text-muted-foreground" />
          <h2 className="text-base font-semibold text-foreground">{t('title')}</h2>
        </div>
        <p className="text-sm text-muted-foreground" data-testid="health-alerts-empty">
          {t('empty')}
        </p>
      </div>
    )
  }

  return (
    <div
      data-testid="health-alerts-panel"
      className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden"
    >
      <div className="flex items-center justify-between p-4 border-b border-border/50">
        <div className="flex items-center gap-2">
          <ShieldAlert className="h-5 w-5 text-foreground" />
          <h2 className="text-base font-semibold text-foreground">{t('title')}</h2>
          <Badge
            variant="destructive"
            data-testid="health-alerts-count-badge"
          >
            {totalCount}
          </Badge>
        </div>
      </div>

      <div className="divide-y divide-border/30">
        {(['High', 'Medium', 'Low'] as AlertSeverity[]).map(severity => {
          const items = grouped[severity]
          if (items.length === 0) return null
          return (
            <div key={severity} data-testid={`health-alerts-group-${severity.toLowerCase()}`}>
              <div className="flex items-center gap-2 px-4 py-2 bg-muted/50">
                {severityIcon(severity)}
                <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                  {t(`severity.${severity.toLowerCase()}`)}
                </span>
                <Badge variant={severityBadgeVariant(severity)} className="text-[10px]">
                  {items.length}
                </Badge>
              </div>
              {items.map(alert => (
                <div
                  key={alert.id}
                  data-testid={`health-alert-${alert.id}`}
                  className="flex flex-col sm:flex-row sm:items-center gap-3 px-4 py-3 hover:bg-muted/30 transition-colors"
                >
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-0.5">
                      <span
                        className="text-[13px] font-semibold text-foreground truncate"
                        data-testid={`alert-patient-name-${alert.id}`}
                      >
                        {alert.patientName}
                      </span>
                      <span className="text-[11px] text-muted-foreground">
                        {alert.breed} &bull; {alert.age}
                      </span>
                      {alert.status === 'Acknowledged' && (
                        <Badge variant="outline" className="text-[10px]" data-testid={`alert-acknowledged-badge-${alert.id}`}>
                          <CheckCircle className="h-3 w-3 me-1" />
                          {t('acknowledged')}
                        </Badge>
                      )}
                    </div>
                    <p
                      className="text-[13px] font-medium text-foreground"
                      data-testid={`alert-title-${alert.id}`}
                    >
                      {alert.title}
                    </p>
                    <p
                      className="text-[12px] text-muted-foreground line-clamp-2"
                      data-testid={`alert-description-${alert.id}`}
                    >
                      {alert.description}
                    </p>
                  </div>
                  <div className="flex items-center gap-2 shrink-0">
                    <Button
                      variant="default"
                      size="sm"
                      data-testid={`alert-schedule-btn-${alert.id}`}
                      onClick={() => handleConvertToAppointment(alert)}
                      className="text-xs"
                    >
                      <CalendarPlus className="h-3.5 w-3.5 me-1" />
                      {t('schedule_appointment')}
                    </Button>
                    {alert.status !== 'Acknowledged' && (
                      <Button
                        variant="outline"
                        size="sm"
                        data-testid={`alert-acknowledge-btn-${alert.id}`}
                        onClick={() => handleAcknowledge(alert)}
                        className="text-xs"
                      >
                        <CheckCircle className="h-3.5 w-3.5 me-1" />
                        {t('acknowledge')}
                      </Button>
                    )}
                    <Button
                      variant="ghost"
                      size="sm"
                      data-testid={`alert-dismiss-btn-${alert.id}`}
                      onClick={() => setDismissingAlert(alert)}
                      className="text-xs text-muted-foreground"
                    >
                      <XCircle className="h-3.5 w-3.5 me-1" />
                      {t('dismiss')}
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )
        })}
      </div>

      <DismissAlertDialog
        alert={dismissingAlert}
        onConfirm={handleDismissConfirm}
        onCancel={() => setDismissingAlert(null)}
      />
    </div>
  )
}

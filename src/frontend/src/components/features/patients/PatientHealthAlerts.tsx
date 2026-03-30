'use client'

import { useEffect, useState, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { AlertTriangle, ShieldAlert, Info, CheckCircle, XCircle, CalendarPlus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { getPatientHealthAlerts, acknowledgeAlert, convertAlertToAppointment } from '@/lib/api/health-alerts'
import type { HealthAlertDto, AlertSeverity } from '@/lib/api/health-alerts'
import { DismissAlertDialog } from '@/components/features/dashboard/DismissAlertDialog'
import { toast } from 'sonner'

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

function statusColor(status: string): string {
  switch (status) {
    case 'Active':
      return ''
    case 'Acknowledged':
      return 'opacity-75'
    case 'Dismissed':
      return 'opacity-50'
    default:
      return ''
  }
}

interface PatientHealthAlertsProps {
  patientId: string
}

export function PatientHealthAlerts({ patientId }: PatientHealthAlertsProps) {
  const router = useRouter()
  const [alerts, setAlerts] = useState<HealthAlertDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [dismissingAlert, setDismissingAlert] = useState<HealthAlertDto | null>(null)

  useEffect(() => {
    if (!patientId) return
    let cancelled = false
    getPatientHealthAlerts(patientId)
      .then((data) => { if (!cancelled) setAlerts(data) })
      .catch(() => {
        if (!cancelled) {
          setAlerts([])
          toast.error('Failed to load health alerts')
        }
      })
      .finally(() => { if (!cancelled) setIsLoading(false) })
    return () => { cancelled = true }
  }, [patientId])

  const handleAcknowledge = useCallback(async (alert: HealthAlertDto) => {
    try {
      await acknowledgeAlert(alert.id)
      setAlerts(prev => prev.map(a => a.id === alert.id ? { ...a, status: 'Acknowledged' as const } : a))
      toast.success('Alert acknowledged')
    } catch {
      toast.error('Failed to acknowledge alert')
    }
  }, [])

  const handleConvert = useCallback(async (alert: HealthAlertDto) => {
    try {
      const preFill = await convertAlertToAppointment(alert.id)
      router.push(`/appointments/new?patientId=${preFill.patientId}&reason=${encodeURIComponent(preFill.reason)}`)
    } catch {
      toast.error('Failed to prepare appointment')
    }
  }, [router])

  const handleDismissConfirm = useCallback(() => {
    if (!dismissingAlert) return
    setAlerts(prev => prev.map(a => a.id === dismissingAlert.id ? { ...a, status: 'Dismissed' as const } : a))
    setDismissingAlert(null)
  }, [dismissingAlert])

  if (isLoading) {
    return (
      <div data-testid="patient-health-alerts-loading" className="space-y-3">
        <div className="h-16 rounded bg-muted animate-pulse" />
        <div className="h-16 rounded bg-muted animate-pulse" />
      </div>
    )
  }

  if (alerts.length === 0) {
    return (
      <p className="text-muted-foreground text-[13px] py-8 text-center" data-testid="patient-health-alerts-empty">
        No health alerts for this patient. All care is up to date.
      </p>
    )
  }

  const activeAlerts = alerts.filter(a => a.status !== 'Dismissed')
  const dismissedAlerts = alerts.filter(a => a.status === 'Dismissed')

  return (
    <div data-testid="patient-health-alerts-list" className="space-y-4">
      {activeAlerts.length > 0 && (
        <div className="bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden">
          <div className="px-4 py-3 bg-muted border-b border-border/50">
            <span className="text-[11px] font-bold text-foreground uppercase tracking-wider">
              Active Alerts
            </span>
          </div>
          {activeAlerts.map(alert => (
            <div
              key={alert.id}
              data-testid={`patient-alert-${alert.id}`}
              className={`flex flex-col sm:flex-row sm:items-center gap-3 px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-muted/30 transition-colors ${statusColor(alert.status)}`}
            >
              <div className="flex items-start gap-2 flex-1 min-w-0">
                {severityIcon(alert.severity)}
                <div>
                  <div className="flex items-center gap-2 mb-0.5">
                    <span className="text-[13px] font-semibold text-foreground" data-testid={`patient-alert-title-${alert.id}`}>
                      {alert.title}
                    </span>
                    <Badge
                      variant={alert.severity === 'High' ? 'destructive' : alert.severity === 'Medium' ? 'default' : 'secondary'}
                      className="text-[10px]"
                    >
                      {alert.severity}
                    </Badge>
                    {alert.status === 'Acknowledged' && (
                      <Badge variant="outline" className="text-[10px]">
                        <CheckCircle className="h-3 w-3 mr-1" />
                        Acknowledged
                      </Badge>
                    )}
                  </div>
                  <p className="text-[12px] text-muted-foreground" data-testid={`patient-alert-desc-${alert.id}`}>
                    {alert.description}
                  </p>
                </div>
              </div>
              <div className="flex items-center gap-2 shrink-0">
                <Button
                  variant="default"
                  size="sm"
                  data-testid={`patient-alert-schedule-btn-${alert.id}`}
                  onClick={() => handleConvert(alert)}
                  className="text-xs"
                >
                  <CalendarPlus className="h-3.5 w-3.5 mr-1" />
                  Schedule
                </Button>
                {alert.status !== 'Acknowledged' && (
                  <Button
                    variant="outline"
                    size="sm"
                    data-testid={`patient-alert-acknowledge-btn-${alert.id}`}
                    onClick={() => handleAcknowledge(alert)}
                    className="text-xs"
                  >
                    <CheckCircle className="h-3.5 w-3.5 mr-1" />
                    Acknowledge
                  </Button>
                )}
                <Button
                  variant="ghost"
                  size="sm"
                  data-testid={`patient-alert-dismiss-btn-${alert.id}`}
                  onClick={() => setDismissingAlert(alert)}
                  className="text-xs text-muted-foreground"
                >
                  <XCircle className="h-3.5 w-3.5 mr-1" />
                  Dismiss
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {dismissedAlerts.length > 0 && (
        <div className="bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden">
          <div className="px-4 py-3 bg-muted border-b border-border/50">
            <span className="text-[11px] font-bold text-muted-foreground uppercase tracking-wider">
              Dismissed Alerts
            </span>
          </div>
          {dismissedAlerts.map(alert => (
            <div
              key={alert.id}
              data-testid={`patient-alert-${alert.id}`}
              className="flex flex-col gap-1 px-4 py-3 border-b border-border/30 last:border-b-0 opacity-50"
            >
              <div className="flex items-center gap-2">
                {severityIcon(alert.severity)}
                <span className="text-[13px] font-medium text-muted-foreground line-through" data-testid={`patient-alert-title-${alert.id}`}>
                  {alert.title}
                </span>
                <Badge variant="secondary" className="text-[10px]">Dismissed</Badge>
              </div>
              {alert.dismissReason && (
                <p className="text-[11px] text-muted-foreground ml-6" data-testid={`patient-alert-dismiss-reason-${alert.id}`}>
                  Reason: {alert.dismissReason}
                </p>
              )}
            </div>
          ))}
        </div>
      )}

      <DismissAlertDialog
        alert={dismissingAlert}
        onConfirm={handleDismissConfirm}
        onCancel={() => setDismissingAlert(null)}
      />
    </div>
  )
}

'use client'

import { useState } from 'react'
import { format } from 'date-fns'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Textarea } from '@/components/ui/textarea'
import { Label } from '@/components/ui/label'
import { StatusBadge } from './StatusBadge'
import { LtrText } from '@/components/ui/ltr-text'
import { transitionAppointment, cancelAppointment } from '@/lib/api/appointments'
import type { AppointmentDto, AppointmentAction } from '@/lib/api/appointments'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'
import { useTranslations } from 'next-intl'

interface TransitionConfig {
  action: AppointmentAction
  labelKey: string
  confirmTitleKey: string
  confirmDescriptionKey: string
  requiresReason?: boolean
  variant?: 'default' | 'destructive' | 'outline'
}

const TRANSITIONS: Record<string, TransitionConfig[]> = {
  SCHEDULED: [
    {
      action: 'CHECK_IN',
      labelKey: 'actions.check_in',
      confirmTitleKey: 'actions.check_in_title',
      confirmDescriptionKey: 'actions.check_in_description',
      variant: 'default',
    },
    {
      action: 'CANCEL',
      labelKey: 'actions.cancel',
      confirmTitleKey: 'actions.cancel_title',
      confirmDescriptionKey: 'actions.cancel_description',
      requiresReason: true,
      variant: 'destructive',
    },
  ],
  CHECKED_IN: [
    {
      action: 'START',
      labelKey: 'actions.start',
      confirmTitleKey: 'actions.start_title',
      confirmDescriptionKey: 'actions.start_description',
      variant: 'default',
    },
  ],
  IN_PROGRESS: [
    {
      action: 'COMPLETE',
      labelKey: 'actions.complete',
      confirmTitleKey: 'actions.complete_title',
      confirmDescriptionKey: 'actions.complete_description',
      variant: 'default',
    },
  ],
  COMPLETED: [],
  CANCELLED: [],
}

interface AppointmentDetailProps {
  appointment: AppointmentDto
}

export function AppointmentDetail({ appointment: initial }: AppointmentDetailProps) {
  const t = useTranslations('appointments.detail')
  const [appointment, setAppointment] = useState<AppointmentDto>(initial)
  const [pendingAction, setPendingAction] = useState<TransitionConfig | null>(null)
  const [cancelReason, setCancelReason] = useState('')
  const [isProcessing, setIsProcessing] = useState(false)

  const transitions = TRANSITIONS[appointment.status] ?? []

  const handleConfirm = async () => {
    if (!pendingAction) return
    setIsProcessing(true)
    const fromStatus = appointment.status
    try {
      let updated: AppointmentDto
      if (pendingAction.action === 'CANCEL') {
        updated = await cancelAppointment(appointment.id, cancelReason)
      } else {
        updated = await transitionAppointment(appointment.id, pendingAction.action)
      }
      trackEvent(AnalyticsEvents.APPOINTMENT_STATUS_CHANGED, {
        from_status: fromStatus,
        to_status: updated.status,
      })
      setAppointment(updated)
      toast.success(t('toast.success'))
      setPendingAction(null)
      setCancelReason('')
    } catch {
      toast.error(t('toast.failed'))
    } finally {
      setIsProcessing(false)
    }
  }

  return (
    <>
      <Card data-testid="appointment-detail">
        <CardHeader className="flex flex-row items-start justify-between">
          <div>
            <CardTitle data-testid="detail-patient-name">{appointment.patientName}</CardTitle>
            <p className="text-sm text-muted-foreground mt-1" data-testid="detail-species">
              {appointment.species}
            </p>
          </div>
          <StatusBadge status={appointment.status} />
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <p className="text-sm font-medium text-muted-foreground">{t('owner')}</p>
              <p data-testid="detail-owner-name">{appointment.ownerName}</p>
              <p className="text-sm text-muted-foreground" data-testid="detail-owner-phone">
                <a href={`tel:${appointment.ownerPhone.replace(/\s+/g, '')}`} className="text-primary hover:underline">
                  <LtrText>{appointment.ownerPhone}</LtrText>
                </a>
              </p>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">{t('vet')}</p>
              <p data-testid="detail-vet-name">{appointment.vetName}</p>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">{t('datetime')}</p>
              <p data-testid="detail-datetime">
                <LtrText>{format(new Date(appointment.scheduledAt), 'dd MMM yyyy HH:mm')}</LtrText>
              </p>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">{t('reason')}</p>
              <p data-testid="detail-reason">{appointment.reason}</p>
            </div>
            {appointment.notes && (
              <div className="md:col-span-2">
                <p className="text-sm font-medium text-muted-foreground">{t('notes')}</p>
                <p data-testid="detail-notes">{appointment.notes}</p>
              </div>
            )}
            {appointment.cancellationReason && (
              <div className="md:col-span-2">
                <p className="text-sm font-medium text-muted-foreground">{t('cancellation_reason')}</p>
                <p
                  className="text-destructive"
                  data-testid="detail-cancellation-reason"
                >
                  {appointment.cancellationReason}
                </p>
              </div>
            )}
          </div>

          {/* Transition buttons */}
          {transitions.length > 0 && (
            <div className="flex gap-3 pt-4 border-t" data-testid="transition-actions">
              {transitions.map((tr) => (
                <Button
                  key={tr.action}
                  variant={tr.variant ?? 'default'}
                  data-testid={`btn-action-${tr.action.toLowerCase().replace('_', '-')}`}
                  onClick={() => setPendingAction(tr)}
                >
                  {t(tr.labelKey)}
                </Button>
              ))}
            </div>
          )}

        </CardContent>
      </Card>

      {/* Confirmation dialog */}
      <Dialog
        open={pendingAction !== null}
        onOpenChange={(open) => {
          if (!open) {
            setPendingAction(null)
            setCancelReason('')
          }
        }}
      >
        <DialogContent data-testid="confirm-dialog">
          <DialogHeader>
            <DialogTitle data-testid="confirm-dialog-title">
              {pendingAction ? t(pendingAction.confirmTitleKey) : ''}
            </DialogTitle>
            <DialogDescription data-testid="confirm-dialog-description">
              {pendingAction ? t(pendingAction.confirmDescriptionKey) : ''}
            </DialogDescription>
          </DialogHeader>

          {pendingAction?.requiresReason && (
            <div className="space-y-2">
              <Label htmlFor="cancel-reason">{t('reason')}</Label>
              <Textarea
                id="cancel-reason"
                data-testid="input-cancel-reason"
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
                placeholder={t('cancel_reason_placeholder')}
                rows={3}
              />
            </div>
          )}

          <DialogFooter>
            <Button
              variant="outline"
              data-testid="btn-dialog-cancel"
              onClick={() => {
                setPendingAction(null)
                setCancelReason('')
              }}
            >
              {t('dialog_back')}
            </Button>
            <Button
              variant={pendingAction?.variant ?? 'default'}
              data-testid="btn-dialog-confirm"
              onClick={handleConfirm}
              disabled={
                isProcessing ||
                (pendingAction?.requiresReason === true && cancelReason.trim() === '')
              }
            >
              {isProcessing ? t('processing') : t('confirm')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}

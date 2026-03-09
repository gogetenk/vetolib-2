'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
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
import { transitionAppointment, cancelAppointment } from '@/lib/api/appointments'
import type { AppointmentDto, AppointmentAction } from '@/lib/api/appointments'

interface TransitionConfig {
  action: AppointmentAction
  label: string
  confirmTitle: string
  confirmDescription: string
  requiresReason?: boolean
  variant?: 'default' | 'destructive' | 'outline'
}

const TRANSITIONS: Record<string, TransitionConfig[]> = {
  SCHEDULED: [
    {
      action: 'CHECK_IN',
      label: 'Check In',
      confirmTitle: 'Check In Patient',
      confirmDescription: 'The patient has arrived and is ready to be checked in.',
      variant: 'default',
    },
    {
      action: 'CANCEL',
      label: 'Cancel',
      confirmTitle: 'Cancel Appointment',
      confirmDescription: 'Please provide a reason for cancellation.',
      requiresReason: true,
      variant: 'destructive',
    },
  ],
  CHECKED_IN: [
    {
      action: 'START',
      label: 'Start Consultation',
      confirmTitle: 'Start Consultation',
      confirmDescription: 'Begin the consultation for this appointment.',
      variant: 'default',
    },
  ],
  IN_PROGRESS: [
    {
      action: 'COMPLETE',
      label: 'Complete',
      confirmTitle: 'Complete Appointment',
      confirmDescription: 'Mark this consultation as completed.',
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
  const router = useRouter()
  const [appointment, setAppointment] = useState<AppointmentDto>(initial)
  const [pendingAction, setPendingAction] = useState<TransitionConfig | null>(null)
  const [cancelReason, setCancelReason] = useState('')
  const [isProcessing, setIsProcessing] = useState(false)

  const transitions = TRANSITIONS[appointment.status] ?? []

  const handleConfirm = async () => {
    if (!pendingAction) return
    setIsProcessing(true)
    try {
      let updated: AppointmentDto
      if (pendingAction.action === 'CANCEL') {
        updated = await cancelAppointment(appointment.id, cancelReason)
      } else {
        updated = await transitionAppointment(appointment.id, pendingAction.action)
      }
      setAppointment(updated)
      toast.success(`Appointment ${pendingAction.label.toLowerCase()}d successfully`)
      setPendingAction(null)
      setCancelReason('')
    } catch {
      toast.error(`Failed to ${pendingAction.label.toLowerCase()} appointment`)
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
              <p className="text-sm font-medium text-muted-foreground">Owner</p>
              <p data-testid="detail-owner-name">{appointment.ownerName}</p>
              <p className="text-sm text-muted-foreground" data-testid="detail-owner-phone">
                {appointment.ownerPhone}
              </p>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">Veterinarian</p>
              <p data-testid="detail-vet-name">{appointment.vetName}</p>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">Date & Time</p>
              <p data-testid="detail-datetime">
                {format(new Date(appointment.scheduledAt), 'dd MMM yyyy HH:mm')}
              </p>
            </div>
            <div>
              <p className="text-sm font-medium text-muted-foreground">Reason</p>
              <p data-testid="detail-reason">{appointment.reason}</p>
            </div>
            {appointment.notes && (
              <div className="md:col-span-2">
                <p className="text-sm font-medium text-muted-foreground">Notes</p>
                <p data-testid="detail-notes">{appointment.notes}</p>
              </div>
            )}
            {appointment.cancellationReason && (
              <div className="md:col-span-2">
                <p className="text-sm font-medium text-muted-foreground">Cancellation Reason</p>
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
              {transitions.map((t) => (
                <Button
                  key={t.action}
                  variant={t.variant ?? 'default'}
                  data-testid={`btn-action-${t.action.toLowerCase().replace('_', '-')}`}
                  onClick={() => setPendingAction(t)}
                >
                  {t.label}
                </Button>
              ))}
            </div>
          )}

          <div className="pt-4">
            <Button
              variant="outline"
              data-testid="btn-back"
              onClick={() => router.push('/appointments')}
            >
              Back to list
            </Button>
          </div>
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
              {pendingAction?.confirmTitle}
            </DialogTitle>
            <DialogDescription data-testid="confirm-dialog-description">
              {pendingAction?.confirmDescription}
            </DialogDescription>
          </DialogHeader>

          {pendingAction?.requiresReason && (
            <div className="space-y-2">
              <Label htmlFor="cancel-reason">Reason</Label>
              <Textarea
                id="cancel-reason"
                data-testid="input-cancel-reason"
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
                placeholder="Enter cancellation reason"
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
              Back
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
              {isProcessing ? 'Processing...' : 'Confirm'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}

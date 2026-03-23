'use client'

import { useState, useCallback } from 'react'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { dismissAlert } from '@/lib/api/health-alerts'
import type { HealthAlertDto } from '@/lib/api/health-alerts'
import { toast } from 'sonner'

interface DismissAlertDialogProps {
  alert: HealthAlertDto | null
  onConfirm: () => void
  onCancel: () => void
}

export function DismissAlertDialog({ alert, onConfirm, onCancel }: DismissAlertDialogProps) {
  const [reason, setReason] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = useCallback(async () => {
    if (!alert || !reason.trim()) return
    setIsSubmitting(true)
    try {
      await dismissAlert(alert.id, reason.trim())
      toast.success(`Alert dismissed for ${alert.patientName}`)
      setReason('')
      onConfirm()
    } catch {
      toast.error('Failed to dismiss alert')
    } finally {
      setIsSubmitting(false)
    }
  }, [alert, reason, onConfirm])

  const handleClose = useCallback(() => {
    setReason('')
    onCancel()
  }, [onCancel])

  return (
    <Dialog
      open={alert !== null}
      onOpenChange={(open) => { if (!open) handleClose() }}
    >
      <DialogContent data-testid="dismiss-alert-dialog">
        <DialogHeader>
          <DialogTitle>Dismiss Health Alert</DialogTitle>
          <DialogDescription>
            Dismiss the alert &quot;{alert?.title}&quot; for {alert?.patientName}.
            Please provide a reason for dismissing this alert.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-2">
          <Label htmlFor="dismiss-reason">Reason</Label>
          <Textarea
            id="dismiss-reason"
            data-testid="dismiss-reason-input"
            placeholder="e.g. Owner confirmed vaccination done at another clinic"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={3}
          />
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={handleClose}
            data-testid="dismiss-cancel-btn"
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleSubmit}
            disabled={!reason.trim() || isSubmitting}
            data-testid="dismiss-confirm-btn"
          >
            {isSubmitting ? 'Dismissing...' : 'Dismiss Alert'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

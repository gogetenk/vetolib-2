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
import { useTranslations } from 'next-intl'

interface DismissAlertDialogProps {
  alert: HealthAlertDto | null
  onConfirm: () => void
  onCancel: () => void
}

export function DismissAlertDialog({ alert, onConfirm, onCancel }: DismissAlertDialogProps) {
  const t = useTranslations('health_alerts.dismiss_dialog')
  const [reason, setReason] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = useCallback(async () => {
    if (!alert || !reason.trim()) return
    setIsSubmitting(true)
    try {
      await dismissAlert(alert.id, reason.trim())
      toast.success(t('success', { patientName: alert.patientName }))
      setReason('')
      onConfirm()
    } catch {
      toast.error(t('error'))
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
          <DialogTitle>{t('title')}</DialogTitle>
          <DialogDescription>
            {t('description', { title: alert?.title ?? '', patientName: alert?.patientName ?? '' })}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-2">
          <Label htmlFor="dismiss-reason">{t('reason_label')}</Label>
          <Textarea
            id="dismiss-reason"
            data-testid="dismiss-reason-input"
            placeholder={t('reason_placeholder')}
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
            {t('cancel')}
          </Button>
          <Button
            variant="destructive"
            onClick={handleSubmit}
            disabled={!reason.trim() || isSubmitting}
            data-testid="dismiss-confirm-btn"
          >
            {isSubmitting ? t('dismissing') : t('dismiss')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

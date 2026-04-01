'use client'

import { useState, useEffect, useCallback } from 'react'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { createWaitlistEntry } from '@/lib/api/waitlist'
import type { WaitlistPriority } from '@/lib/api/waitlist'
import { getPatients } from '@/lib/api/patients'
import type { PatientDto } from '@/lib/api/patients'
import { useTranslations } from 'next-intl'

interface AddWaitlistDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onAdded: () => void
}

const DAYS = ['Any', 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'] as const
const TIMES = ['Any', 'Morning', 'Afternoon', 'Evening'] as const
const PRIORITIES: WaitlistPriority[] = ['LOW', 'NORMAL', 'HIGH', 'URGENT']

export function AddWaitlistDialog({ open, onOpenChange, onAdded }: AddWaitlistDialogProps) {
  const t = useTranslations('waitlist.dialog')
  const tPriority = useTranslations('waitlist.priority')

  const [patients, setPatients] = useState<PatientDto[]>([])
  const [patientId, setPatientId] = useState('')
  const [reason, setReason] = useState('')
  const [preferredDay, setPreferredDay] = useState<string>('Any')
  const [preferredTime, setPreferredTime] = useState<string>('Any')
  const [priority, setPriority] = useState<WaitlistPriority>('NORMAL')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const loadPatients = useCallback(async () => {
    try {
      const result = await getPatients({ pageSize: 100 })
      setPatients(result.items)
    } catch {
      // Patients load failure is non-blocking — user can still type
    }
  }, [])

  useEffect(() => {
    if (open) {
      loadPatients()
    }
  }, [open, loadPatients])

  const resetForm = useCallback(() => {
    setPatientId('')
    setReason('')
    setPreferredDay('Any')
    setPreferredTime('Any')
    setPriority('NORMAL')
    setError(null)
  }, [])

  const handleSubmit = useCallback(async () => {
    if (!patientId || !reason.trim()) return
    setSubmitting(true)
    setError(null)
    try {
      await createWaitlistEntry({
        patientId,
        reason: reason.trim(),
        preferredDay,
        preferredTime,
        priority,
      })
      resetForm()
      onAdded()
    } catch {
      setError(t('submit_error'))
    } finally {
      setSubmitting(false)
    }
  }, [patientId, reason, preferredDay, preferredTime, priority, resetForm, onAdded, t])

  const handleOpenChange = useCallback((nextOpen: boolean) => {
    if (!nextOpen) {
      resetForm()
    }
    onOpenChange(nextOpen)
  }, [onOpenChange, resetForm])

  const canSubmit = patientId && reason.trim().length > 0

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent data-testid="waitlist-add-dialog" className="sm:max-w-[480px]">
        <DialogHeader>
          <DialogTitle data-testid="waitlist-dialog-title">{t('title')}</DialogTitle>
          <DialogDescription>{t('description')}</DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          {/* Patient selector */}
          <div className="grid gap-2">
            <Label htmlFor="waitlist-patient">{t('patient')}</Label>
            <Select value={patientId} onValueChange={(v) => setPatientId(v ?? '')}>
              <SelectTrigger id="waitlist-patient" data-testid="waitlist-patient-select">
                <SelectValue placeholder={t('patient_placeholder')} />
              </SelectTrigger>
              <SelectContent data-testid="waitlist-patient-options">
                {patients.map((p) => (
                  <SelectItem key={p.id} value={p.id} data-testid={`waitlist-patient-option-${p.id}`}>
                    {p.name} — {p.ownerName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Reason */}
          <div className="grid gap-2">
            <Label htmlFor="waitlist-reason">{t('reason')}</Label>
            <Input
              id="waitlist-reason"
              data-testid="waitlist-reason-input"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder={t('reason_placeholder')}
            />
          </div>

          {/* Preferred day */}
          <div className="grid gap-2">
            <Label htmlFor="waitlist-day">{t('preferred_day')}</Label>
            <Select value={preferredDay} onValueChange={(v) => setPreferredDay(v ?? 'Any')}>
              <SelectTrigger id="waitlist-day" data-testid="waitlist-day-select">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {DAYS.map((day) => (
                  <SelectItem key={day} value={day} data-testid={`waitlist-day-${day.toLowerCase()}`}>
                    {day}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Preferred time */}
          <div className="grid gap-2">
            <Label htmlFor="waitlist-time">{t('preferred_time')}</Label>
            <Select value={preferredTime} onValueChange={(v) => setPreferredTime(v ?? 'Any')}>
              <SelectTrigger id="waitlist-time" data-testid="waitlist-time-select">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {TIMES.map((time) => (
                  <SelectItem key={time} value={time} data-testid={`waitlist-time-${time.toLowerCase()}`}>
                    {time}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Priority */}
          <div className="grid gap-2">
            <Label htmlFor="waitlist-priority">{t('priority_label')}</Label>
            <Select value={priority} onValueChange={(v) => setPriority((v ?? 'NORMAL') as WaitlistPriority)}>
              <SelectTrigger id="waitlist-priority" data-testid="waitlist-priority-select">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {PRIORITIES.map((p) => (
                  <SelectItem key={p} value={p} data-testid={`waitlist-priority-option-${p.toLowerCase()}`}>
                    {tPriority(p)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {error && (
            <p className="text-sm text-destructive" data-testid="waitlist-dialog-error">
              {error}
            </p>
          )}
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            data-testid="waitlist-dialog-cancel"
            onClick={() => handleOpenChange(false)}
            disabled={submitting}
          >
            {t('cancel')}
          </Button>
          <Button
            data-testid="waitlist-dialog-submit"
            onClick={handleSubmit}
            disabled={!canSubmit || submitting}
          >
            {submitting ? t('submitting') : t('submit')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

'use client'

import { useState } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import { toast } from 'sonner'
import Link from 'next/link'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { CONSULTATION_COLORS } from './consultation-colors'
import { createAppointment } from '@/lib/api/appointments'
import type { VetDto, Species } from '@/lib/api/appointments'
import { ExternalLinkIcon } from 'lucide-react'

interface QuickAppointmentFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  date: Date
  time: string // HH:mm format
  vets: VetDto[]
  onCreated: () => void
}

const CONSULTATION_TYPES = Object.keys(CONSULTATION_COLORS)

function roundToNearest15(timeStr: string): string {
  const [h, m] = timeStr.split(':').map(Number)
  const rounded = Math.round(m / 15) * 15
  const finalM = rounded === 60 ? 0 : rounded
  const finalH = rounded === 60 ? h + 1 : h
  return `${String(finalH).padStart(2, '0')}:${String(finalM).padStart(2, '0')}`
}

export function QuickAppointmentForm({
  open,
  onOpenChange,
  date,
  time,
  vets,
  onCreated,
}: QuickAppointmentFormProps) {
  const locale = useLocale()
  const t = useTranslations('calendar.quickCreate')

  const roundedTime = roundToNearest15(time)

  const [patientName, setPatientName] = useState('')
  const [ownerName, setOwnerName] = useState('')
  const [consultationType, setConsultationType] = useState<string>('General Checkup')
  const [vetId, setVetId] = useState<string>(vets[0]?.id ?? '')
  const [reason, setReason] = useState('')
  const [editableTime, setEditableTime] = useState(roundedTime)
  const [submitting, setSubmitting] = useState(false)

  const dateLabel = new Intl.DateTimeFormat(locale, {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(date)

  const fullFormParams = new URLSearchParams({
    date: date.toISOString().split('T')[0],
    time: editableTime,
  })

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!patientName || !ownerName || !vetId) return

    setSubmitting(true)
    try {
      const [hours, minutes] = editableTime.split(':').map(Number)
      const scheduledAt = new Date(date)
      scheduledAt.setHours(hours, minutes, 0, 0)

      await createAppointment({
        patientName,
        species: 'Dog' as Species,
        ownerName,
        ownerPhone: '',
        vetId,
        scheduledAt: scheduledAt.toISOString(),
        reason,
      })

      toast.success(t('success', { name: patientName, time: editableTime }))
      onOpenChange(false)
      resetForm()
      onCreated()
    } catch {
      toast.error('Failed to create appointment')
    } finally {
      setSubmitting(false)
    }
  }

  function resetForm() {
    setPatientName('')
    setOwnerName('')
    setConsultationType('General Checkup')
    setReason('')
    setEditableTime(roundedTime)
  }

  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen) {
      resetForm()
    }
    onOpenChange(nextOpen)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="sm:max-w-md rounded-2xl"
        data-testid="quick-create-dialog"
      >
        <DialogHeader>
          <DialogTitle className="text-[18px] font-bold text-foreground">{t('title')}</DialogTitle>
          <DialogDescription className="text-[13px]">{dateLabel}</DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          {/* Time */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="quick-time" className="text-[13px] font-semibold text-foreground">{t('time')}</Label>
            <Input
              id="quick-time"
              type="time"
              value={editableTime}
              onChange={(e) => setEditableTime(e.target.value)}
              step={900}
              data-testid="quick-create-time-input"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
          </div>

          {/* Patient */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="quick-patient" className="text-[13px] font-semibold text-foreground">{t('patient')}</Label>
            <Input
              id="quick-patient"
              value={patientName}
              onChange={(e) => setPatientName(e.target.value)}
              placeholder={t('patient')}
              required
              data-testid="quick-create-patient-input"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
          </div>

          {/* Owner */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="quick-owner" className="text-[13px] font-semibold text-foreground">{t('owner')}</Label>
            <Input
              id="quick-owner"
              value={ownerName}
              onChange={(e) => setOwnerName(e.target.value)}
              placeholder={t('owner')}
              required
              data-testid="quick-create-owner-input"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
          </div>

          {/* Consultation type */}
          <div className="flex flex-col gap-1.5">
            <Label className="text-[13px] font-semibold text-foreground">{t('type')}</Label>
            <Select
              value={consultationType}
              onValueChange={(v) => v && setConsultationType(v)}
              data-testid="quick-create-type-select"
            >
              <SelectTrigger className="w-full rounded-xl border-border/80 text-base sm:text-[13px]" data-testid="quick-create-type-select">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {CONSULTATION_TYPES.map((type) => {
                  const colors = CONSULTATION_COLORS[type]
                  return (
                    <SelectItem key={type} value={type}>
                      <span className={`inline-block w-2 h-2 rounded-full ${colors.bg} ${colors.border} me-1.5`} />
                      {type}
                    </SelectItem>
                  )
                })}
              </SelectContent>
            </Select>
          </div>

          {/* Vet */}
          <div className="flex flex-col gap-1.5">
            <Label className="text-[13px] font-semibold text-foreground">{t('vet')}</Label>
            <Select
              value={vetId}
              onValueChange={(v) => v && setVetId(v)}
              data-testid="quick-create-vet-select"
            >
              <SelectTrigger className="w-full rounded-xl border-border/80 text-base sm:text-[13px]" data-testid="quick-create-vet-select">
                <SelectValue>
                  {vets.find((v) => v.id === vetId)?.name}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {vets.map((vet) => (
                  <SelectItem key={vet.id} value={vet.id}>
                    {vet.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Reason */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="quick-reason" className="text-[13px] font-semibold text-foreground">{t('reason')}</Label>
            <Textarea
              id="quick-reason"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder={t('reason')}
              rows={2}
              data-testid="quick-create-reason-input"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
          </div>

          <DialogFooter className="flex-row gap-2 sm:justify-between">
            <Link
              href={`/${locale}/appointments/new?${fullFormParams.toString()}`}
              className="inline-flex items-center gap-1 text-sm text-primary hover:underline"
              data-testid="quick-create-full-form-link"
            >
              <ExternalLinkIcon className="size-3.5" />
              {t('fullForm')}
            </Link>
            <div className="flex gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => handleOpenChange(false)}
                data-testid="quick-create-cancel-btn"
                className="rounded-xl font-semibold border-border/80 hover:bg-muted"
              >
                {t('cancel')}
              </Button>
              <Button
                type="submit"
                disabled={submitting || !patientName || !ownerName}
                data-testid="quick-create-submit-btn"
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                {t('submit')}
              </Button>
            </div>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'

interface AddWeightDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSubmit: (data: {
    weightKg: number
    recordedAt: string
    note?: string | null
  }) => Promise<void>
}

function todayDateString(): string {
  const now = new Date()
  // Format as YYYY-MM-DD
  const year = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export function AddWeightDialog({
  open,
  onOpenChange,
  onSubmit,
}: AddWeightDialogProps) {
  const t = useTranslations('patients.detail.weight')
  const [weightStr, setWeightStr] = useState('')
  const [date, setDate] = useState(todayDateString())
  const [note, setNote] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {}
    const weightNum = parseFloat(weightStr)

    if (!weightStr.trim() || isNaN(weightNum)) {
      newErrors.weight = t('validation.weight_required')
    } else if (weightNum <= 0) {
      newErrors.weight = t('validation.weight_positive')
    } else if (weightNum > 10000) {
      newErrors.weight = t('validation.weight_max')
    }

    if (!date) {
      newErrors.date = t('validation.date_required')
    }

    if (note.length > 500) {
      newErrors.note = t('validation.note_max')
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!validate()) return

    setIsSubmitting(true)
    try {
      await onSubmit({
        weightKg: parseFloat(weightStr),
        recordedAt: new Date(date).toISOString(),
        note: note.trim() || null,
      })
      // Reset form on success
      setWeightStr('')
      setDate(todayDateString())
      setNote('')
      setErrors({})
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      // Reset form when closing
      setWeightStr('')
      setDate(todayDateString())
      setNote('')
      setErrors({})
    }
    onOpenChange(newOpen)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{t('add_weight')}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Weight input */}
          <div className="space-y-1.5">
            <Label htmlFor="weight-input">{t('form.weight_label')}</Label>
            <Input
              id="weight-input"
              type="number"
              step="0.1"
              min="0.1"
              max="10000"
              placeholder={t('form.weight_placeholder')}
              value={weightStr}
              onChange={(e) => {
                setWeightStr(e.target.value)
                if (errors.weight) {
                  setErrors((prev) => {
                    const next = { ...prev }
                    delete next.weight
                    return next
                  })
                }
              }}
              data-testid="weight-input"
              aria-invalid={!!errors.weight}
            />
            {errors.weight && (
              <p className="text-[12px] text-destructive" data-testid="weight-input-error">
                {errors.weight}
              </p>
            )}
          </div>

          {/* Date input */}
          <div className="space-y-1.5">
            <Label htmlFor="weight-date-input">{t('form.date_label')}</Label>
            <Input
              id="weight-date-input"
              type="date"
              value={date}
              onChange={(e) => {
                setDate(e.target.value)
                if (errors.date) {
                  setErrors((prev) => {
                    const next = { ...prev }
                    delete next.date
                    return next
                  })
                }
              }}
              data-testid="weight-date-input"
              aria-invalid={!!errors.date}
            />
            {errors.date && (
              <p className="text-[12px] text-destructive" data-testid="weight-date-error">
                {errors.date}
              </p>
            )}
          </div>

          {/* Note textarea */}
          <div className="space-y-1.5">
            <Label htmlFor="weight-note-input">
              {t('form.note_label')}{' '}
              <span className="text-muted-foreground font-normal">
                ({t('form.optional')})
              </span>
            </Label>
            <Textarea
              id="weight-note-input"
              placeholder={t('form.note_placeholder')}
              value={note}
              onChange={(e) => {
                setNote(e.target.value)
                if (errors.note) {
                  setErrors((prev) => {
                    const next = { ...prev }
                    delete next.note
                    return next
                  })
                }
              }}
              maxLength={500}
              rows={3}
              data-testid="weight-note-input"
              aria-invalid={!!errors.note}
            />
            <div className="flex justify-between">
              {errors.note ? (
                <p className="text-[12px] text-destructive" data-testid="weight-note-error">
                  {errors.note}
                </p>
              ) : (
                <span />
              )}
              <p className="text-[11px] text-muted-foreground">
                {note.length}/500
              </p>
            </div>
          </div>

          <DialogFooter>
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="weight-submit-button"
              className="w-full sm:w-auto"
            >
              {isSubmitting ? t('form.submitting') : t('form.submit')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

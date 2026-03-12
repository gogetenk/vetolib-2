'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
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
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { createAppointment, getVets } from '@/lib/api/appointments'
import type { VetDto, Species } from '@/lib/api/appointments'
import { ApiError } from '@/lib/api/client'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'
import { useTranslations } from 'next-intl'

const SPECIES_OPTIONS: Species[] = ['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Exotic']

const TIME_SLOTS: string[] = []
for (let h = 8; h <= 18; h++) {
  TIME_SLOTS.push(`${String(h).padStart(2, '0')}:00`)
  TIME_SLOTS.push(`${String(h).padStart(2, '0')}:30`)
}
TIME_SLOTS.push('19:00')

const schema = z.object({
  patientName: z.string().min(1, 'Patient name is required'),
  species: z.enum(['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Exotic'] as [Species, ...Species[]]),
  ownerName: z.string().min(1, 'Owner name is required'),
  ownerPhone: z.string().min(1, 'Owner phone is required'),
  vetId: z.string().min(1, 'Vet is required'),
  date: z.string().min(1, 'Date is required'),
  time: z.string().min(1, 'Time is required'),
  reason: z.string().min(1, 'Reason is required'),
  notes: z.string().optional(),
})

type FormValues = z.infer<typeof schema>

export function AppointmentForm() {
  const router = useRouter()
  const t = useTranslations('appointments.form')
  const [vets, setVets] = useState<VetDto[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
  })

  useEffect(() => {
    trackEvent(AnalyticsEvents.APPOINTMENT_FORM_OPENED)
    getVets().then(setVets).catch(() => {
      toast.error(t('toast.load_vets_failed'))
    })
  }, [])

  const onSubmit = async (values: FormValues) => {
    setIsSubmitting(true)
    try {
      const scheduledAt = new Date(`${values.date}T${values.time}:00`).toISOString()
      await createAppointment({
        patientName: values.patientName,
        species: values.species,
        ownerName: values.ownerName,
        ownerPhone: values.ownerPhone,
        vetId: values.vetId,
        scheduledAt,
        reason: values.reason,
        notes: values.notes,
      })
      trackEvent(AnalyticsEvents.APPOINTMENT_CREATED, {
        species: values.species,
        has_notes: String(Boolean(values.notes)),
      })
      toast.success(t('toast.created'))
      router.push('/appointments')
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        const body = err.body as { title?: string }
        toast.error(body?.title ?? t('toast.slot_unavailable'))
      } else {
        toast.error(t('toast.failed'))
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t('title')}</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="space-y-4"
          data-testid="appointment-form"
        >
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {/* Patient Name */}
            <div className="space-y-1">
              <Label htmlFor="patientName">{t('patient_name')}</Label>
              <Input
                id="patientName"
                data-testid="input-patient-name"
                {...register('patientName')}
                placeholder={t('patient_name_placeholder')}
              />
              {errors.patientName && (
                <p className="text-sm text-destructive" data-testid="error-patient-name">
                  {errors.patientName.message}
                </p>
              )}
            </div>

            {/* Species */}
            <div className="space-y-1">
              <Label htmlFor="species">{t('species')}</Label>
              <Select
                onValueChange={(val) => setValue('species', val as Species)}
                data-testid="select-species"
              >
                <SelectTrigger data-testid="select-species-trigger">
                  <SelectValue placeholder={t('select_species')} />
                </SelectTrigger>
                <SelectContent>
                  {SPECIES_OPTIONS.map((s) => (
                    <SelectItem key={s} value={s} data-testid={`species-option-${s.toLowerCase()}`}>
                      {s}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.species && (
                <p className="text-sm text-destructive" data-testid="error-species">
                  {errors.species.message}
                </p>
              )}
            </div>

            {/* Owner Name */}
            <div className="space-y-1">
              <Label htmlFor="ownerName">{t('owner_name')}</Label>
              <Input
                id="ownerName"
                data-testid="input-owner-name"
                {...register('ownerName')}
                placeholder={t('owner_name_placeholder')}
              />
              {errors.ownerName && (
                <p className="text-sm text-destructive" data-testid="error-owner-name">
                  {errors.ownerName.message}
                </p>
              )}
            </div>

            {/* Owner Phone */}
            <div className="space-y-1">
              <Label htmlFor="ownerPhone">{t('owner_phone')}</Label>
              <Input
                id="ownerPhone"
                data-testid="input-owner-phone"
                {...register('ownerPhone')}
                placeholder={t('owner_phone_placeholder')}
              />
              {errors.ownerPhone && (
                <p className="text-sm text-destructive" data-testid="error-owner-phone">
                  {errors.ownerPhone.message}
                </p>
              )}
            </div>

            {/* Vet */}
            <div className="space-y-1">
              <Label htmlFor="vetId">{t('vet')}</Label>
              <Select
                onValueChange={(val) => setValue('vetId', val as string)}
                data-testid="select-vet"
              >
                <SelectTrigger data-testid="select-vet-trigger">
                  <SelectValue placeholder={t('select_vet')} />
                </SelectTrigger>
                <SelectContent>
                  {vets.map((v) => (
                    <SelectItem key={v.id} value={v.id} data-testid={`vet-option-${v.id}`}>
                      {v.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.vetId && (
                <p className="text-sm text-destructive" data-testid="error-vet">
                  {errors.vetId.message}
                </p>
              )}
            </div>

            {/* Date */}
            <div className="space-y-1">
              <Label htmlFor="date">{t('date')}</Label>
              <Input
                id="date"
                type="date"
                data-testid="input-date"
                {...register('date')}
                min={new Date().toISOString().split('T')[0]}
              />
              {errors.date && (
                <p className="text-sm text-destructive" data-testid="error-date">
                  {errors.date.message}
                </p>
              )}
            </div>

            {/* Time */}
            <div className="space-y-1">
              <Label htmlFor="time">{t('time_slot')}</Label>
              <Select
                onValueChange={(val) => setValue('time', val as string)}
                data-testid="select-time"
              >
                <SelectTrigger data-testid="select-time-trigger">
                  <SelectValue placeholder={t('select_time')} />
                </SelectTrigger>
                <SelectContent>
                  {TIME_SLOTS.map((t) => (
                    <SelectItem key={t} value={t} data-testid={`time-option-${t.replace(':', '')}`}>
                      {t}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.time && (
                <p className="text-sm text-destructive" data-testid="error-time">
                  {errors.time.message}
                </p>
              )}
            </div>
          </div>

          {/* Reason */}
          <div className="space-y-1">
            <Label htmlFor="reason">{t('reason')}</Label>
            <Textarea
              id="reason"
              data-testid="textarea-reason"
              {...register('reason')}
              placeholder={t('reason_placeholder')}
              rows={3}
            />
            {errors.reason && (
              <p className="text-sm text-destructive" data-testid="error-reason">
                {errors.reason.message}
              </p>
            )}
          </div>

          {/* Notes */}
          <div className="space-y-1">
            <Label htmlFor="notes">{t('notes')}</Label>
            <Textarea
              id="notes"
              data-testid="textarea-notes"
              {...register('notes')}
              placeholder={t('notes_placeholder')}
              rows={2}
            />
          </div>

          <div className="flex gap-3 justify-end">
            <Button
              type="button"
              variant="outline"
              data-testid="btn-cancel-form"
              onClick={() => router.push('/appointments')}
            >
              {t('cancel')}
            </Button>
            <Button
              type="submit"
              data-testid="btn-save"
              disabled={isSubmitting}
            >
              {isSubmitting ? t('saving') : t('save')}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

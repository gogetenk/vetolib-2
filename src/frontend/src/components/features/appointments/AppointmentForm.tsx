'use client'

import { useState, useEffect, useRef } from 'react'
import { useFormShake } from '@/hooks/use-form-shake'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Loader2 } from 'lucide-react'
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
import { useAhaMoment } from '@/hooks/use-aha-moment'
import { useTranslations } from 'next-intl'

const SPECIES_OPTIONS: Species[] = ['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Exotic', 'Falcon', 'Reptile']

const TIME_SLOTS: string[] = []
for (let h = 8; h <= 18; h++) {
  TIME_SLOTS.push(`${String(h).padStart(2, '0')}:00`)
  TIME_SLOTS.push(`${String(h).padStart(2, '0')}:30`)
}
TIME_SLOTS.push('19:00')

const schema = z.object({
  patientName: z.string().min(1, 'Patient name is required'),
  species: z.enum(['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Exotic', 'Falcon', 'Reptile'] as [Species, ...Species[]], { message: 'Please select a species' }),
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
  const { triggerAha } = useAhaMoment()
  const [vets, setVets] = useState<VetDto[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)
  const { shakeForm: hasShake, triggerShake } = useFormShake()
  const formRef = useRef<HTMLFormElement>(null)

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
  })

  const selectedVetId = watch('vetId')
  const selectedVetName = vets.find((v) => v.id === selectedVetId)?.name

  // Shake form on validation errors
  const errorCount = Object.keys(errors).length
  useEffect(() => {
    if (errorCount > 0) {
      triggerShake()
    }
  }, [errorCount, triggerShake])

  useEffect(() => {
    trackEvent(AnalyticsEvents.APPOINTMENT_FORM_OPENED)
    getVets().then(setVets).catch(() => {
      toast.error(t('toast.load_vets_failed'))
    })
  }, [t])

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
      triggerAha('first_appointment')
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
    <Card className="border-border/80 shadow-sm">
      <CardHeader>
        <CardTitle className="text-[18px] font-bold text-foreground">{t('title')}</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          ref={formRef}
          onSubmit={handleSubmit(onSubmit)}
          className={`space-y-4 ${hasShake ? 'animate-shake' : ''}`}
          data-testid="appointment-form"
        >
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {/* Patient Name */}
            <div className="space-y-1">
              <Label htmlFor="patientName" className="text-[13px] font-semibold text-foreground">{t('patient_name')} <span className="text-destructive">*</span></Label>
              <Input
                id="patientName"
                data-testid="input-patient-name"
                {...register('patientName')}
                placeholder={t('patient_name_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              />
              {errors.patientName && (
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-patient-name">
                  {errors.patientName.message}
                </p>
              )}
            </div>

            {/* Species */}
            <div className="space-y-1">
              <Label htmlFor="species" className="text-[13px] font-semibold text-foreground">{t('species')} <span className="text-destructive">*</span></Label>
              <Select
                onValueChange={(val) => setValue('species', val as Species)}
                data-testid="select-species"
              >
                <SelectTrigger className="rounded-xl border-border/80 text-[13px]" data-testid="select-species-trigger">
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
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-species">
                  {errors.species.message}
                </p>
              )}
            </div>

            {/* Owner Name */}
            <div className="space-y-1">
              <Label htmlFor="ownerName" className="text-[13px] font-semibold text-foreground">{t('owner_name')} <span className="text-destructive">*</span></Label>
              <Input
                id="ownerName"
                data-testid="input-owner-name"
                {...register('ownerName')}
                placeholder={t('owner_name_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              />
              {errors.ownerName && (
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-owner-name">
                  {errors.ownerName.message}
                </p>
              )}
            </div>

            {/* Owner Phone */}
            <div className="space-y-1">
              <Label htmlFor="ownerPhone" className="text-[13px] font-semibold text-foreground">{t('owner_phone')} <span className="text-destructive">*</span></Label>
              <Input
                id="ownerPhone"
                data-testid="input-owner-phone"
                {...register('ownerPhone')}
                placeholder={t('owner_phone_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              />
              {errors.ownerPhone && (
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-owner-phone">
                  {errors.ownerPhone.message}
                </p>
              )}
            </div>

            {/* Vet */}
            <div className="space-y-1">
              <Label htmlFor="vetId" className="text-[13px] font-semibold text-foreground">{t('vet')} <span className="text-destructive">*</span></Label>
              <Select
                onValueChange={(val) => setValue('vetId', val as string)}
                data-testid="select-vet"
              >
                <SelectTrigger className="rounded-xl border-border/80 text-[13px]" data-testid="select-vet-trigger">
                  <SelectValue placeholder={t('select_vet')}>
                    {selectedVetName}
                  </SelectValue>
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
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-vet">
                  {errors.vetId.message}
                </p>
              )}
            </div>

            {/* Date */}
            <div className="space-y-1">
              <Label htmlFor="date" className="text-[13px] font-semibold text-foreground">{t('date')} <span className="text-destructive">*</span></Label>
              <Input
                id="date"
                type="date"
                data-testid="input-date"
                {...register('date')}
                min={new Date().toISOString().split('T')[0]}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              />
              {errors.date && (
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-date">
                  {errors.date.message}
                </p>
              )}
            </div>

            {/* Time */}
            <div className="space-y-1">
              <Label htmlFor="time" className="text-[13px] font-semibold text-foreground">{t('time_slot')} <span className="text-destructive">*</span></Label>
              <Select
                onValueChange={(val) => setValue('time', val as string)}
                data-testid="select-time"
              >
                <SelectTrigger className="rounded-xl border-border/80 text-[13px]" data-testid="select-time-trigger">
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
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-time">
                  {errors.time.message}
                </p>
              )}
            </div>
          </div>

          {/* Reason */}
          <div className="space-y-1">
            <Label htmlFor="reason" className="text-[13px] font-semibold text-foreground">{t('reason')} <span className="text-destructive">*</span></Label>
            <Textarea
              id="reason"
              data-testid="textarea-reason"
              {...register('reason')}
              placeholder={t('reason_placeholder')}
              rows={3}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
            {errors.reason && (
              <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-reason">
                {errors.reason.message}
              </p>
            )}
          </div>

          {/* Notes */}
          <div className="space-y-1">
            <Label htmlFor="notes" className="text-[13px] font-semibold text-foreground">{t('notes')}</Label>
            <Textarea
              id="notes"
              data-testid="textarea-notes"
              {...register('notes')}
              placeholder={t('notes_placeholder')}
              rows={2}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
          </div>

          <div className="sticky bottom-0 bg-white py-3 border-t border-border/50 flex gap-3 justify-end -mx-6 px-6">
            <Button
              type="button"
              variant="outline"
              data-testid="btn-cancel-form"
              onClick={() => router.push('/appointments')}
              className="rounded-xl font-semibold border-border/80 hover:bg-muted"
            >
              {t('cancel')}
            </Button>
            <Button
              type="submit"
              data-testid="btn-save"
              disabled={isSubmitting}
              className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
            >
              {isSubmitting && <Loader2 className="h-4 w-4 mr-1.5 animate-spin" aria-hidden />}
              {isSubmitting ? t('saving') : t('save')}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

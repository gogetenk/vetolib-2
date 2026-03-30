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
import type { VetDto } from '@/lib/api/appointments'
import { ApiError } from '@/lib/api/client'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'
import { useAhaMoment } from '@/hooks/use-aha-moment'
import { useTranslations } from 'next-intl'

const TIME_SLOTS: string[] = []
for (let h = 8; h <= 18; h++) {
  TIME_SLOTS.push(`${String(h).padStart(2, '0')}:00`)
  TIME_SLOTS.push(`${String(h).padStart(2, '0')}:30`)
}
TIME_SLOTS.push('19:00')

const schema = z.object({
  animalId: z.string().min(1, 'Animal is required'),
  veterinarianId: z.string().min(1, 'Vet is required'),
  date: z.string().min(1, 'Date is required'),
  time: z.string().min(1, 'Time is required'),
  durationMinutes: z.coerce.number().min(5, 'Duration must be at least 5 minutes'),
  reason: z.string().optional(),
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

  const selectedVetId = watch('veterinarianId')
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
      await createAppointment({
        animalId: values.animalId,
        veterinarianId: values.veterinarianId,
        date: values.date,
        startTime: `${values.time}:00`,
        durationMinutes: values.durationMinutes,
        reason: values.reason,
        source: 'Staff',
      })
      trackEvent(AnalyticsEvents.APPOINTMENT_CREATED, {})
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
            {/* Animal ID */}
            <div className="space-y-1">
              <Label htmlFor="animalId" className="text-[13px] font-semibold text-foreground">{t('animal')} <span className="text-destructive">*</span></Label>
              <Input
                id="animalId"
                data-testid="input-animal-id"
                {...register('animalId')}
                placeholder={t('animal_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              />
              {errors.animalId && (
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-animal-id">
                  {errors.animalId.message}
                </p>
              )}
            </div>

            {/* Vet */}
            <div className="space-y-1">
              <Label htmlFor="veterinarianId" className="text-[13px] font-semibold text-foreground">{t('vet')} <span className="text-destructive">*</span></Label>
              <Select
                onValueChange={(val) => setValue('veterinarianId', val as string)}
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
              {errors.veterinarianId && (
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-vet">
                  {errors.veterinarianId.message}
                </p>
              )}
            </div>

            {/* Duration */}
            <div className="space-y-1">
              <Label htmlFor="durationMinutes" className="text-[13px] font-semibold text-foreground">{t('duration')} <span className="text-destructive">*</span></Label>
              <Input
                id="durationMinutes"
                type="number"
                data-testid="input-duration"
                {...register('durationMinutes')}
                defaultValue={30}
                min={5}
                step={5}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              />
              {errors.durationMinutes && (
                <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-duration">
                  {errors.durationMinutes.message}
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
            <Label htmlFor="reason" className="text-[13px] font-semibold text-foreground">{t('reason')}</Label>
            <Textarea
              id="reason"
              data-testid="textarea-reason"
              {...register('reason')}
              placeholder={t('reason_placeholder')}
              rows={3}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
            />
          </div>

          <div className="sticky bottom-0 bg-card py-3 border-t border-border/50 flex gap-3 justify-end -mx-6 px-6">
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
              {isSubmitting && <Loader2 className="h-4 w-4 me-1.5 animate-spin" aria-hidden />}
              {isSubmitting ? t('saving') : t('save')}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

'use client'

import { useState, useMemo, useRef, useEffect } from 'react'
import { useFormShake } from '@/hooks/use-form-shake'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { useTranslations } from 'next-intl'
import { Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { createPatient, updatePatient } from '@/lib/api/patients'
import type { PatientDto, Species } from '@/lib/api/patients'
import { SPECIES_LABELS, ALL_SPECIES } from './SpeciesIcon'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'

function createPatientSchema(t: (key: string) => string) {
  return z.object({
    name: z.string().min(1, t('errors.name_required')),
    species: z.enum(['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Camel', 'Exotic'] as [Species, ...Species[]], { message: t('errors.species_required') }),
    breed: z.string().optional(),
    dateOfBirth: z.string().min(1, t('errors.date_of_birth_required')).refine(
      (val) => {
        const date = new Date(val)
        return !isNaN(date.getTime()) && date <= new Date()
      },
      { message: t('errors.date_of_birth_future') }
    ),
    gender: z.enum(['Male', 'Female', 'Unknown'] as ['Male', 'Female', 'Unknown']),
    weightKg: z.preprocess(
      (val) => (val === '' || val === null || val === undefined ? null : Number(val)),
      z.number().positive(t('errors.weight_positive')).nullable()
    ),
    ownerName: z.string().min(1, t('errors.owner_name_required')),
    ownerPhone: z.string().min(1, t('errors.owner_phone_required')).regex(
      /^\+971\s\d{2}\s\d{3}\s\d{4}$/,
      t('errors.owner_phone_format')
    ),
    ownerEmail: z.string().email(t('errors.owner_email_invalid')).optional().or(z.literal('')),
  })
}

type PatientFormValues = z.infer<ReturnType<typeof createPatientSchema>>

interface PatientFormProps {
  /** When provided, the form is in edit mode */
  patient?: PatientDto
  onSuccess?: (patient: PatientDto) => void
}

export function PatientForm({ patient, onSuccess }: PatientFormProps) {
  const router = useRouter()
  const t = useTranslations('patients.form')
  const [serverError, setServerError] = useState<string | null>(null)
  const formRef = useRef<HTMLFormElement>(null)
  const patientSchema = useMemo(() => createPatientSchema(t), [t])

  const isEdit = !!patient

  const { shakeForm: hasShake, triggerShake } = useFormShake()

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<PatientFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(patientSchema) as any,
    defaultValues: patient
      ? {
          name: patient.name,
          species: patient.species,
          breed: patient.breed,
          dateOfBirth: patient.dateOfBirth,
          gender: patient.gender,
          weightKg: patient.weightKg,
          ownerName: patient.ownerName,
          ownerPhone: patient.ownerPhone,
          ownerEmail: patient.ownerEmail,
        }
      : {
          gender: 'Unknown',
          weightKg: null,
        },
  })

  // Shake form on validation errors
  const errorCount = Object.keys(errors).length
  useEffect(() => {
    if (errorCount > 0) {
      triggerShake()
    }
  }, [errorCount, triggerShake])

  // eslint-disable-next-line react-hooks/incompatible-library
  const selectedSpecies = watch('species')
  const selectedGender = watch('gender')

  const onSubmit = async (data: PatientFormValues) => {
    setServerError(null)
    try {
      const payload = {
        name: data.name,
        species: data.species,
        breed: data.breed || undefined,
        dateOfBirth: data.dateOfBirth,
        gender: data.gender,
        weightKg: data.weightKg ?? null,
        ownerName: data.ownerName,
        ownerPhone: data.ownerPhone,
        ownerEmail: data.ownerEmail || undefined,
      }

      let result: PatientDto
      if (isEdit && patient) {
        result = await updatePatient(patient.id, payload)
        toast.success(t('toast_updated'))
      } else {
        result = await createPatient(payload)
        trackEvent(AnalyticsEvents.PATIENT_CREATED, {
          species: data.species,
          has_microchip: "false",
        })
        toast.success(t('toast_created'))
      }

      if (onSuccess) {
        onSuccess(result)
      } else {
        router.push(`/patients/${result.id}`)
      }
    } catch {
      setServerError(t('error_save_failed'))
    }
  }

  const today = new Date().toISOString().split('T')[0]

  return (
    <Card className="border-border/80 shadow-sm" data-testid="patient-form">
      <CardHeader>
        <CardTitle className="text-[18px] font-bold text-foreground">
          {isEdit ? t('edit_title', { name: patient!.name }) : t('title')}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <form
          ref={formRef}
          onSubmit={handleSubmit(onSubmit)}
          className={`space-y-6 ${hasShake ? 'animate-shake' : ''}`}
          data-testid="patient-form-body"
          noValidate
        >
          {/* Animal Name */}
          <div className="space-y-1">
            <Label htmlFor="name" className="text-[13px] font-semibold text-foreground">
              {t('patient_name')} <span className="text-destructive">*</span>
            </Label>
            <Input
              id="name"
              data-testid="input-patient-name"
              placeholder={t('patient_name_placeholder')}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              {...register('name')}
            />
            {errors.name && (
              <p className="text-xs text-destructive animate-slide-up-fade" data-testid="error-patient-name">
                {errors.name.message}
              </p>
            )}
          </div>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {/* Species */}
            <div className="space-y-1">
              <Label htmlFor="species" className="text-[13px] font-semibold text-foreground">
                {t('species')} <span className="text-destructive">*</span>
              </Label>
              <Select
                value={selectedSpecies}
                onValueChange={(val) => setValue('species', val as Species, { shouldValidate: true })}
                data-testid="select-species"
              >
                <SelectTrigger className="rounded-xl border-border/80 text-[13px]" data-testid="select-species-trigger">
                  <SelectValue placeholder={t('select_species')}>
                    {selectedSpecies ? SPECIES_LABELS[selectedSpecies] : null}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {ALL_SPECIES.map((s) => (
                    <SelectItem key={s} value={s} data-testid={`species-option-${s.toLowerCase()}`}>
                      {SPECIES_LABELS[s]}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.species && (
                <p className="text-xs text-destructive animate-slide-up-fade" data-testid="error-species">
                  {errors.species.message}
                </p>
              )}
            </div>

            {/* Breed */}
            <div className="space-y-1">
              <Label htmlFor="breed" className="text-[13px] font-semibold text-foreground">{t('breed')}</Label>
              <Input
                id="breed"
                data-testid="input-breed"
                placeholder={t('breed_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                {...register('breed')}
              />
            </div>

            {/* Date of Birth */}
            <div className="space-y-1">
              <Label htmlFor="dateOfBirth" className="text-[13px] font-semibold text-foreground">
                {t('date_of_birth')} <span className="text-destructive">*</span>
              </Label>
              <Input
                id="dateOfBirth"
                type="date"
                data-testid="input-date-of-birth"
                max={today}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                {...register('dateOfBirth')}
              />
              {errors.dateOfBirth && (
                <p className="text-xs text-destructive animate-slide-up-fade" data-testid="error-date-of-birth">
                  {errors.dateOfBirth.message}
                </p>
              )}
            </div>

            {/* Gender */}
            <div className="space-y-1">
              <Label htmlFor="gender" className="text-[13px] font-semibold text-foreground">{t('gender')}</Label>
              <Select
                value={selectedGender}
                onValueChange={(val) =>
                  setValue('gender', val as 'Male' | 'Female' | 'Unknown', { shouldValidate: true })
                }
                data-testid="select-gender"
              >
                <SelectTrigger className="rounded-xl border-border/80 text-[13px]" data-testid="select-gender-trigger">
                  <SelectValue placeholder={t('select_gender')}>
                    {selectedGender ?? null}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Male" data-testid="gender-option-male">{t('gender_male')}</SelectItem>
                  <SelectItem value="Female" data-testid="gender-option-female">{t('gender_female')}</SelectItem>
                  <SelectItem value="Unknown" data-testid="gender-option-unknown">{t('gender_unknown')}</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Weight */}
            <div className="space-y-1">
              <Label htmlFor="weightKg" className="text-[13px] font-semibold text-foreground">
                {t('weight')}{' '}
                <span className="text-muted-foreground font-normal">{t('weight_optional')}</span>
              </Label>
              <Input
                id="weightKg"
                type="number"
                step="0.1"
                min="0"
                data-testid="patient-weight-input"
                placeholder={t('weight_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                {...register('weightKg')}
              />
              <p className="text-xs text-muted-foreground">{t('weight_hint')}</p>
              {errors.weightKg && (
                <p className="text-xs text-destructive animate-slide-up-fade" data-testid="error-weight">
                  {errors.weightKg.message}
                </p>
              )}
            </div>
          </div>

          <hr className="border-border/50" />
          <p className="text-[13px] font-bold text-foreground">{t('owner_information')}</p>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {/* Owner Name */}
            <div className="space-y-1 md:col-span-2">
              <Label htmlFor="ownerName" className="text-[13px] font-semibold text-foreground">
                {t('owner_name')} <span className="text-destructive">*</span>
              </Label>
              <Input
                id="ownerName"
                data-testid="input-owner-name"
                placeholder={t('owner_name_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                {...register('ownerName')}
              />
              {errors.ownerName && (
                <p className="text-xs text-destructive animate-slide-up-fade" data-testid="error-owner-name">
                  {errors.ownerName.message}
                </p>
              )}
            </div>

            {/* Owner Phone */}
            <div className="space-y-1">
              <Label htmlFor="ownerPhone" className="text-[13px] font-semibold text-foreground">
                {t('owner_phone')} <span className="text-destructive">*</span>
              </Label>
              <Input
                id="ownerPhone"
                type="tel"
                data-testid="input-owner-phone"
                placeholder={t('owner_phone_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                {...register('ownerPhone')}
              />
              {errors.ownerPhone && (
                <p className="text-xs text-destructive animate-slide-up-fade" data-testid="error-owner-phone">
                  {errors.ownerPhone.message}
                </p>
              )}
            </div>

            {/* Owner Email */}
            <div className="space-y-1">
              <Label htmlFor="ownerEmail" className="text-[13px] font-semibold text-foreground">
                {t('owner_email')} <span className="text-muted-foreground">{t('owner_email_optional')}</span>
              </Label>
              <Input
                id="ownerEmail"
                type="email"
                data-testid="input-owner-email"
                placeholder={t('owner_email_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                {...register('ownerEmail')}
              />
              {errors.ownerEmail && (
                <p className="text-xs text-destructive animate-slide-up-fade" data-testid="error-owner-email">
                  {errors.ownerEmail.message}
                </p>
              )}
            </div>
          </div>

          {serverError && (
            <p className="text-sm text-destructive animate-slide-up-fade" data-testid="error-server" role="alert">
              {serverError}
            </p>
          )}

          <div className="sticky bottom-0 bg-white py-3 border-t border-border/50 flex gap-3 justify-end -mx-6 px-6">
            <Button
              type="button"
              variant="outline"
              data-testid="btn-cancel-patient"
              onClick={() => {
                if (patient) {
                  router.push(`/patients/${patient.id}`)
                } else {
                  router.push('/patients')
                }
              }}
              className="rounded-xl font-semibold border-border/80 hover:bg-muted"
            >
              {t('cancel')}
            </Button>
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="btn-save-patient"
              className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
            >
              {isSubmitting && <Loader2 className="h-4 w-4 mr-1.5 animate-spin" aria-hidden />}
              {isSubmitting ? t('saving') : t('save_patient')}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

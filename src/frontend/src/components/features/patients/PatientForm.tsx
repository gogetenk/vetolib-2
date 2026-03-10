'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
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

const patientSchema = z.object({
  name: z.string().min(1, 'Animal name is required'),
  species: z.enum(['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Camel', 'Exotic'] as [Species, ...Species[]]),
  breed: z.string().optional(),
  dateOfBirth: z.string().min(1, 'Date of birth is required').refine(
    (val) => {
      const date = new Date(val)
      return !isNaN(date.getTime()) && date <= new Date()
    },
    { message: 'Date of birth cannot be in the future' }
  ),
  gender: z.enum(['Male', 'Female', 'Unknown'] as ['Male', 'Female', 'Unknown']),
  weightKg: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? null : Number(val)),
    z.number().positive('Weight must be greater than 0').nullable()
  ),
  ownerName: z.string().min(1, 'Owner name is required'),
  ownerPhone: z.string().min(1, 'Owner phone is required').regex(
    /^\+971\s\d{2}\s\d{3}\s\d{4}$/,
    'Phone must follow UAE format: +971 XX XXX XXXX'
  ),
  ownerEmail: z.string().email('Invalid email').optional().or(z.literal('')),
})

type PatientFormValues = z.infer<typeof patientSchema>

interface PatientFormProps {
  /** When provided, the form is in edit mode */
  patient?: PatientDto
  onSuccess?: (patient: PatientDto) => void
}

export function PatientForm({ patient, onSuccess }: PatientFormProps) {
  const router = useRouter()
  const [serverError, setServerError] = useState<string | null>(null)

  const isEdit = !!patient

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
        toast.success('Patient updated successfully')
      } else {
        result = await createPatient(payload)
        trackEvent(AnalyticsEvents.PATIENT_CREATED, {
          species: data.species,
          has_microchip: "false",
        })
        toast.success('Patient created successfully')
      }

      if (onSuccess) {
        onSuccess(result)
      } else {
        router.push(`/patients/${result.id}`)
      }
    } catch {
      setServerError('Failed to save patient. Please try again.')
    }
  }

  const today = new Date().toISOString().split('T')[0]

  return (
    <Card data-testid="patient-form">
      <CardHeader>
        <CardTitle>{isEdit ? `Edit Patient — ${patient!.name}` : 'New Patient'}</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="space-y-6"
          data-testid="patient-form-body"
          noValidate
        >
          {/* Animal Name */}
          <div className="space-y-1">
            <Label htmlFor="name">
              Animal Name <span className="text-destructive">*</span>
            </Label>
            <Input
              id="name"
              data-testid="input-patient-name"
              placeholder="e.g. Max"
              {...register('name')}
            />
            {errors.name && (
              <p className="text-xs text-destructive" data-testid="error-patient-name">
                {errors.name.message}
              </p>
            )}
          </div>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {/* Species */}
            <div className="space-y-1">
              <Label htmlFor="species">
                Species <span className="text-destructive">*</span>
              </Label>
              <Select
                value={selectedSpecies}
                onValueChange={(val) => setValue('species', val as Species, { shouldValidate: true })}
                data-testid="select-species"
              >
                <SelectTrigger data-testid="select-species-trigger">
                  <SelectValue placeholder="Select species">
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
                <p className="text-xs text-destructive" data-testid="error-species">
                  {errors.species.message}
                </p>
              )}
            </div>

            {/* Breed */}
            <div className="space-y-1">
              <Label htmlFor="breed">Breed</Label>
              <Input
                id="breed"
                data-testid="input-breed"
                placeholder="e.g. Golden Retriever"
                {...register('breed')}
              />
            </div>

            {/* Date of Birth */}
            <div className="space-y-1">
              <Label htmlFor="dateOfBirth">
                Date of Birth <span className="text-destructive">*</span>
              </Label>
              <Input
                id="dateOfBirth"
                type="date"
                data-testid="input-date-of-birth"
                max={today}
                {...register('dateOfBirth')}
              />
              {errors.dateOfBirth && (
                <p className="text-xs text-destructive" data-testid="error-date-of-birth">
                  {errors.dateOfBirth.message}
                </p>
              )}
            </div>

            {/* Gender */}
            <div className="space-y-1">
              <Label htmlFor="gender">Sex</Label>
              <Select
                value={selectedGender}
                onValueChange={(val) =>
                  setValue('gender', val as 'Male' | 'Female' | 'Unknown', { shouldValidate: true })
                }
                data-testid="select-gender"
              >
                <SelectTrigger data-testid="select-gender-trigger">
                  <SelectValue placeholder="Select sex">
                    {selectedGender ?? null}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Male" data-testid="gender-option-male">Male</SelectItem>
                  <SelectItem value="Female" data-testid="gender-option-female">Female</SelectItem>
                  <SelectItem value="Unknown" data-testid="gender-option-unknown">Unknown</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Weight */}
            <div className="space-y-1">
              <Label htmlFor="weightKg">
                Weight (kg){' '}
                <span className="text-muted-foreground font-normal">(optional)</span>
              </Label>
              <Input
                id="weightKg"
                type="number"
                step="0.1"
                min="0"
                data-testid="patient-weight-input"
                placeholder="e.g. 32.5"
                {...register('weightKg')}
              />
              <p className="text-xs text-muted-foreground">Optional — used for dosage calculations</p>
              {errors.weightKg && (
                <p className="text-xs text-destructive" data-testid="error-weight">
                  {errors.weightKg.message}
                </p>
              )}
            </div>
          </div>

          <hr className="border-border" />
          <p className="text-sm font-medium text-muted-foreground">Owner Information</p>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {/* Owner Name */}
            <div className="space-y-1 md:col-span-2">
              <Label htmlFor="ownerName">
                Full Name <span className="text-destructive">*</span>
              </Label>
              <Input
                id="ownerName"
                data-testid="input-owner-name"
                placeholder="e.g. Ahmed Al-Mansoori"
                {...register('ownerName')}
              />
              {errors.ownerName && (
                <p className="text-xs text-destructive" data-testid="error-owner-name">
                  {errors.ownerName.message}
                </p>
              )}
            </div>

            {/* Owner Phone */}
            <div className="space-y-1">
              <Label htmlFor="ownerPhone">
                Phone <span className="text-destructive">*</span>
              </Label>
              <Input
                id="ownerPhone"
                type="tel"
                data-testid="input-owner-phone"
                placeholder="+971 50 123 4567"
                {...register('ownerPhone')}
              />
              {errors.ownerPhone && (
                <p className="text-xs text-destructive" data-testid="error-owner-phone">
                  {errors.ownerPhone.message}
                </p>
              )}
            </div>

            {/* Owner Email */}
            <div className="space-y-1">
              <Label htmlFor="ownerEmail">
                Email <span className="text-muted-foreground">(optional)</span>
              </Label>
              <Input
                id="ownerEmail"
                type="email"
                data-testid="input-owner-email"
                placeholder="owner@email.ae"
                {...register('ownerEmail')}
              />
              {errors.ownerEmail && (
                <p className="text-xs text-destructive" data-testid="error-owner-email">
                  {errors.ownerEmail.message}
                </p>
              )}
            </div>
          </div>

          {serverError && (
            <p className="text-sm text-destructive" data-testid="error-server" role="alert">
              {serverError}
            </p>
          )}

          <div className="flex gap-3 justify-end">
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
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="btn-save-patient"
            >
              {isSubmitting ? 'Saving...' : 'Save Patient'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

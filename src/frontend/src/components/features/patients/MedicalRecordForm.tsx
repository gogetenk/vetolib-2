'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { createMedicalRecord } from '@/lib/api/medical-records'

const medicalRecordSchema = z.object({
  reason: z.string().min(1, 'Reason is required'),
  anamnesis: z.string().min(1, 'Anamnesis is required'),
  weight: z.coerce.number().positive('Weight must be positive'),
  temperature: z.coerce.number().min(35).max(43, 'Temperature must be between 35 and 43°C'),
  heartRate: z.coerce.number().positive('Heart rate must be positive'),
  diagnosis: z.string().min(1, 'Diagnosis is required'),
  treatment: z.string().min(1, 'Treatment is required'),
  prescription: z.string().optional(),
  nextVisitDate: z.string().optional(),
})

type MedicalRecordFormValues = z.infer<typeof medicalRecordSchema>

interface MedicalRecordFormProps {
  patientId: string
  patientName: string
}

export function MedicalRecordForm({ patientId, patientName }: MedicalRecordFormProps) {
  const router = useRouter()
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  } = useForm<MedicalRecordFormValues>({
    resolver: zodResolver(medicalRecordSchema) as any,
  })

  const onSubmit = async (data: MedicalRecordFormValues) => {
    setServerError(null)
    try {
      await createMedicalRecord(patientId, {
        reason: data.reason,
        anamnesis: data.anamnesis,
        weight: data.weight,
        temperature: data.temperature,
        heartRate: data.heartRate,
        diagnosis: data.diagnosis,
        treatment: data.treatment,
        prescription: data.prescription || undefined,
        nextVisitDate: data.nextVisitDate || undefined,
      })
      router.push(`/patients/${patientId}`)
    } catch {
      setServerError('Failed to save record. Please try again.')
    }
  }

  return (
    <Card data-testid="medical-record-form">
      <CardHeader>
        <CardTitle>New Medical Record — {patientName}</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="space-y-6"
          data-testid="medical-record-form-body"
          noValidate
        >
          {/* Reason */}
          <div className="space-y-2">
            <Label htmlFor="reason">Reason for Consultation</Label>
            <Input
              id="reason"
              placeholder="e.g. Annual vaccination, Limping"
              data-testid="input-reason"
              aria-invalid={!!errors.reason}
              {...register('reason')}
            />
            {errors.reason && (
              <p className="text-xs text-destructive" data-testid="error-reason">
                {errors.reason.message}
              </p>
            )}
          </div>

          {/* Anamnesis */}
          <div className="space-y-2">
            <Label htmlFor="anamnesis">Anamnesis</Label>
            <textarea
              id="anamnesis"
              rows={4}
              placeholder="Patient history, owner observations..."
              data-testid="input-anamnesis"
              aria-invalid={!!errors.anamnesis}
              className="flex min-h-[80px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              {...register('anamnesis')}
            />
            {errors.anamnesis && (
              <p className="text-xs text-destructive" data-testid="error-anamnesis">
                {errors.anamnesis.message}
              </p>
            )}
          </div>

          {/* Clinical exam */}
          <fieldset className="space-y-3">
            <legend className="text-sm font-medium">Clinical Examination</legend>
            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="weight">Weight (kg)</Label>
                <Input
                  id="weight"
                  type="number"
                  step="0.1"
                  placeholder="32.5"
                  data-testid="input-weight"
                  aria-invalid={!!errors.weight}
                  {...register('weight')}
                />
                {errors.weight && (
                  <p className="text-xs text-destructive" data-testid="error-weight">
                    {errors.weight.message}
                  </p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="temperature">Temperature (°C)</Label>
                <Input
                  id="temperature"
                  type="number"
                  step="0.1"
                  placeholder="38.5"
                  data-testid="input-temperature"
                  aria-invalid={!!errors.temperature}
                  {...register('temperature')}
                />
                {errors.temperature && (
                  <p className="text-xs text-destructive" data-testid="error-temperature">
                    {errors.temperature.message}
                  </p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="heartRate">Heart Rate (bpm)</Label>
                <Input
                  id="heartRate"
                  type="number"
                  placeholder="80"
                  data-testid="input-heart-rate"
                  aria-invalid={!!errors.heartRate}
                  {...register('heartRate')}
                />
                {errors.heartRate && (
                  <p className="text-xs text-destructive" data-testid="error-heart-rate">
                    {errors.heartRate.message}
                  </p>
                )}
              </div>
            </div>
          </fieldset>

          {/* Diagnosis */}
          <div className="space-y-2">
            <Label htmlFor="diagnosis">Diagnosis</Label>
            <textarea
              id="diagnosis"
              rows={3}
              placeholder="Clinical findings and diagnosis..."
              data-testid="input-diagnosis"
              aria-invalid={!!errors.diagnosis}
              className="flex min-h-[60px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              {...register('diagnosis')}
            />
            {errors.diagnosis && (
              <p className="text-xs text-destructive" data-testid="error-diagnosis">
                {errors.diagnosis.message}
              </p>
            )}
          </div>

          {/* Treatment */}
          <div className="space-y-2">
            <Label htmlFor="treatment">Treatment</Label>
            <textarea
              id="treatment"
              rows={3}
              placeholder="Treatment plan, procedures performed..."
              data-testid="input-treatment"
              aria-invalid={!!errors.treatment}
              className="flex min-h-[60px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              {...register('treatment')}
            />
            {errors.treatment && (
              <p className="text-xs text-destructive" data-testid="error-treatment">
                {errors.treatment.message}
              </p>
            )}
          </div>

          {/* Prescription (optional) */}
          <div className="space-y-2">
            <Label htmlFor="prescription">
              Prescription <span className="text-muted-foreground">(optional)</span>
            </Label>
            <textarea
              id="prescription"
              rows={3}
              placeholder="Medications, dosage, duration..."
              data-testid="input-prescription"
              className="flex min-h-[60px] w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              {...register('prescription')}
            />
          </div>

          {/* Next visit (optional) */}
          <div className="space-y-2">
            <Label htmlFor="nextVisitDate">
              Recommended Next Visit <span className="text-muted-foreground">(optional)</span>
            </Label>
            <Input
              id="nextVisitDate"
              type="date"
              data-testid="input-next-visit-date"
              {...register('nextVisitDate')}
            />
          </div>

          {serverError && (
            <p
              className="text-sm text-destructive"
              data-testid="error-server"
              role="alert"
            >
              {serverError}
            </p>
          )}

          <div className="flex gap-3">
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="save-record-btn"
            >
              {isSubmitting ? 'Saving...' : 'Save Record'}
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={() => router.push(`/patients/${patientId}`)}
              data-testid="cancel-record-btn"
            >
              Cancel
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

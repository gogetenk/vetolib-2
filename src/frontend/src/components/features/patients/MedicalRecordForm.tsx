'use client'

import { useCallback, useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { createMedicalRecord } from '@/lib/api/medical-records'
import { checkPrescriptionPreflight } from '@/lib/api/prescriptions'
import { DrugSelector } from './DrugSelector'
import { InteractionAlertsPanel } from './InteractionAlertsPanel'
import { OverrideSection } from './OverrideSection'
import { AlternativeSuggestions } from './AlternativeSuggestions'
import { DosageRangeIndicator } from './DosageRangeIndicator'
import type { DrugSelectorValue } from './DrugSelector'
import type {
  InteractionAlert,
  PrescriptionPreflightResult,
  SafeAlternative,
} from '@/lib/api/types'
import type { Species } from '@/lib/api/patients'

// ─── Schema ───────────────────────────────────────────────────────────────────

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

// ─── Props ────────────────────────────────────────────────────────────────────

interface MedicalRecordFormProps {
  patientId: string
  patientName: string
  /** Optional patient species — used for preflight species contraindication check */
  patientSpecies?: Species
  /** Optional patient weight from last record — used for dosage range calculation */
  patientWeightKg?: number | null
}

// ─── Component ────────────────────────────────────────────────────────────────

export function MedicalRecordForm({
  patientId,
  patientName,
  patientSpecies,
  patientWeightKg,
}: MedicalRecordFormProps) {
  const router = useRouter()
  const [serverError, setServerError] = useState<string | null>(null)

  // Drug selection state
  const [drugSelection, setDrugSelection] = useState<DrugSelectorValue | null>(null)

  // Preflight state
  const [preflightLoading, setPreflightLoading] = useState(false)
  const [preflightResult, setPreflightResult] = useState<PrescriptionPreflightResult | null>(null)

  // Override justification (only shown when Critical alert present)
  const [justification, setJustification] = useState('')
  const [overrideConfirmed, setOverrideConfirmed] = useState(false)

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
  } = useForm<MedicalRecordFormValues>({
    resolver: zodResolver(medicalRecordSchema) as any,
  })

  // Watch weight field so preflight can use up-to-date value
  const weightFieldValue = watch('weight')

  // ── Alerts derived state ───────────────────────────────────────────────────
  const alerts: InteractionAlert[] = preflightResult?.interactionAlerts ?? []
  const hasCritical = alerts.some(a => a.severity === 'Critical')
  const safeAlternatives: SafeAlternative[] = preflightResult?.safeAlternatives ?? []
  const dosageRange = preflightResult?.dosageRange ?? null

  // Whether submit is blocked: Critical alert present and override not confirmed
  const isSubmitBlocked = hasCritical && !overrideConfirmed

  // ── Trigger preflight when drug selection changes ──────────────────────────
  const runPreflight = useCallback(
    async (selection: DrugSelectorValue | null) => {
      if (!selection || selection.mode !== 'catalog') {
        setPreflightResult(null)
        setJustification('')
        setOverrideConfirmed(false)
        return
      }

      const drug = selection.drug
      const weightKg =
        typeof weightFieldValue === 'number' && weightFieldValue > 0
          ? weightFieldValue
          : patientWeightKg ?? undefined

      setPreflightLoading(true)
      setPreflightResult(null)
      setJustification('')
      setOverrideConfirmed(false)

      try {
        const result = await checkPrescriptionPreflight({
          patientId,
          drugCatalogEntryId: drug.id,
          patientSpecies: patientSpecies,
          patientWeightKg: weightKg ?? undefined,
        })
        setPreflightResult(result)
      } catch {
        // Preflight failure is non-blocking — silently clear
        setPreflightResult(null)
      } finally {
        setPreflightLoading(false)
      }
    },
    [patientId, patientSpecies, patientWeightKg, weightFieldValue]
  )

  const handleDrugChange = useCallback(
    (value: DrugSelectorValue | null) => {
      setDrugSelection(value)
      runPreflight(value)
    },
    [runPreflight]
  )

  // Re-run preflight if weight changes after drug is selected (catalog mode only)
  useEffect(() => {
    if (drugSelection?.mode === 'catalog' && weightFieldValue > 0) {
      // Debounce: only re-run if weight actually settled
      const timer = setTimeout(() => runPreflight(drugSelection), 600)
      return () => clearTimeout(timer)
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [weightFieldValue])

  // ── Alternative selection handler ──────────────────────────────────────────
  const handleAlternativeSelect = useCallback((alt: SafeAlternative) => {
    // Build a minimal DrugCatalogEntryDto-like object from the alternative data
    // and trigger a new drug selection. Since we don't have the full catalog entry,
    // we reset to free-text with the alternative name.
    setDrugSelection({ mode: 'free-text', text: `${alt.displayName} — ${alt.commonDosage}` })
    setPreflightResult(null)
    setJustification('')
    setOverrideConfirmed(false)
  }, [])

  // ── Submit ─────────────────────────────────────────────────────────────────
  const onSubmit = async (data: MedicalRecordFormValues) => {
    if (isSubmitBlocked) return
    setServerError(null)

    let prescriptionText: string | undefined
    if (drugSelection) {
      if (drugSelection.mode === 'catalog') {
        prescriptionText = `${drugSelection.drug.displayName} — ${drugSelection.drug.commonDosage}`
        if (justification) {
          prescriptionText += ` [Override: ${justification}]`
        }
      } else {
        prescriptionText = drugSelection.text || undefined
      }
    } else {
      prescriptionText = data.prescription || undefined
    }

    try {
      await createMedicalRecord(patientId, {
        reason: data.reason,
        anamnesis: data.anamnesis,
        weight: data.weight,
        temperature: data.temperature,
        heartRate: data.heartRate,
        diagnosis: data.diagnosis,
        treatment: data.treatment,
        prescription: prescriptionText,
        nextVisitDate: data.nextVisitDate || undefined,
      })
      router.push(`/patients/${patientId}`)
    } catch {
      setServerError('Failed to save record. Please try again.')
    }
  }

  // ── Render ─────────────────────────────────────────────────────────────────

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

          {/* Prescription (optional) — Drug Selector + preflight results */}
          <div className="space-y-3" data-testid="prescription-section">
            <DrugSelector
              label="Prescription (optional)"
              onChange={handleDrugChange}
              defaultValue={drugSelection}
            />

            {/* Dosage range indicator (shown when preflight returns a range) */}
            {!preflightLoading && dosageRange && (
              <DosageRangeIndicator
                range={dosageRange}
                patientWeightKg={
                  typeof weightFieldValue === 'number' && weightFieldValue > 0
                    ? weightFieldValue
                    : patientWeightKg ?? undefined
                }
              />
            )}

            {/* Interaction alerts */}
            <InteractionAlertsPanel
              alerts={alerts}
              isLoading={preflightLoading}
            />

            {/* Override section — only shown when Critical alert present */}
            {!preflightLoading && hasCritical && !overrideConfirmed && (
              <OverrideSection
                justification={justification}
                onChange={setJustification}
                onConfirm={() => setOverrideConfirmed(true)}
                isSubmitting={isSubmitting}
              />
            )}

            {/* Confirmed override badge */}
            {overrideConfirmed && (
              <p
                className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded px-2 py-1"
                data-testid="override-confirmed-notice"
                role="status"
              >
                Override confirmed. Justification recorded.
              </p>
            )}

            {/* Alternative suggestions */}
            {!preflightLoading && safeAlternatives.length > 0 && (
              <AlternativeSuggestions
                alternatives={safeAlternatives}
                onSelect={handleAlternativeSelect}
              />
            )}
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
              disabled={isSubmitting || isSubmitBlocked}
              data-testid="save-record-btn"
              title={isSubmitBlocked ? 'Provide clinical justification before saving' : undefined}
            >
              {isSubmitting ? 'Saving...' : 'Save Record'}
            </Button>
            {isSubmitBlocked && (
              <p
                className="self-center text-xs text-destructive"
                data-testid="submit-blocked-notice"
                role="status"
              >
                Provide justification for the critical alert above to enable saving.
              </p>
            )}
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

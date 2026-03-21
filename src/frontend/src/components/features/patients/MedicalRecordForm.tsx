'use client'

import { useCallback, useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { createMedicalRecord } from '@/lib/api/medical-records'
import { checkPrescriptionPreflight } from '@/lib/api/prescriptions'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'
import { DrugSelector } from './DrugSelector'
import { InteractionAlertsPanel } from './InteractionAlertsPanel'
import { OverrideSection } from './OverrideSection'
import { AlternativeSuggestions } from './AlternativeSuggestions'
import { DosageRangeIndicator } from './DosageRangeIndicator'
import { StockAvailabilityPanel } from './StockAvailabilityPanel'
import { DispenseToggle } from './DispenseToggle'
import { SoapNotesPanel } from './SoapNotesPanel'
import type { SoapNotesValues } from './SoapNotesPanel'
import type { DrugSelectorValue } from './DrugSelector'
import type { DispenseToggleValue } from './DispenseToggle'
import type {
  InteractionAlert,
  PrescriptionPreflightResult,
  SafeAlternative,
  StockAvailabilityResult,
  StockAlternativeDto,
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

  // Dispense toggle state
  const [, setDispenseValue] = useState<DispenseToggleValue>({
    dispense: false,
    quantity: null,
    partialConfirmed: false,
  })

  // SOAP Notes panel state
  const [soapOpen, setSoapOpen] = useState(false)

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<MedicalRecordFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(medicalRecordSchema) as any,
  })

  // Watch fields for preflight and SOAP notes
  const weightFieldValue = watch('weight')
  const reasonFieldValue = watch('reason')
  const anamnesisFieldValue = watch('anamnesis')
  const temperatureFieldValue = watch('temperature')
  const heartRateFieldValue = watch('heartRate')

  // SOAP notes accept handler — fills diagnosis and treatment fields
  const handleSoapAccept = useCallback(
    (soapValues: SoapNotesValues) => {
      // Map SOAP sections to form fields:
      // Subjective → anamnesis (enriches owner history)
      // Assessment → diagnosis
      // Plan → treatment
      setValue('diagnosis', soapValues.assessment, { shouldValidate: true })
      setValue('treatment', soapValues.plan, { shouldValidate: true })
      // Enrich anamnesis with subjective if current value is shorter
      const currentAnamnesis = anamnesisFieldValue || ''
      if (soapValues.subjective.length > currentAnamnesis.length) {
        setValue('anamnesis', soapValues.subjective, { shouldValidate: true })
      }
      setSoapOpen(false)
    },
    [setValue, anamnesisFieldValue]
  )

  // ── Alerts derived state ───────────────────────────────────────────────────
  const alerts: InteractionAlert[] = preflightResult?.interactionAlerts ?? []
  const hasCritical = alerts.some(a => a.severity === 'Critical')
  const safeAlternatives: SafeAlternative[] = preflightResult?.safeAlternatives ?? []
  const dosageRange = preflightResult?.dosageRange ?? null

  // ── Stock derived state ────────────────────────────────────────────────────
  // stockAvailability may be a boolean (legacy) or a rich StockAvailabilityResult
  const stockResult: StockAvailabilityResult | null = (() => {
    if (!preflightResult) return null
    const raw = preflightResult.stockAvailability
    if (typeof raw === 'boolean') return null
    return raw
  })()

  const isCatalogMode = drugSelection?.mode === 'catalog'

  // Whether submit is blocked: Critical alert present and override not confirmed
  const isSubmitBlocked = hasCritical && !overrideConfirmed

  // ── Trigger preflight when drug selection changes ──────────────────────────
  const runPreflight = useCallback(
    async (selection: DrugSelectorValue | null) => {
      if (!selection || selection.mode !== 'catalog') {
        setPreflightResult(null)
        setJustification('')
        setOverrideConfirmed(false)
        setDispenseValue({ dispense: false, quantity: null, partialConfirmed: false })
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

  // ── Alternative selection handler (drug catalog alternatives) ─────────────
  const handleAlternativeSelect = useCallback((alt: SafeAlternative) => {
    // Build a minimal DrugCatalogEntryDto-like object from the alternative data
    // and trigger a new drug selection. Since we don't have the full catalog entry,
    // we reset to free-text with the alternative name.
    setDrugSelection({ mode: 'free-text', text: `${alt.displayName} — ${alt.commonDosage}` })
    setPreflightResult(null)
    setJustification('')
    setOverrideConfirmed(false)
    setDispenseValue({ dispense: false, quantity: null, partialConfirmed: false })
  }, [])

  // ── Stock alternative selection handler ────────────────────────────────────
  const handleStockAlternativeSelect = useCallback((alt: StockAlternativeDto) => {
    setDrugSelection({ mode: 'free-text', text: alt.name })
    setPreflightResult(null)
    setJustification('')
    setOverrideConfirmed(false)
    setDispenseValue({ dispense: false, quantity: null, partialConfirmed: false })
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
      trackEvent(AnalyticsEvents.MEDICAL_RECORD_ADDED, {
        has_prescription: String(Boolean(prescriptionText)),
        record_type: "consultation",
      })
      router.push(`/patients/${patientId}`)
    } catch {
      setServerError('Failed to save record. Please try again.')
    }
  }

  // ── Render ─────────────────────────────────────────────────────────────────

  return (
    <Card className="bg-white border-border/80 rounded-xl shadow-sm" data-testid="medical-record-form">
      <CardHeader>
        <CardTitle className="text-[18px] font-bold text-foreground">New Medical Record — {patientName}</CardTitle>
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
            <Label htmlFor="reason" className="text-[13px] font-semibold text-foreground">Reason for Consultation</Label>
            <Input
              id="reason"
              placeholder="e.g. Annual vaccination, Limping"
              data-testid="input-reason"
              aria-invalid={!!errors.reason}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
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
            <Label htmlFor="anamnesis" className="text-[13px] font-semibold text-foreground">Anamnesis</Label>
            <Textarea
              id="anamnesis"
              rows={4}
              placeholder="Patient history, owner observations..."
              data-testid="input-anamnesis"
              aria-invalid={!!errors.anamnesis}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
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
            <legend className="text-[13px] font-bold text-foreground">Clinical Examination</legend>
            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="weight" className="text-[13px] font-semibold text-foreground">Weight (kg)</Label>
                <Input
                  id="weight"
                  type="number"
                  step="0.1"
                  placeholder="32.5"
                  data-testid="input-weight"
                  aria-invalid={!!errors.weight}
                  className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                  {...register('weight')}
                />
                {errors.weight && (
                  <p className="text-xs text-destructive" data-testid="error-weight">
                    {errors.weight.message}
                  </p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="temperature" className="text-[13px] font-semibold text-foreground">Temperature (°C)</Label>
                <Input
                  id="temperature"
                  type="number"
                  step="0.1"
                  placeholder="38.5"
                  data-testid="input-temperature"
                  aria-invalid={!!errors.temperature}
                  className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
                  {...register('temperature')}
                />
                {errors.temperature && (
                  <p className="text-xs text-destructive" data-testid="error-temperature">
                    {errors.temperature.message}
                  </p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="heartRate" className="text-[13px] font-semibold text-foreground">Heart Rate (bpm)</Label>
                <Input
                  id="heartRate"
                  type="number"
                  placeholder="80"
                  data-testid="input-heart-rate"
                  aria-invalid={!!errors.heartRate}
                  className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
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
            <Label htmlFor="diagnosis" className="text-[13px] font-semibold text-foreground">Diagnosis</Label>
            <Textarea
              id="diagnosis"
              rows={3}
              placeholder="Clinical findings and diagnosis..."
              data-testid="input-diagnosis"
              aria-invalid={!!errors.diagnosis}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
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
            <Label htmlFor="treatment" className="text-[13px] font-semibold text-foreground">Treatment</Label>
            <Textarea
              id="treatment"
              rows={3}
              placeholder="Treatment plan, procedures performed..."
              data-testid="input-treatment"
              aria-invalid={!!errors.treatment}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              {...register('treatment')}
            />
            {errors.treatment && (
              <p className="text-xs text-destructive" data-testid="error-treatment">
                {errors.treatment.message}
              </p>
            )}
          </div>

          {/* AI SOAP Notes */}
          <div className="space-y-3" data-testid="soap-notes-section">
            {!soapOpen && (
              <Button
                type="button"
                variant="outline"
                onClick={() => setSoapOpen(true)}
                data-testid="open-soap-btn"
                className="rounded-xl font-semibold border-blue-200 text-blue-700 hover:bg-blue-50"
              >
                Generate SOAP Notes
              </Button>
            )}
            <SoapNotesPanel
              open={soapOpen}
              requestData={{
                reason: reasonFieldValue || '',
                anamnesis: anamnesisFieldValue || '',
                species: patientSpecies,
                weight: typeof weightFieldValue === 'number' && weightFieldValue > 0 ? weightFieldValue : undefined,
                temperature: typeof temperatureFieldValue === 'number' && temperatureFieldValue > 0 ? temperatureFieldValue : undefined,
                heartRate: typeof heartRateFieldValue === 'number' && heartRateFieldValue > 0 ? heartRateFieldValue : undefined,
              }}
              onAccept={handleSoapAccept}
            />
          </div>

          {/* Prescription (optional) — Drug Selector + preflight results */}
          <div className="space-y-3" data-testid="prescription-section">
            <DrugSelector
              label="Prescription (optional)"
              onChange={handleDrugChange}
              defaultValue={drugSelection}
            />

            {/* Stock availability — catalog mode only */}
            {!preflightLoading && isCatalogMode && stockResult && (
              <StockAvailabilityPanel
                stock={stockResult}
                onSelectAlternative={handleStockAlternativeSelect}
              />
            )}

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
                className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-xl px-3 py-1.5"
                data-testid="override-confirmed-notice"
                role="status"
              >
                Override confirmed. Justification recorded.
              </p>
            )}

            {/* Alternative suggestions (drug catalog alternatives) */}
            {!preflightLoading && safeAlternatives.length > 0 && (
              <AlternativeSuggestions
                alternatives={safeAlternatives}
                onSelect={handleAlternativeSelect}
              />
            )}

            {/* Dispense toggle — shown when a drug is selected (catalog mode, stock info available) */}
            {isCatalogMode && stockResult && (
              <DispenseToggle
                prescribedQuantity={undefined}
                availableQuantity={stockResult.available ? stockResult.quantity : 0}
                unit={stockResult.unit}
                stockAvailable={stockResult.available}
                onChange={setDispenseValue}
              />
            )}
          </div>

          {/* Next visit (optional) */}
          <div className="space-y-2">
            <Label htmlFor="nextVisitDate" className="text-[13px] font-semibold text-foreground">
              Recommended Next Visit <span className="text-muted-foreground font-normal">(optional)</span>
            </Label>
            <Input
              id="nextVisitDate"
              type="date"
              data-testid="input-next-visit-date"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
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

          <div className="sticky bottom-0 bg-white py-3 border-t border-border/50 flex gap-3 justify-end items-center -mx-6 px-6">
            {isSubmitBlocked && (
              <p
                className="text-xs text-destructive mr-auto"
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
              className="rounded-xl font-semibold border-border/80 hover:bg-muted"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={isSubmitting || isSubmitBlocked}
              data-testid="save-record-btn"
              title={isSubmitBlocked ? 'Provide clinical justification before saving' : undefined}
              className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
            >
              {isSubmitting ? 'Saving...' : 'Save Record'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

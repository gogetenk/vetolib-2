'use client'

import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'

// ─── Constants ────────────────────────────────────────────────────────────────

const MIN_CHARS = 10

// ─── Props ────────────────────────────────────────────────────────────────────

interface OverrideSectionProps {
  justification: string
  onChange: (value: string) => void
  /** Called when the user confirms the override */
  onConfirm: () => void
  /** Whether a preflight submission is in progress */
  isSubmitting?: boolean
}

// ─── Component ────────────────────────────────────────────────────────────────

export function OverrideSection({
  justification,
  onChange,
  onConfirm,
  isSubmitting = false,
}: OverrideSectionProps) {
  const charCount = justification.length
  const isValid = charCount >= MIN_CHARS

  return (
    <div
      className="rounded-xl border border-red-300 bg-red-50 p-4 space-y-3 dark:border-red-800 dark:bg-red-950"
      data-testid="override-section"
    >
      <div className="space-y-1">
        <p className="text-[13px] font-bold text-red-900 dark:text-red-200">
          Critical alert detected — clinical justification required
        </p>
        <p className="text-xs text-red-700 dark:text-red-400">
          You must provide a justification of at least {MIN_CHARS} characters to proceed with this prescription.
        </p>
      </div>

      <div className="space-y-2">
        <Label
          htmlFor="override-justification"
          className="text-[13px] font-semibold text-red-900 dark:text-red-200"
        >
          Clinical justification
        </Label>
        <Textarea
          id="override-justification"
          data-testid="override-justification-input"
          rows={3}
          value={justification}
          onChange={e => onChange(e.target.value)}
          placeholder="e.g. Patient history reviewed, weight adjusted — benefit outweighs risk because..."
          aria-required="true"
          aria-invalid={!isValid && charCount > 0}
          aria-describedby="override-char-count"
          className="rounded-xl border-red-300 focus-visible:ring-red-500/50 dark:border-red-700 text-[13px]"
        />
        <p
          id="override-char-count"
          data-testid="override-char-count"
          className={`text-xs ${isValid ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}`}
        >
          {charCount} / {MIN_CHARS} characters minimum
        </p>
      </div>

      <Button
        type="button"
        data-testid="override-submit-button"
        variant="destructive"
        disabled={!isValid || isSubmitting}
        onClick={onConfirm}
        className="w-full sm:w-auto rounded-xl font-semibold"
      >
        {isSubmitting ? 'Saving...' : 'Override & Save Prescription'}
      </Button>
    </div>
  )
}

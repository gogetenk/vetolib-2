'use client'

import type { DosageRange } from '@/lib/api/types'

// ─── Props ────────────────────────────────────────────────────────────────────

interface DosageRangeIndicatorProps {
  range: DosageRange
  /** Current dosage entered by the user (mg/kg) */
  currentDosage?: number
  patientWeightKg?: number
}

// ─── Component ────────────────────────────────────────────────────────────────

export function DosageRangeIndicator({
  range,
  currentDosage,
  patientWeightKg,
}: DosageRangeIndicatorProps) {
  const inRange =
    currentDosage !== undefined
      ? currentDosage >= range.minDose && currentDosage <= range.maxDose
      : true

  const statusClass = currentDosage !== undefined
    ? inRange
      ? 'text-green-700 bg-green-50 border-green-200 dark:text-green-300 dark:bg-green-950 dark:border-green-800'
      : 'text-orange-700 bg-orange-50 border-orange-200 dark:text-orange-300 dark:bg-orange-950 dark:border-orange-800'
    : 'text-blue-700 bg-blue-50 border-blue-200 dark:text-blue-300 dark:bg-blue-950 dark:border-blue-800'

  return (
    <div
      data-testid="dosage-range-indicator"
      className={`rounded-md border px-3 py-2 text-xs space-y-1 ${statusClass}`}
      aria-live="polite"
    >
      <p className="font-semibold">Recommended dosage range</p>
      <div className="flex items-center gap-4">
        <span>
          Min:{' '}
          <strong data-testid="dosage-range-min">
            {range.minDose} {range.unit}
          </strong>
        </span>
        <span>
          Max:{' '}
          <strong data-testid="dosage-range-max">
            {range.maxDose} {range.unit}
          </strong>
        </span>
        {range.recommendedDose !== undefined && patientWeightKg && (
          <span>
            Recommended total: <strong>{range.recommendedDose.toFixed(1)} mg</strong> (
            {patientWeightKg} kg)
          </span>
        )}
      </div>
      {currentDosage !== undefined && !inRange && (
        <p className="font-medium">
          Current dose ({currentDosage} {range.unit}) is outside the recommended range.
        </p>
      )}
    </div>
  )
}

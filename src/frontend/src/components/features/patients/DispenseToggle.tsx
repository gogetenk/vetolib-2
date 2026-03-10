'use client'

import { useEffect, useState } from 'react'
import { AlertTriangle } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'

// ─── Props ────────────────────────────────────────────────────────────────────

export interface DispenseToggleValue {
  dispense: boolean
  quantity: number | null
  /** true when the user confirmed a partial dispense */
  partialConfirmed: boolean
}

interface DispenseToggleProps {
  /** Prescribed quantity (pre-fills the quantity input) */
  prescribedQuantity?: number
  /** Available stock quantity — used to detect insufficient stock */
  availableQuantity?: number
  /** Unit label (e.g. "tablets") */
  unit?: string
  /** Whether stock is available at all */
  stockAvailable?: boolean
  /** Called whenever dispense intent changes */
  onChange: (value: DispenseToggleValue) => void
}

// ─── Component ────────────────────────────────────────────────────────────────

export function DispenseToggle({
  prescribedQuantity,
  availableQuantity,
  unit = 'units',
  stockAvailable = true,
  onChange,
}: DispenseToggleProps) {
  const [dispense, setDispense] = useState(stockAvailable)
  const [quantityStr, setQuantityStr] = useState(
    prescribedQuantity !== undefined ? String(prescribedQuantity) : ''
  )
  const [partialConfirmed, setPartialConfirmed] = useState(false)

  const quantity = parseFloat(quantityStr)
  const isQuantityValid = !isNaN(quantity) && quantity > 0

  // Detect insufficient stock
  const isInsufficient =
    dispense &&
    isQuantityValid &&
    availableQuantity !== undefined &&
    quantity > availableQuantity

  // Notify parent whenever state changes
  useEffect(() => {
    onChange({
      dispense,
      quantity: dispense && isQuantityValid ? quantity : null,
      partialConfirmed,
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dispense, quantityStr, partialConfirmed])

  // Reset partial confirmation when quantity or dispense changes
  useEffect(() => {
    setPartialConfirmed(false)
  }, [quantityStr, dispense])

  // Reset defaults when external props change (e.g. different drug selected)
  useEffect(() => {
    setDispense(stockAvailable)
    setQuantityStr(prescribedQuantity !== undefined ? String(prescribedQuantity) : '')
    setPartialConfirmed(false)
  }, [stockAvailable, prescribedQuantity])

  return (
    <div className="space-y-3" data-testid="dispense-toggle-container">
      {/* Checkbox row */}
      <div className="flex items-center gap-2">
        <input
          type="checkbox"
          id="dispense-toggle"
          data-testid="dispense-toggle"
          checked={dispense}
          onChange={(e) => setDispense(e.target.checked)}
          disabled={!stockAvailable}
          aria-label="Dispense from clinic stock"
          className="h-4 w-4 rounded border-input accent-primary cursor-pointer disabled:cursor-not-allowed disabled:opacity-50"
        />
        <Label
          htmlFor="dispense-toggle"
          className="cursor-pointer select-none"
        >
          Dispense from clinic stock
          {!stockAvailable && (
            <span className="ml-1 text-muted-foreground text-xs">(out of stock)</span>
          )}
        </Label>
      </div>

      {/* Quantity input — shown only when dispense is checked */}
      {dispense && (
        <div className="space-y-2 pl-6">
          <Label htmlFor="dispense-quantity-input" className="text-sm">
            Quantity to dispense ({unit})
          </Label>
          <Input
            id="dispense-quantity-input"
            data-testid="dispense-quantity-input"
            type="number"
            min={0.5}
            step={0.5}
            value={quantityStr}
            onChange={(e) => setQuantityStr(e.target.value)}
            placeholder="e.g. 10"
            className="max-w-[140px]"
            aria-invalid={isInsufficient}
          />

          {/* Insufficient stock warning */}
          {isInsufficient && !partialConfirmed && (
            <div
              className="flex flex-col gap-2 rounded-md border border-amber-300 bg-amber-50 p-3 text-sm text-amber-900"
              data-testid="dispense-partial-warning"
              role="alert"
            >
              <div className="flex items-start gap-2">
                <AlertTriangle
                  className="h-4 w-4 text-amber-600 shrink-0 mt-0.5"
                  aria-hidden="true"
                />
                <span>
                  Only <strong>{availableQuantity} {unit}</strong> available in stock.
                  You requested <strong>{quantity} {unit}</strong>.
                  Do you want to perform a partial dispense?
                </span>
              </div>
              <Button
                type="button"
                data-testid="dispense-partial-confirm"
                size="sm"
                variant="outline"
                className="self-start border-amber-400 text-amber-800 hover:bg-amber-100"
                onClick={() => setPartialConfirmed(true)}
              >
                Confirm partial dispense ({availableQuantity} {unit})
              </Button>
            </div>
          )}

          {/* Partial confirmed notice */}
          {partialConfirmed && isInsufficient && (
            <p
              className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded px-2 py-1"
              data-testid="dispense-partial-confirmed-notice"
              role="status"
            >
              Partial dispense confirmed: {availableQuantity} {unit} will be dispensed.
            </p>
          )}
        </div>
      )}
    </div>
  )
}

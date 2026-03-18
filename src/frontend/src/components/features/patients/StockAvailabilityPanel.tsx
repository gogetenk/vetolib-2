'use client'

import { Package, PackageX } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import type { StockAvailabilityResult, StockAlternativeDto } from '@/lib/api/types'

// ─── Props ────────────────────────────────────────────────────────────────────

interface StockAvailabilityPanelProps {
  stock: StockAvailabilityResult
  /** Called when user clicks an alternative to select it */
  onSelectAlternative?: (alt: StockAlternativeDto) => void
}

// ─── Component ────────────────────────────────────────────────────────────────

export function StockAvailabilityPanel({
  stock,
  onSelectAlternative,
}: StockAvailabilityPanelProps) {
  const isOutOfStock = !stock.available || stock.quantity === 0

  return (
    <div
      className="rounded-xl border border-border/80 bg-[#f4f6f9] p-3 space-y-2 text-[13px]"
      data-testid="stock-availability-panel"
    >
      {/* Header row */}
      <div className="flex items-center gap-2 flex-wrap">
        {isOutOfStock ? (
          <PackageX className="h-4 w-4 text-destructive shrink-0" aria-hidden="true" />
        ) : (
          <Package className="h-4 w-4 text-muted-foreground shrink-0" aria-hidden="true" />
        )}

        <span className="font-semibold text-[#061e44]">Clinic stock:</span>

        {!isOutOfStock && (
          <span data-testid="stock-quantity" className="text-[#061e44] font-medium">
            {stock.quantity} {stock.unit}
          </span>
        )}

        {/* Badges */}
        {isOutOfStock && (
          <Badge
            variant="destructive"
            data-testid="stock-badge-out"
            className="text-xs"
          >
            Out of stock
          </Badge>
        )}
        {!isOutOfStock && stock.isLowStock && (
          <Badge
            variant="outline"
            data-testid="stock-badge-low"
            className="text-xs border-amber-400 text-amber-700 bg-amber-50"
          >
            Low stock
          </Badge>
        )}
        {!isOutOfStock && stock.isExpiringSoon && (
          <Badge
            variant="outline"
            data-testid="stock-badge-expiring"
            className="text-xs border-orange-400 text-orange-700 bg-orange-50"
          >
            Expiring soon
          </Badge>
        )}
      </div>

      {/* Alternatives (shown when out of stock) */}
      {isOutOfStock && stock.alternatives.length > 0 && (
        <div className="space-y-1 pt-1 border-t border-border/50">
          <p className="text-xs text-muted-foreground font-medium">
            Available alternatives in stock:
          </p>
          <ul className="space-y-1" role="list">
            {stock.alternatives.map(alt => (
              <li
                key={alt.stockItemId}
                data-testid={`stock-alternative-${alt.stockItemId}`}
                className="flex items-center justify-between gap-2 rounded-xl border border-border/60 bg-white px-3 py-2 text-[12px]"
              >
                <span className="font-medium truncate">{alt.name}</span>
                <span className="text-muted-foreground shrink-0">
                  {alt.quantity} {alt.unit}
                </span>
                {onSelectAlternative && (
                  <button
                    type="button"
                    data-testid={`stock-alternative-select-${alt.stockItemId}`}
                    onClick={() => onSelectAlternative(alt)}
                    className="text-xs text-primary underline underline-offset-2 hover:no-underline shrink-0"
                  >
                    Use this
                  </button>
                )}
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}

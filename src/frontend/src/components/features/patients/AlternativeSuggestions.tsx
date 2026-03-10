'use client'

import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { ArrowRight } from 'lucide-react'
import type { SafeAlternative } from '@/lib/api/types'

// ─── Props ────────────────────────────────────────────────────────────────────

interface AlternativeSuggestionsProps {
  alternatives: SafeAlternative[]
  onSelect: (alternative: SafeAlternative) => void
}

// ─── Component ────────────────────────────────────────────────────────────────

export function AlternativeSuggestions({
  alternatives,
  onSelect,
}: AlternativeSuggestionsProps) {
  if (alternatives.length === 0) return null

  return (
    <div
      className="rounded-md border border-green-200 bg-green-50 p-4 space-y-3 dark:border-green-800 dark:bg-green-950"
      data-testid="alternative-suggestions"
    >
      <p className="text-sm font-semibold text-green-900 dark:text-green-200">
        Suggested safe alternatives
      </p>

      <ul className="space-y-2">
        {alternatives.map(alt => (
          <li
            key={alt.id}
            data-testid={`alternative-drug-${alt.id}`}
            className="flex items-center justify-between gap-3 rounded-md bg-white border border-green-100 px-3 py-2 dark:bg-green-900/20 dark:border-green-800"
          >
            <div className="flex flex-col min-w-0">
              <span className="text-sm font-medium text-green-900 dark:text-green-100 truncate">
                {alt.displayName}
              </span>
              <div className="flex items-center gap-2 mt-0.5">
                <Badge variant="secondary" className="text-xs">
                  {alt.innName}
                </Badge>
                <span className="text-xs text-muted-foreground truncate">
                  {alt.commonDosage}
                </span>
              </div>
            </div>
            <Button
              type="button"
              variant="outline"
              size="sm"
              data-testid={`alternative-use-button-${alt.id}`}
              onClick={() => onSelect(alt)}
              className="shrink-0 border-green-300 text-green-800 hover:bg-green-100 dark:border-green-700 dark:text-green-200"
            >
              <ArrowRight className="h-3.5 w-3.5 mr-1" />
              Use this instead
            </Button>
          </li>
        ))}
      </ul>
    </div>
  )
}

'use client'

import { useCallback, useEffect, useRef, useState } from 'react'
import { useTranslations } from 'next-intl'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Badge } from '@/components/ui/badge'
import { searchDrugs } from '@/lib/api/drugs'
import type { DrugCatalogEntryDto } from '@/lib/api/types'

// ─── Types ───────────────────────────────────────────────────────────────────

export type DrugSelectorValue =
  | { mode: 'catalog'; drug: DrugCatalogEntryDto }
  | { mode: 'free-text'; text: string }

interface DrugSelectorProps {
  /** Called whenever the selection changes */
  onChange: (value: DrugSelectorValue | null) => void
  /** Initial value (used when editing an existing record) */
  defaultValue?: DrugSelectorValue | null
  /** Label displayed above the input */
  label?: string
  /** Whether the field is required */
  required?: boolean
}

// ─── Debounce hook ───────────────────────────────────────────────────────────

function useDebounce<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState(value)
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(timer)
  }, [value, delay])
  return debounced
}

// ─── Component ───────────────────────────────────────────────────────────────

export function DrugSelector({
  onChange,
  defaultValue,
  label,
  required = false,
}: DrugSelectorProps) {
  const t = useTranslations('drug_selector')

  // Free-text mode state
  const [isFreeText, setIsFreeText] = useState(
    defaultValue?.mode === 'free-text'
  )

  // Catalog autocomplete state
  const [query, setQuery] = useState(
    defaultValue?.mode === 'catalog'
      ? defaultValue.drug.displayName
      : ''
  )
  const [results, setResults] = useState<DrugCatalogEntryDto[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [isOpen, setIsOpen] = useState(false)
  const [selected, setSelected] = useState<DrugCatalogEntryDto | null>(
    defaultValue?.mode === 'catalog' ? defaultValue.drug : null
  )

  // Free-text state
  const [freeText, setFreeText] = useState(
    defaultValue?.mode === 'free-text' ? defaultValue.text : ''
  )

  const debouncedQuery = useDebounce(query, 300)
  const containerRef = useRef<HTMLDivElement>(null)

  // ── Search effect ──────────────────────────────────────────────────────────
  useEffect(() => {
    if (isFreeText) return
    if (selected) return // already selected — don't re-search
    if (!debouncedQuery.trim()) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setResults([])
      setIsOpen(false)
      return
    }

    let cancelled = false
    setIsLoading(true)

    searchDrugs(debouncedQuery)
      .then(data => {
        if (!cancelled) {
          setResults(data)
          setIsOpen(data.length > 0)
        }
      })
      .catch(() => {
        if (!cancelled) setResults([])
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [debouncedQuery, isFreeText, selected])

  // ── Close dropdown on outside click ───────────────────────────────────────
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  // ── Handlers ───────────────────────────────────────────────────────────────
  const handleQueryChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value
    setQuery(val)
    setSelected(null) // clear previous selection when typing again
    onChange(null)
  }, [onChange])

  const handleSelect = useCallback((drug: DrugCatalogEntryDto) => {
    setSelected(drug)
    setQuery(drug.displayName)
    setIsOpen(false)
    setResults([])
    onChange({ mode: 'catalog', drug })
  }, [onChange])

  const handleFreeTextChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value
    setFreeText(val)
    onChange(val.trim() ? { mode: 'free-text', text: val } : null)
  }, [onChange])

  const handleToggleFreeText = useCallback(() => {
    const next = !isFreeText
    setIsFreeText(next)
    // Reset both modes when toggling
    setQuery('')
    setSelected(null)
    setFreeText('')
    setResults([])
    setIsOpen(false)
    onChange(null)
  }, [isFreeText, onChange])

  const handleInputKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      setIsOpen(false)
    }
    if (e.key === 'Enter' && results.length > 0) {
      e.preventDefault()
      handleSelect(results[0])
    }
  }, [results, handleSelect])

  // ── Render ─────────────────────────────────────────────────────────────────
  const displayLabel = label ?? t('label')

  return (
    <div className="space-y-2" ref={containerRef}>
      {/* Label row */}
      <div className="flex items-center justify-between">
        <Label htmlFor={isFreeText ? 'drug-selector-free-text-input' : 'drug-selector-input'} className="text-[13px] font-semibold text-[#061e44]">
          {displayLabel}
          {required && <span className="text-destructive ml-1">*</span>}
        </Label>
        <button
          type="button"
          data-testid="drug-selector-free-text-toggle"
          onClick={handleToggleFreeText}
          className="text-[12px] text-[#303ef5] underline underline-offset-2 hover:text-[#2530c4] transition-colors font-medium"
          aria-pressed={isFreeText}
        >
          {isFreeText ? t('use_catalog') : t('use_free_text')}
        </button>
      </div>

      {isFreeText ? (
        /* ── Free-text mode ─────────────────────────────────────── */
        <Input
          id="drug-selector-free-text-input"
          data-testid="drug-selector-free-text-input"
          value={freeText}
          onChange={handleFreeTextChange}
          placeholder={t('free_text_placeholder')}
          aria-label={t('free_text_label')}
          className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50"
        />
      ) : (
        /* ── Catalog autocomplete mode ──────────────────────────── */
        <div className="relative">
          <Input
            id="drug-selector-input"
            data-testid="drug-selector-input"
            value={query}
            onChange={handleQueryChange}
            onKeyDown={handleInputKeyDown}
            onFocus={() => {
              if (results.length > 0 && !selected) setIsOpen(true)
            }}
            placeholder={t('search_placeholder')}
            autoComplete="off"
            aria-autocomplete="list"
            aria-expanded={isOpen}
            aria-controls="drug-selector-listbox"
            aria-label={t('label')}
            className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50"
          />

          {/* Skeleton rows while loading */}
          {isLoading && (
            <div
              className="absolute z-50 mt-1 w-full rounded-xl border border-border/80 bg-popover shadow-lg p-2 space-y-2"
              aria-busy="true"
              data-testid="drug-selector-loading"
            >
              {[1, 2, 3].map(i => (
                <Skeleton key={i} className="h-10 w-full" />
              ))}
            </div>
          )}

          {/* Results dropdown */}
          {!isLoading && isOpen && results.length > 0 && (
            <ul
              id="drug-selector-listbox"
              role="listbox"
              aria-label={t('results_label')}
              className="absolute z-50 mt-1 w-full rounded-xl border border-border/80 bg-popover text-popover-foreground shadow-lg max-h-64 overflow-y-auto"
              data-testid="drug-selector-results"
            >
              {results.map(drug => (
                <li
                  key={drug.id}
                  role="option"
                  aria-selected={selected?.id === drug.id}
                  data-testid={`drug-selector-option-${drug.id}`}
                  onClick={() => handleSelect(drug)}
                  onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); handleSelect(drug) } }}
                  tabIndex={0}
                  className="flex items-start justify-between px-3 py-2.5 cursor-pointer hover:bg-[#f4f6f9] transition-colors focus:outline-none focus:bg-[#f4f6f9] rounded-lg"
                >
                  <div className="flex flex-col min-w-0">
                    <span className="font-semibold text-[13px] text-[#061e44] truncate">
                      {drug.displayName}
                    </span>
                    <span className="text-[12px] text-muted-foreground truncate">
                      {drug.innName}
                    </span>
                  </div>
                  <div className="flex flex-col items-end gap-1 shrink-0 ml-2">
                    <Badge variant="secondary" className="text-[10px] font-bold uppercase tracking-wider whitespace-nowrap rounded-md">
                      {drug.category}
                    </Badge>
                    {drug.requiresPrescription && (
                      <span className="text-xs text-destructive whitespace-nowrap">
                        {t('rx_required')}
                      </span>
                    )}
                  </div>
                </li>
              ))}
            </ul>
          )}

          {/* No results state */}
          {!isLoading && isOpen && results.length === 0 && query.trim() && (
            <div
              className="absolute z-50 mt-1 w-full rounded-xl border border-border/80 bg-popover text-popover-foreground shadow-lg px-3 py-4 text-[13px] text-muted-foreground text-center"
              data-testid="drug-selector-no-results"
            >
              {t('no_results')}
            </div>
          )}

          {/* Selected drug summary */}
          {selected && !isOpen && (
            <p className="mt-1 text-xs text-muted-foreground" data-testid="drug-selector-selected-summary">
              {selected.innName} &mdash; {selected.commonDosage}
            </p>
          )}
        </div>
      )}
    </div>
  )
}

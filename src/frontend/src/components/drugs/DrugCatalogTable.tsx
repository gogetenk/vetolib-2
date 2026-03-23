'use client'

import { useState, useMemo } from 'react'
import { useTranslations } from 'next-intl'
import { Search, Eye, ChevronUp, ChevronDown } from 'lucide-react'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { DrugCatalogEntryDto, DrugCategory } from '@/lib/api/drugs'

interface DrugCatalogTableProps {
  drugs: DrugCatalogEntryDto[]
  onView: (drug: DrugCatalogEntryDto) => void
  searchQuery: string
  onSearchChange: (value: string) => void
  categoryFilter: string
  onCategoryFilterChange: (value: string) => void
  speciesFilter: string
  onSpeciesFilterChange: (value: string) => void
}

type SortField = 'displayName' | 'category' | 'innName'
type SortDir = 'asc' | 'desc'

function SortIcon({ field, sortField, sortDir }: { field: SortField; sortField: SortField; sortDir: SortDir }) {
  if (sortField !== field) return null
  return sortDir === 'asc' ? (
    <ChevronUp className="inline h-3 w-3 ml-1" />
  ) : (
    <ChevronDown className="inline h-3 w-3 ml-1" />
  )
}

const CATEGORIES: DrugCategory[] = [
  'Antibiotic',
  'Antiparasitic',
  'AntiInflammatory',
  'Analgesic',
  'Vaccine',
  'Antifungal',
  'Cardiac',
  'Dermatological',
  'Ophthalmic',
  'Hormonal',
  'Other',
]

const SPECIES = ['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Camel', 'Exotic']

export function DrugCatalogTable({
  drugs,
  onView,
  searchQuery,
  onSearchChange,
  categoryFilter,
  onCategoryFilterChange,
  speciesFilter,
  onSpeciesFilterChange,
}: DrugCatalogTableProps) {
  const t = useTranslations('drugs')
  const [sortField, setSortField] = useState<SortField>('displayName')
  const [sortDir, setSortDir] = useState<SortDir>('asc')

  const sorted = useMemo(() => {
    return [...drugs].sort((a, b) => {
      const aVal = a[sortField].toLowerCase()
      const bVal = b[sortField].toLowerCase()
      return sortDir === 'asc' ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal)
    })
  }, [drugs, sortField, sortDir])

  function handleSort(field: SortField) {
    if (sortField === field) {
      setSortDir(prev => (prev === 'asc' ? 'desc' : 'asc'))
    } else {
      setSortField(field)
      setSortDir('asc')
    }
  }

  function getCategoryBadge(category: string) {
    const colorMap: Record<string, string> = {
      Antibiotic: 'bg-blue-50 text-blue-700 border-blue-200',
      Antiparasitic: 'bg-purple-50 text-purple-700 border-purple-200',
      AntiInflammatory: 'bg-orange-50 text-orange-700 border-orange-200',
      Analgesic: 'bg-rose-50 text-rose-700 border-rose-200',
      Vaccine: 'bg-success/10 text-success border-success/25',
      Antifungal: 'bg-amber-50 text-amber-700 border-amber-200',
      Cardiac: 'bg-red-50 text-red-700 border-red-200',
      Dermatological: 'bg-teal-50 text-teal-700 border-teal-200',
      Ophthalmic: 'bg-cyan-50 text-cyan-700 border-cyan-200',
      Hormonal: 'bg-pink-50 text-pink-700 border-pink-200',
      Other: 'bg-gray-50 text-gray-700 border-gray-200',
    }
    const base = 'rounded-md text-[10px] font-bold uppercase tracking-wider border'
    return (
      <Badge className={`${base} ${colorMap[category] ?? colorMap.Other}`}>
        {t(`categories.${category}`)}
      </Badge>
    )
  }

  return (
    <div className="space-y-4" data-testid="drug-catalog-table-container">
      {/* Filters */}
      <div className="flex flex-wrap gap-3" data-testid="drug-filters">
        <div className="relative">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            data-testid="drug-search"
            placeholder={t('search_placeholder')}
            className="w-64 pl-9 bg-white border-border/80 rounded-xl h-10 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50 focus:shadow-md transition-shadow"
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
          />
        </div>
        <Select
          value={categoryFilter}
          onValueChange={(v) => onCategoryFilterChange(v ?? 'all')}
        >
          <SelectTrigger className="w-48 rounded-xl border-border/80 text-[13px]" data-testid="filter-drug-category-trigger">
            <SelectValue placeholder={t('filters.all_categories')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all" data-testid="filter-drug-category-all">
              {t('filters.all_categories')}
            </SelectItem>
            {CATEGORIES.map(cat => (
              <SelectItem
                key={cat}
                value={cat}
                data-testid={`filter-drug-category-${cat.toLowerCase()}`}
              >
                {t(`categories.${cat}`)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          value={speciesFilter}
          onValueChange={(v) => onSpeciesFilterChange(v ?? 'all')}
        >
          <SelectTrigger className="w-40 rounded-xl border-border/80 text-[13px]" data-testid="filter-drug-species-trigger">
            <SelectValue placeholder={t('filters.all_species')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all" data-testid="filter-drug-species-all">
              {t('filters.all_species')}
            </SelectItem>
            {SPECIES.map(sp => (
              <SelectItem
                key={sp}
                value={sp}
                data-testid={`filter-drug-species-${sp.toLowerCase()}`}
              >
                {sp}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Mobile card layout */}
      {sorted.length > 0 && (
        <div className="md:hidden space-y-3" data-testid="drug-cards">
          {sorted.map(drug => (
            <div
              key={drug.id}
              className="bg-white border border-border/80 rounded-xl p-4 shadow-sm transition-all duration-200 ease-in-out hover:-translate-y-0.5 hover:shadow-md cursor-pointer"
              onClick={() => onView(drug)}
              data-testid={`drug-card-${drug.id}`}
            >
              <div className="flex items-start justify-between gap-2">
                <div className="min-w-0 flex-1">
                  <p className="text-[14px] font-bold text-foreground">{drug.displayName}</p>
                  <p className="text-[12px] text-muted-foreground italic">{drug.innName}</p>
                </div>
                {getCategoryBadge(drug.category)}
              </div>
              <p className="mt-2 text-[13px] text-muted-foreground line-clamp-2">{drug.commonDosage}</p>
              <div className="mt-2 flex items-center gap-2">
                {drug.requiresPrescription && (
                  <Badge className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-amber-50 text-amber-700 border-amber-200">
                    {t('prescription_required')}
                  </Badge>
                )}
                {drug.contraindicatedSpecies.length > 0 && (
                  <Badge className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-red-50 text-red-700 border-red-200">
                    {t('has_contraindications')}
                  </Badge>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Desktop table */}
      {sorted.length === 0 ? (
        <p
          className="py-12 text-center text-sm text-muted-foreground animate-in fade-in duration-300"
          data-testid="drug-catalog-empty"
        >
          {t('no_drugs')}
        </p>
      ) : (
        <div className="hidden md:block bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden" data-testid="drug-catalog-table">
          <Table>
            <TableHeader>
              <TableRow className="bg-muted hover:bg-muted border-b border-border/50">
                <TableHead
                  className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground cursor-pointer select-none"
                  onClick={() => handleSort('displayName')}
                  data-testid="th-drug-name"
                >
                  {t('columns.name')}<SortIcon field="displayName" sortField={sortField} sortDir={sortDir} />
                </TableHead>
                <TableHead
                  className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground cursor-pointer select-none"
                  onClick={() => handleSort('innName')}
                  data-testid="th-drug-inn"
                >
                  {t('columns.inn_name')}<SortIcon field="innName" sortField={sortField} sortDir={sortDir} />
                </TableHead>
                <TableHead
                  className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground cursor-pointer select-none"
                  onClick={() => handleSort('category')}
                  data-testid="th-drug-category"
                >
                  {t('columns.category')}<SortIcon field="category" sortField={sortField} sortDir={sortDir} />
                </TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-drug-dosage">
                  {t('columns.dosage')}
                </TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-drug-species">
                  {t('columns.species')}
                </TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-drug-rx">
                  {t('columns.prescription')}
                </TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-drug-actions">
                  {t('columns.actions')}
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sorted.map(drug => (
                <TableRow
                  key={drug.id}
                  data-testid={`drug-row-${drug.id}`}
                  className="group hover:bg-muted/50 border-border/30 transition-colors"
                >
                  <TableCell className="py-3 px-4 text-[13px] font-semibold text-foreground" data-testid={`drug-name-${drug.id}`}>
                    {drug.displayName}
                  </TableCell>
                  <TableCell className="py-3 px-4 text-[13px] text-muted-foreground italic" data-testid={`drug-inn-${drug.id}`}>
                    {drug.innName}
                  </TableCell>
                  <TableCell className="py-3 px-4" data-testid={`drug-category-${drug.id}`}>
                    {getCategoryBadge(drug.category)}
                  </TableCell>
                  <TableCell className="py-3 px-4 text-[13px] text-muted-foreground max-w-[200px] truncate" data-testid={`drug-dosage-${drug.id}`}>
                    {drug.commonDosage}
                  </TableCell>
                  <TableCell className="py-3 px-4" data-testid={`drug-species-${drug.id}`}>
                    <div className="flex flex-wrap gap-1">
                      {drug.dosageGuidelines.map(g => (
                        <span
                          key={g.species}
                          className="inline-block rounded bg-slate-100 px-1.5 py-0.5 text-[10px] font-medium text-slate-600"
                        >
                          {g.species}
                        </span>
                      ))}
                    </div>
                  </TableCell>
                  <TableCell className="py-3 px-4" data-testid={`drug-rx-${drug.id}`}>
                    {drug.requiresPrescription ? (
                      <Badge className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-amber-50 text-amber-700 border-amber-200">
                        Rx
                      </Badge>
                    ) : (
                      <Badge className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-success/10 text-success border-success/25">
                        OTC
                      </Badge>
                    )}
                  </TableCell>
                  <TableCell className="py-3 px-4">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => onView(drug)}
                      data-testid={`btn-view-drug-${drug.id}`}
                      aria-label={`${t('actions.view')} ${drug.displayName}`}
                      className="rounded-xl border-border/80 hover:bg-muted opacity-0 group-hover:opacity-100 transition-all duration-200"
                    >
                      <Eye className="h-3.5 w-3.5 me-1" />
                      {t('actions.view')}
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  )
}

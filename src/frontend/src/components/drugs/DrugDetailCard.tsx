'use client'

import { useTranslations } from 'next-intl'
import { Badge } from '@/components/ui/badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import type { DrugCatalogEntryDto } from '@/lib/api/drugs'

interface DrugDetailCardProps {
  drug: DrugCatalogEntryDto
}

export function DrugDetailCard({ drug }: DrugDetailCardProps) {
  const t = useTranslations('drugs')

  return (
    <div className="space-y-6" data-testid="drug-detail-card">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <h2 className="text-[18px] font-bold text-foreground" data-testid="drug-detail-name">
            {drug.displayName}
          </h2>
          <p className="text-[13px] text-muted-foreground italic" data-testid="drug-detail-inn">
            INN: {drug.innName}
          </p>
        </div>
        <div className="flex gap-2">
          <Badge
            className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-blue-50 text-blue-700 border-blue-200"
            data-testid="drug-detail-category"
          >
            {t(`categories.${drug.category}`)}
          </Badge>
          {drug.requiresPrescription ? (
            <Badge
              className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-amber-50 text-amber-700 border-amber-200"
              data-testid="drug-detail-rx"
            >
              {t('prescription_required')}
            </Badge>
          ) : (
            <Badge
              className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-success/10 text-success border-success/25"
              data-testid="drug-detail-otc"
            >
              OTC
            </Badge>
          )}
          {drug.interactionSeverity && (
            <Badge
              className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-orange-50 text-orange-700 border-orange-200"
              data-testid="drug-detail-interaction"
            >
              {t('interaction')}: {drug.interactionSeverity}
            </Badge>
          )}
        </div>
      </div>

      {/* Common Dosage */}
      <div className="bg-white border border-border/80 rounded-xl p-4 shadow-sm" data-testid="drug-detail-dosage-section">
        <h3 className="text-[13px] font-bold uppercase tracking-wider text-foreground mb-2">
          {t('detail.common_dosage')}
        </h3>
        <p className="text-[14px] text-foreground" data-testid="drug-detail-common-dosage">
          {drug.commonDosage}
        </p>
      </div>

      {/* Dosage Guidelines by Species */}
      {drug.dosageGuidelines.length > 0 && (
        <div className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden" data-testid="drug-detail-guidelines-section">
          <div className="px-4 pt-4 pb-2">
            <h3 className="text-[13px] font-bold uppercase tracking-wider text-foreground">
              {t('detail.dosage_guidelines')}
            </h3>
          </div>
          <Table>
            <TableHeader>
              <TableRow className="bg-muted hover:bg-muted border-b border-border/50">
                <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground">
                  {t('detail.species')}
                </TableHead>
                <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground">
                  {t('detail.dose_per_kg')}
                </TableHead>
                <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground">
                  {t('detail.frequency')}
                </TableHead>
                <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground">
                  {t('detail.max_duration')}
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {drug.dosageGuidelines.map((g) => (
                <TableRow key={g.species} className="hover:bg-muted/50 border-border/30" data-testid={`guideline-row-${g.species.toLowerCase()}`}>
                  <TableCell className="text-[13px] font-semibold text-foreground">{g.species}</TableCell>
                  <TableCell className="text-[13px] text-muted-foreground tabular-nums">
                    {g.dosePerKg} {g.unit}/kg
                  </TableCell>
                  <TableCell className="text-[13px] text-muted-foreground">{g.frequency}</TableCell>
                  <TableCell className="text-[13px] text-muted-foreground">
                    {g.maxDurationDays ? `${g.maxDurationDays} ${t('detail.days')}` : t('detail.ongoing')}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Contraindications */}
      {drug.contraindicatedSpecies.length > 0 && (
        <div className="bg-red-50/50 border border-red-200 rounded-xl p-4" data-testid="drug-detail-contraindications-section">
          <h3 className="text-[13px] font-bold uppercase tracking-wider text-red-800 mb-3">
            {t('detail.contraindications')}
          </h3>
          <div className="space-y-2">
            {drug.contraindicatedSpecies.map((c) => (
              <div key={c.species} className="flex items-start gap-2" data-testid={`contraindication-${c.species.toLowerCase()}`}>
                <Badge className="rounded-md text-[10px] font-bold uppercase tracking-wider border bg-red-100 text-red-700 border-red-300 shrink-0">
                  {c.species}
                </Badge>
                <p className="text-[13px] text-red-700">{c.reason}</p>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

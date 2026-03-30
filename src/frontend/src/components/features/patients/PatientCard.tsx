'use client'

import Link from 'next/link'
import { useLocale } from 'next-intl'
import { Card, CardContent } from '@/components/ui/card'
import { LtrText } from '@/components/ui/ltr-text'
import { SpeciesIcon, getSpeciesColor } from '@/components/features/patients/SpeciesIcon'
import type { PatientDto } from '@/lib/api/patients'

function formatDate(dateStr: string | null): string {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

interface PatientCardProps {
  patient: PatientDto
}

export function PatientCard({ patient }: PatientCardProps) {
  const locale = useLocale()
  const speciesColor = getSpeciesColor(patient.species)

  return (
    <Link
      href={`/${locale}/patients/${patient.id}`}
      data-testid={`patient-card-link-${patient.id}`}
      className="block outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 rounded-xl"
    >
      <Card
        data-testid={`patient-card-${patient.id}`}
        className={`group transition-all duration-300 ease-out hover:-translate-y-0.5 hover:shadow-md bg-card border border-border/80 cursor-pointer overflow-hidden rounded-xl h-full flex flex-col`}
      >
        <CardContent className="p-5 flex-1 flex flex-col">
          <div className="flex items-start justify-between gap-4 mb-4">
            <div className="flex items-center gap-4">
              <div
                className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-full ${speciesColor.bg} ${speciesColor.text}`}
                data-testid={`patient-species-icon-${patient.id}`}
                aria-label={patient.species}
              >
                <SpeciesIcon species={patient.species} className="h-5 w-5" />
              </div>
              <div className="min-w-0 flex flex-col">
                <p
                  className="truncate text-[15px] font-bold text-foreground leading-tight"
                  data-testid={`patient-name-${patient.id}`}
                >
                  {patient.name}
                </p>
                <p className="truncate text-[12px] text-muted-foreground font-medium mt-0.5" data-testid={`patient-breed-${patient.id}`}>
                  {patient.species} — {patient.breed}
                </p>
              </div>
            </div>

            <span className="shrink-0 text-[12px] font-bold text-primary bg-primary/10 px-2 py-0.5 rounded-md" data-testid={`patient-age-${patient.id}`}>
              {patient.ageYears}y
            </span>
          </div>

          <div className="flex items-center gap-2 mb-2">
            <span className="text-[12px] text-muted-foreground font-medium flex items-center gap-1" data-testid={`patient-sex-display-${patient.id}`}>
              {patient.sex === 'Male' || patient.sex === 'Intact Male' ? '\u2642' : patient.sex === 'Female' || patient.sex === 'Intact Female' ? '\u2640' : '\u26A5'}{' '}
              {patient.sex}
            </span>
            {patient.microchipNumber && (
              <span className="text-[11px] text-muted-foreground font-mono bg-muted px-1.5 py-0.5 rounded flex items-center gap-1" data-testid={`patient-microchip-display-${patient.id}`}>
                <svg xmlns="http://www.w3.org/2000/svg" className="h-3 w-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect x="4" y="4" width="16" height="16" rx="2"/><rect x="9" y="9" width="6" height="6"/><line x1="9" y1="1" x2="9" y2="4"/><line x1="15" y1="1" x2="15" y2="4"/><line x1="9" y1="20" x2="9" y2="23"/><line x1="15" y1="20" x2="15" y2="23"/><line x1="20" y1="9" x2="23" y2="9"/><line x1="20" y1="14" x2="23" y2="14"/><line x1="1" y1="9" x2="4" y2="9"/><line x1="1" y1="14" x2="4" y2="14"/></svg>
                {patient.microchipNumber}
              </span>
            )}
          </div>

          <div className="space-y-1 mb-6 flex-1">
            <p data-testid={`patient-owner-${patient.id}`} className="text-[13px] flex items-center gap-1.5 text-foreground font-medium">
              <span className="text-muted-foreground">Owner:</span> {patient.ownerName}
            </p>
            <p data-testid={`patient-owner-phone-${patient.id}`} className="text-[13px] text-muted-foreground font-medium">
              <LtrText>{patient.ownerPhone}</LtrText>
            </p>
          </div>

          <div className="pt-4 border-t border-border/30 flex justify-between items-center text-[12px] text-muted-foreground mt-auto">
            <span data-testid={`patient-last-visit-${patient.id}`}>
              Last visit: <LtrText className="font-semibold text-foreground ml-1">{formatDate(patient.lastVisitDate)}</LtrText>
            </span>
            <span data-testid={`patient-next-appt-${patient.id}`}>
              Next: <LtrText className="font-bold text-primary ml-1">{formatDate(patient.nextAppointmentDate)}</LtrText>
            </span>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}

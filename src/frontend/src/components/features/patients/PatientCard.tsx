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
        className={`group transition-all duration-300 ease-out hover:-translate-y-0.5 hover:shadow-md bg-white border border-border/80 cursor-pointer overflow-hidden rounded-xl h-full flex flex-col`}
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
                  className="truncate text-[15px] font-bold text-[#061e44] leading-tight"
                  data-testid={`patient-name-${patient.id}`}
                >
                  {patient.name}
                </p>
                <p className="truncate text-[12px] text-muted-foreground font-medium mt-0.5" data-testid={`patient-breed-${patient.id}`}>
                  {patient.species} — {patient.breed}
                </p>
              </div>
            </div>

            <span className="shrink-0 text-[12px] font-bold text-[#303ef5] bg-[#eef2fd] px-2 py-0.5 rounded-md" data-testid={`patient-age-${patient.id}`}>
              {patient.ageYears}y {patient.gender}
            </span>
          </div>

          <div className="space-y-1 mb-6 flex-1">
            <p data-testid={`patient-owner-${patient.id}`} className="text-[13px] flex items-center gap-1.5 text-[#061e44] font-medium">
              <span className="text-muted-foreground">Owner:</span> {patient.ownerName}
            </p>
            <p data-testid={`patient-owner-phone-${patient.id}`} className="text-[13px] text-muted-foreground font-medium">
              <LtrText>{patient.ownerPhone}</LtrText>
            </p>
          </div>

          <div className="pt-4 border-t border-border/30 flex justify-between items-center text-[12px] text-muted-foreground mt-auto">
            <span data-testid={`patient-last-visit-${patient.id}`}>
              Last visit: <LtrText className="font-semibold text-[#061e44] ml-1">{formatDate(patient.lastVisitDate)}</LtrText>
            </span>
            <span data-testid={`patient-next-appt-${patient.id}`}>
              Next: <LtrText className="font-bold text-[#303ef5] ml-1">{formatDate(patient.nextAppointmentDate)}</LtrText>
            </span>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}

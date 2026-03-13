'use client'

import Link from 'next/link'
import { useLocale } from 'next-intl'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { LtrText } from '@/components/ui/ltr-text'
import { SpeciesIcon, getSpeciesColor } from '@/components/features/patients/SpeciesIcon'
import type { PatientDto } from '@/lib/api/patients'

function formatDate(dateStr: string | null): string {
  if (!dateStr) return '\u2014'
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
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
      className="block"
    >
      <Card
        data-testid={`patient-card-${patient.id}`}
        className={`group transition-all duration-200 ease-in-out hover:-translate-y-0.5 hover:shadow-md border-s-4 ${speciesColor.border} cursor-pointer`}
      >
        <CardContent className="p-3">
          <div className="flex items-start justify-between gap-3">
            {/* Species icon + name */}
            <div className="flex items-center gap-2.5">
              <div
                className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-full ${speciesColor.bg} ${speciesColor.text}`}
                data-testid={`patient-species-icon-${patient.id}`}
                aria-label={patient.species}
              >
                <SpeciesIcon species={patient.species} className="h-4.5 w-4.5" />
              </div>
              <div className="min-w-0">
                <p
                  className="font-semibold text-sm text-stone-900 truncate"
                  data-testid={`patient-name-${patient.id}`}
                >
                  {patient.name}
                </p>
                <p className="text-xs text-stone-500" data-testid={`patient-breed-${patient.id}`}>
                  {patient.species} &mdash; {patient.breed}
                </p>
              </div>
            </div>

            {/* Age badge */}
            <Badge variant="secondary" className="shrink-0 text-xs" data-testid={`patient-age-${patient.id}`}>
              {patient.ageYears}y {patient.gender}
            </Badge>
          </div>

          {/* Owner info */}
          <div className="mt-2 space-y-0.5 text-xs text-stone-500">
            <p data-testid={`patient-owner-${patient.id}`}>
              <span className="font-medium text-stone-700">Owner:</span> {patient.ownerName}
            </p>
            <p data-testid={`patient-owner-phone-${patient.id}`}>
              <LtrText>{patient.ownerPhone}</LtrText>
            </p>
          </div>

          {/* Visit info */}
          <div className="mt-2 flex flex-wrap gap-3 text-xs text-stone-400">
            <span data-testid={`patient-last-visit-${patient.id}`}>
              Last visit: <LtrText>{formatDate(patient.lastVisitDate)}</LtrText>
            </span>
            <span data-testid={`patient-next-appt-${patient.id}`}>
              Next: <LtrText>{formatDate(patient.nextAppointmentDate)}</LtrText>
            </span>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}

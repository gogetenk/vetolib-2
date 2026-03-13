'use client'

import Link from 'next/link'
import { useLocale } from 'next-intl'
import { Dog, Cat, Bird, Rabbit, PawPrint } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { LtrText } from '@/components/ui/ltr-text'
import type { PatientDto, Species } from '@/lib/api/patients'

function SpeciesIcon({ species }: { species: Species }) {
  switch (species) {
    case 'Dog':
      return <Dog className="h-5 w-5" aria-label="Dog" />
    case 'Cat':
      return <Cat className="h-5 w-5" aria-label="Cat" />
    case 'Bird':
      return <Bird className="h-5 w-5" aria-label="Bird" />
    case 'Rabbit':
      return <Rabbit className="h-5 w-5" aria-label="Rabbit" />
    default:
      return <PawPrint className="h-5 w-5" aria-label={species} />
  }
}

function formatDate(dateStr: string | null): string {
  if (!dateStr) return '—'
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
  return (
    <Card data-testid={`patient-card-${patient.id}`} className="group transition-all duration-200 ease-in-out hover:-translate-y-0.5 hover:shadow-md">
      <CardContent className="p-4">
        <div className="flex items-start justify-between gap-4">
          {/* Species icon + name */}
          <div className="flex items-center gap-3">
            <div
              className="flex h-10 w-10 items-center justify-center rounded-full bg-muted text-muted-foreground"
              data-testid={`patient-species-icon-${patient.id}`}
              aria-label={patient.species}
            >
              <SpeciesIcon species={patient.species} />
            </div>
            <div>
              <p
                className="font-semibold text-base"
                data-testid={`patient-name-${patient.id}`}
              >
                {patient.name}
              </p>
              <p className="text-sm text-muted-foreground" data-testid={`patient-breed-${patient.id}`}>
                {patient.species} — {patient.breed}
              </p>
            </div>
          </div>

          {/* Age badge */}
          <Badge variant="secondary" data-testid={`patient-age-${patient.id}`}>
            {patient.ageYears}y {patient.gender}
          </Badge>
        </div>

        {/* Owner info */}
        <div className="mt-3 space-y-1 text-sm text-muted-foreground">
          <p data-testid={`patient-owner-${patient.id}`}>
            <span className="font-medium text-foreground">Owner:</span> {patient.ownerName}
          </p>
          <p data-testid={`patient-owner-phone-${patient.id}`}>
            <LtrText>{patient.ownerPhone}</LtrText>
          </p>
        </div>

        {/* Visit info */}
        <div className="mt-3 flex flex-wrap gap-4 text-xs text-muted-foreground">
          <span data-testid={`patient-last-visit-${patient.id}`}>
            Last visit: <LtrText>{formatDate(patient.lastVisitDate)}</LtrText>
          </span>
          <span data-testid={`patient-next-appt-${patient.id}`}>
            Next appt: <LtrText>{formatDate(patient.nextAppointmentDate)}</LtrText>
          </span>
        </div>

        {/* Action */}
        <div className="mt-4 opacity-0 group-hover:opacity-100 transition-opacity duration-200 ease-in-out">
          <Link href={`/${locale}/patients/${patient.id}`}>
            <Button
              variant="outline"
              size="sm"
              data-testid={`view-record-btn-${patient.id}`}
            >
              View Record
            </Button>
          </Link>
        </div>
      </CardContent>
    </Card>
  )
}

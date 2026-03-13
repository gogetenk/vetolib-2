'use client'

import { useEffect, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { MedicalRecordForm } from '@/components/features/patients/MedicalRecordForm'
import { getPatient } from '@/lib/api/patients'
import type { PatientDto } from '@/lib/api/patients'
import { useRole } from '@/hooks/use-role'

export default function NewMedicalRecordPage() {
  const { id } = useParams<{ id: string }>()
  const router = useRouter()
  const role = useRole()
  const [patient, setPatient] = useState<PatientDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    if (role && role !== 'VET') {
      router.replace(`/patients/${id}`)
      return
    }

    if (!id) return
    getPatient(id)
      .then(setPatient)
      .catch(() => setPatient(null))
      .finally(() => setIsLoading(false))
  }, [id, role, router])

  if (isLoading) {
    return (
      <div data-testid="new-record-loading" className="space-y-4">
        <div className="h-8 w-32 rounded bg-muted animate-pulse" />
        <div className="h-96 rounded-lg bg-muted animate-pulse" />
      </div>
    )
  }

  if (!patient) {
    return (
      <div data-testid="new-record-patient-not-found" className="py-12 text-center">
        <p className="text-muted-foreground">Patient not found.</p>
        <Link href="../../..">
          <Button variant="outline" className="mt-4" data-testid="back-to-patients-fallback-btn">
            Back to Patients
          </Button>
        </Link>
      </div>
    )
  }

  return (
    <div className="space-y-6" data-testid="new-medical-record-page">
      <Link href={`../../${id}`}>
        <Button
          variant="ghost"
          size="sm"
          data-testid="back-to-patient-btn"
          className="-ms-2"
        >
          <ArrowLeft className="h-4 w-4 me-1" />
          {patient.name}&apos;s record
        </Button>
      </Link>

      <MedicalRecordForm
        patientId={id}
        patientName={patient.name}
        patientSpecies={patient.species}
        patientWeightKg={patient.weightKg}
      />
    </div>
  )
}

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
      <div data-testid="new-record-loading" className="space-y-4 p-6 lg:p-8">
        <div className="h-8 w-32 rounded-xl bg-muted animate-pulse" />
        <div className="h-96 rounded-xl bg-muted animate-pulse" />
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
    <div className="p-6 lg:p-8 space-y-6 max-w-5xl mx-auto" data-testid="new-medical-record-page">
      <Link href={`../../${id}`}>
        <Button
          variant="ghost"
          size="sm"
          data-testid="back-to-patient-btn"
          className="-ms-2 group/back text-muted-foreground hover:text-[#061e44]"
        >
          <ArrowLeft className="h-4 w-4 me-1 transition-transform duration-200 ease-in-out group-hover/back:-translate-x-0.5 rtl:group-hover/back:translate-x-0.5" />
          {patient.name}&apos;s record
        </Button>
      </Link>

      <div>
        <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="new-record-title">
          <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
          New Medical Record
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ml-3">
          Record consultation details for {patient.name}.
        </p>
      </div>

      <MedicalRecordForm
        patientId={id}
        patientName={patient.name}
        patientSpecies={patient.species}
        patientWeightKg={patient.weightKg}
      />
    </div>
  )
}

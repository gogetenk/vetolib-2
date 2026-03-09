import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { PatientForm } from '@/components/features/patients/PatientForm'

export default function NewPatientPage() {
  return (
    <div className="space-y-6" data-testid="new-patient-page">
      <Button
        render={<Link href="/patients" />}
        variant="ghost"
        size="sm"
        data-testid="back-to-patients-btn"
        className="-ml-2"
      >
        <ArrowLeft className="h-4 w-4 mr-1" />
        Patients
      </Button>

      <div>
        <h1 className="text-2xl font-bold" data-testid="new-patient-title">
          New Patient
        </h1>
        <p className="text-muted-foreground text-sm mt-1">
          Register a new patient and their owner.
        </p>
      </div>

      <PatientForm />
    </div>
  )
}

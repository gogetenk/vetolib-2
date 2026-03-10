import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { PatientForm } from '@/components/features/patients/PatientForm'
import { getTranslations } from 'next-intl/server'

export default async function NewPatientPage() {
  const t = await getTranslations('patients')

  return (
    <div className="space-y-6" data-testid="new-patient-page">
      <Button
        render={<Link href="../patients" />}
        variant="ghost"
        size="sm"
        data-testid="back-to-patients-btn"
        className="-ms-2"
      >
        <ArrowLeft className="h-4 w-4 me-1" />
        {t('title')}
      </Button>

      <div>
        <h1 className="text-2xl font-bold" data-testid="new-patient-title">
          {t('form.title')}
        </h1>
        <p className="text-muted-foreground text-sm mt-1">
          Register a new patient and their owner.
        </p>
      </div>

      <PatientForm />
    </div>
  )
}

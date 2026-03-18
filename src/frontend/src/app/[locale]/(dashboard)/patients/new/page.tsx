import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { PatientForm } from '@/components/features/patients/PatientForm'
import { getTranslations } from 'next-intl/server'

export default async function NewPatientPage() {
  const t = await getTranslations('patients')

  return (
    <div className="p-6 lg:p-8 space-y-6 max-w-5xl mx-auto" data-testid="new-patient-page">
      <Link href="../patients">
        <Button
          variant="ghost"
          size="sm"
          data-testid="back-to-patients-btn"
          className="-ms-2 group/back text-muted-foreground hover:text-[#061e44]"
        >
          <ArrowLeft className="h-4 w-4 me-1 transition-transform duration-200 ease-in-out group-hover/back:-translate-x-0.5 rtl:group-hover/back:translate-x-0.5" />
          {t('title')}
        </Button>
      </Link>

      <div>
        <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="new-patient-title">
          <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
          {t('form.title')}
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ml-3">
          Register a new patient and their owner.
        </p>
      </div>

      <PatientForm />
    </div>
  )
}

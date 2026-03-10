import { AppointmentForm } from '@/components/features/appointments/AppointmentForm'
import { getTranslations } from 'next-intl/server'

export default async function NewAppointmentPage() {
  const t = await getTranslations('appointments')

  return (
    <div className="space-y-6" data-testid="new-appointment-page">
      <h1 className="text-2xl font-bold" data-testid="new-appointment-title">
        {t('form.title')}
      </h1>
      <AppointmentForm />
    </div>
  )
}

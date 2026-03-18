import { AppointmentForm } from '@/components/features/appointments/AppointmentForm'
import { getTranslations } from 'next-intl/server'

export default async function NewAppointmentPage() {
  const t = await getTranslations('appointments')

  return (
    <div className="p-6 lg:p-8 space-y-6 max-w-5xl mx-auto" data-testid="new-appointment-page">
      <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="new-appointment-title">
        <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
        {t('form.title')}
      </h1>
      <AppointmentForm />
    </div>
  )
}

import { AppointmentDetailLoader } from '@/components/features/appointments/AppointmentDetailLoader'
import { getTranslations } from 'next-intl/server'

interface Props {
  params: Promise<{ id: string }>
}

export default async function AppointmentDetailPage({ params }: Props) {
  const { id } = await params
  const t = await getTranslations('appointments')

  return (
    <div className="space-y-6" data-testid="appointment-detail-page">
      <h1 className="text-2xl font-bold" data-testid="detail-page-title">
        {t('detail.back')}
      </h1>
      <AppointmentDetailLoader id={id} />
    </div>
  )
}

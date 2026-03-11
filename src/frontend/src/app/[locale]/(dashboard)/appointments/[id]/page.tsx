import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
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
      <Button
        render={<Link href="../appointments" />}
        variant="ghost"
        size="sm"
        data-testid="back-to-appointments-btn"
        className="-ms-2"
      >
        <ArrowLeft className="h-4 w-4 me-1" />
        {t('title')}
      </Button>
      <AppointmentDetailLoader id={id} />
    </div>
  )
}

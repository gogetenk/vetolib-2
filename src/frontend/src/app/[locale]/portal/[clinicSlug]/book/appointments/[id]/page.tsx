import { AppointmentDetail } from '@/components/features/portal/booking/AppointmentDetail'
import { PortalLayout } from '@/components/features/portal/PortalLayout'

interface Props {
  params: Promise<{ locale: string; clinicSlug: string; id: string }>
}

export default async function AppointmentDetailPage({ params }: Props) {
  const { clinicSlug, id } = await params
  const clinicName = clinicSlug
    .split('-')
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')

  return (
    <PortalLayout clinicName={clinicName}>
      <AppointmentDetail appointmentId={id} />
    </PortalLayout>
  )
}

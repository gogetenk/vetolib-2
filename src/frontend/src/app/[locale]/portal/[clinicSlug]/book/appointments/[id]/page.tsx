import { AppointmentDetail } from '@/components/features/portal/booking/AppointmentDetail'

interface Props {
  params: Promise<{ id: string }>
}

export default async function AppointmentDetailPage({ params }: Props) {
  const { id } = await params
  return <AppointmentDetail appointmentId={id} />
}

import { AppointmentDetailLoader } from '@/components/features/appointments/AppointmentDetailLoader'

interface Props {
  params: Promise<{ id: string }>
}

export default async function AppointmentDetailPage({ params }: Props) {
  const { id } = await params
  return (
    <div className="space-y-6" data-testid="appointment-detail-page">
      <h1 className="text-2xl font-bold" data-testid="detail-page-title">
        Appointment Details
      </h1>
      <AppointmentDetailLoader id={id} />
    </div>
  )
}

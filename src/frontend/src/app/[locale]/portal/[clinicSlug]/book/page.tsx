import { BookingLanding } from '@/components/features/portal/booking/BookingLanding'
import { PortalLayout } from '@/components/features/portal/PortalLayout'

interface Props {
  params: Promise<{ locale: string; clinicSlug: string }>
}

export default async function BookingPage({ params }: Props) {
  const { clinicSlug } = await params
  const clinicName = clinicSlug
    .split('-')
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')

  return (
    <PortalLayout clinicName={clinicName}>
      <BookingLanding />
    </PortalLayout>
  )
}

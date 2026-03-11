import { BookingLanding } from '@/components/features/portal/booking/BookingLanding'
import { PortalLayout } from '@/components/features/portal/PortalLayout'

interface BookingPageProps {
  params: Promise<{ locale: string; clinicSlug: string }>
}

export default async function BookingPage({ params }: BookingPageProps) {
  const { clinicSlug } = await params
  // Derive a human-readable name from slug (e.g. "desert-paws" → "Desert Paws")
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

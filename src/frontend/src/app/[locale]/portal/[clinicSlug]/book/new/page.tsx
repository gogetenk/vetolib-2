import { BookingWizard } from '@/components/features/portal/booking/BookingWizard'

interface Props {
  params: Promise<{ locale: string; clinicSlug: string }>
}

export default async function BookNewAppointmentPage({ params }: Props) {
  const { locale, clinicSlug } = await params

  return (
    <div className="mx-auto max-w-lg px-4 py-8">
      {/* Page header */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Book an Appointment</h1>
        <p className="mt-1 text-sm text-gray-500">
          Schedule a visit for your pet in a few easy steps.
        </p>
      </div>

      {/* Wizard */}
      <BookingWizard locale={locale} clinicSlug={clinicSlug} />
    </div>
  )
}

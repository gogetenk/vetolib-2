'use client'

import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { CalendarPlus, ClipboardList } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'

export function BookingLanding() {
  const t = useTranslations('portal.booking.appointments')
  const router = useRouter()
  const params = useParams()
  const locale = params.locale as string
  const clinicSlug = params.clinicSlug as string

  function goToMyAppointments() {
    router.push(`/${locale}/portal/${clinicSlug}/book/appointments`)
  }

  return (
    <div data-testid="booking-landing" className="space-y-4">
      <h1 className="text-xl font-bold text-gray-900" data-testid="booking-landing-title">
        Desert Paws Veterinary Clinic
      </h1>
      <p className="text-gray-600 text-sm">Book an appointment or view your existing appointments.</p>

      <div className="grid gap-3">
        <Card
          className="cursor-pointer hover:shadow-md transition-shadow"
          data-testid="book-new-card"
          role="button"
          tabIndex={0}
        >
          <CardContent className="p-4 flex items-center gap-3">
            <CalendarPlus className="h-8 w-8 text-emerald-600 flex-shrink-0" />
            <div>
              <p className="font-semibold text-gray-900">Book a New Appointment</p>
              <p className="text-sm text-gray-500">Choose a time slot that works for you</p>
            </div>
          </CardContent>
        </Card>

        <Card
          className="cursor-pointer hover:shadow-md transition-shadow"
          data-testid="my-appointments-card"
          role="button"
          tabIndex={0}
          onClick={goToMyAppointments}
          onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') goToMyAppointments() }}
        >
          <CardContent className="p-4 flex items-center gap-3">
            <ClipboardList className="h-8 w-8 text-blue-600 flex-shrink-0" />
            <div>
              <p className="font-semibold text-gray-900">{t('page_title')}</p>
              <p className="text-sm text-gray-500">View and manage your scheduled visits</p>
            </div>
          </CardContent>
        </Card>
      </div>

      <Button
        onClick={goToMyAppointments}
        variant="outline"
        className="w-full"
        data-testid="view-my-appointments-btn"
      >
        {t('page_title')}
      </Button>
    </div>
  )
}

'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { CalendarDays, ListChecks, Clock } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { LtrText } from '@/components/ui/ltr-text'
import { format } from 'date-fns'

interface NextAppointment {
  id: string
  scheduledAt: string
  consultationType: string
  petName: string
}

export function BookingLanding() {
  const t = useTranslations('portal.booking')
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const [nextAppointment] = useState<NextAppointment | null>(() => {
    if (typeof window === 'undefined') return null
    try {
      const stored = sessionStorage.getItem('portal_next_appointment')
      return stored ? JSON.parse(stored) : null
    } catch {
      return null
    }
  })

  const base = `/${params.locale}/portal/${params.clinicSlug}/book`

  return (
    <div className="space-y-6" data-testid="booking-landing">
      <div>
        <h1
          className="text-2xl font-bold text-gray-900"
          data-testid="booking-landing-title"
        >
          {t('landing.title')}
        </h1>
        <p className="text-gray-600 text-sm mt-1" data-testid="booking-landing-subtitle">
          {t('landing.subtitle')}
        </p>
      </div>

      {/* Next Appointment preview card */}
      {nextAppointment && (
        <Card className="border-emerald-200 bg-emerald-50/50" data-testid="next-appointment-card">
          <CardContent className="flex items-center gap-4 py-4">
            <div className="flex items-center justify-center w-12 h-12 rounded-full bg-emerald-100">
              <Clock className="w-6 h-6 text-emerald-600" />
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium text-emerald-700 uppercase tracking-wide">
                {t('landing.nextAppointment')}
              </p>
              <p className="text-sm font-semibold text-gray-900 mt-0.5">
                <LtrText>{format(new Date(nextAppointment.scheduledAt), 'EEEE, dd MMM yyyy - HH:mm')}</LtrText>
              </p>
              <div className="flex items-center gap-2 mt-1">
                <span className="text-sm text-gray-700">{nextAppointment.petName}</span>
                <Badge variant="secondary" className="text-xs">{nextAppointment.consultationType}</Badge>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {/* My Appointments card */}
        <div
          role="button"
          tabIndex={0}
          data-testid="booking-my-appointments-card"
          className="flex flex-col gap-3 rounded-xl border border-gray-200 bg-white p-6 shadow-sm hover:border-emerald-400 hover:shadow-md transition-all cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 min-h-[44px]"
          onClick={() => router.push(`${base}/appointments`)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              router.push(`${base}/appointments`)
            } else if (e.key === ' ') {
              e.preventDefault()
              router.push(`${base}/appointments`)
            }
          }}
        >
          <div className="flex items-center justify-center w-12 h-12 rounded-full bg-emerald-50">
            <ListChecks className="w-7 h-7 text-emerald-600" />
          </div>
          <div>
            <h2 className="text-base font-semibold text-gray-900">
              {t('myAppointments.title')}
            </h2>
            <p className="text-sm text-gray-600 mt-0.5">
              {t('landing.myAppointmentsSubtitle')}
            </p>
          </div>
        </div>

        {/* Book New Appointment card -- visual only, no action yet */}
        <div
          data-testid="booking-new-appointment-card"
          className="flex flex-col gap-3 rounded-xl border border-gray-200 bg-white p-6 shadow-sm opacity-60 cursor-not-allowed"
          aria-disabled="true"
        >
          <div className="flex items-center justify-center w-12 h-12 rounded-full bg-blue-50">
            <CalendarDays className="w-7 h-7 text-blue-600" />
          </div>
          <div>
            <h2 className="text-base font-semibold text-gray-900">
              {t('wizard.step1Title')}
            </h2>
            <p className="text-sm text-gray-600 mt-0.5">
              {t('landing.bookNewSubtitle')}
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

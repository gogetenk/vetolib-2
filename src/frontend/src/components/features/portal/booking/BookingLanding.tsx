'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useParams } from 'next/navigation'
import Link from 'next/link'
import { CalendarDays, Clock, User, Stethoscope, PawPrint, Plus, MessageCircle, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { listBookingAppointments } from '@/lib/api/booking'
import { ApiError } from '@/lib/api/client'
import type { BookingAppointmentDto } from '@/lib/api/booking-types'

const UPCOMING_STATUSES = new Set(['Scheduled', 'CheckedIn'])

const STATUS_COLORS: Record<string, string> = {
  Scheduled: 'bg-emerald-100 text-emerald-800',
  CheckedIn: 'bg-blue-100 text-blue-800',
}

export function BookingLanding() {
  const t = useTranslations('portal.booking.landing')
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const [appointments, setAppointments] = useState<BookingAppointmentDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [expired, setExpired] = useState(false)

  useEffect(() => {
    listBookingAppointments()
      .then((data) => {
        setAppointments(data.filter((a) => UPCOMING_STATUSES.has(a.status)))
      })
      .catch((err) => {
        if (err instanceof ApiError && err.status === 401) {
          setExpired(true)
        }
      })
      .finally(() => setIsLoading(false))
  }, [])

  function formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-AE', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      timeZone: 'Asia/Dubai',
    })
  }

  function formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('en-AE', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: true,
      timeZone: 'Asia/Dubai',
    })
  }

  return (
    <div className="space-y-6" data-testid="booking-landing">
      {/* Upcoming Appointments Section */}
      <section>
        <h2 className="text-lg font-semibold text-gray-900 mb-3" data-testid="upcoming-appointments">
          {t('upcoming_appointments')}
        </h2>

        {isLoading ? (
          <div className="space-y-3" data-testid="booking-loading">
            {[1, 2].map((i) => (
              <div key={i} className="h-24 rounded-lg bg-gray-200 animate-pulse" />
            ))}
            <p className="text-sm text-gray-500 text-center">{t('loading')}</p>
          </div>
        ) : expired ? (
          <div className="text-center py-8 text-gray-500" data-testid="booking-expired">
            <MessageCircle className="w-10 h-10 mx-auto mb-2 text-gray-300" />
            <p className="text-sm">{t('no_upcoming')}</p>
          </div>
        ) : appointments.length === 0 ? (
          <div className="text-center py-8 text-gray-500" data-testid="booking-empty">
            <CalendarDays className="w-10 h-10 mx-auto mb-2 text-gray-300" />
            <p className="text-sm">{t('no_upcoming')}</p>
          </div>
        ) : (
          <ul className="space-y-3">
            {appointments.map((appt) => (
              <li key={appt.id}>
                <Link
                  href={`/${params.locale}/portal/${params.clinicSlug}/book/appointments/${appt.id}`}
                  data-testid={`appointment-card-${appt.id}`}
                  className="block"
                >
                  <Card className="hover:border-emerald-300 hover:shadow-sm transition-all cursor-pointer">
                    <CardContent className="px-4 py-3">
                      <div className="flex items-start justify-between gap-2">
                        <div className="flex-1 min-w-0 space-y-1.5">
                          {/* Status badge */}
                          <span
                            className={`inline-block text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_COLORS[appt.status] ?? 'bg-gray-100 text-gray-600'}`}
                          >
                            {t(`status.${appt.status}`)}
                          </span>

                          {/* Date & Time */}
                          <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-gray-700">
                            <span className="flex items-center gap-1">
                              <CalendarDays className="h-3.5 w-3.5 text-emerald-600 flex-shrink-0" aria-hidden="true" />
                              <span>{formatDate(appt.scheduledAt)}</span>
                            </span>
                            <span className="flex items-center gap-1">
                              <Clock className="h-3.5 w-3.5 text-emerald-600 flex-shrink-0" aria-hidden="true" />
                              <span>{formatTime(appt.scheduledAt)}</span>
                            </span>
                          </div>

                          {/* Vet & Type */}
                          <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-gray-600">
                            <span className="flex items-center gap-1">
                              <User className="h-3.5 w-3.5 text-gray-400 flex-shrink-0" aria-hidden="true" />
                              <span>{appt.veterinarianName}</span>
                            </span>
                            <span className="flex items-center gap-1">
                              <Stethoscope className="h-3.5 w-3.5 text-gray-400 flex-shrink-0" aria-hidden="true" />
                              <span>{appt.consultationTypeName}</span>
                            </span>
                          </div>

                          {/* Pet */}
                          <div className="flex items-center gap-1 text-sm text-gray-600">
                            <PawPrint className="h-3.5 w-3.5 text-gray-400 flex-shrink-0" aria-hidden="true" />
                            <span>{appt.petName}</span>
                          </div>
                        </div>
                        <ChevronRight className="h-4 w-4 text-gray-400 flex-shrink-0 mt-1" aria-hidden="true" />
                      </div>
                    </CardContent>
                  </Card>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>

      {/* CTA: Book an appointment */}
      <Link
        href={`/${params.locale}/portal/${params.clinicSlug}/book/new`}
        data-testid="book-appointment-btn"
      >
        <Button className="w-full bg-emerald-600 hover:bg-emerald-700 text-white">
          <Plus className="h-4 w-4 me-2" />
          {t('book_appointment')}
        </Button>
      </Link>

      {/* Contact clinic link */}
      <div className="text-center">
        <Link
          href={`/${params.locale}/portal/${params.clinicSlug}`}
          data-testid="contact-clinic-link"
          className="text-sm text-emerald-600 hover:text-emerald-800 hover:underline"
        >
          {t('contact_clinic')}
        </Link>
      </div>
    </div>
  )
}

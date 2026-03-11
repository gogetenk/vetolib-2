'use client'

import { useState, useEffect } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { CalendarDays, Clock, User, PawPrint, Plus } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { listBookingAppointments } from '@/lib/api/booking'
import type { BookingAppointmentDto } from '@/lib/api/booking-types'

// ─── Status styling ───────────────────────────────────────────────────────────

function getStatusClass(status: string): string {
  switch (status) {
    case 'Scheduled': return 'bg-blue-100 text-blue-800 border-blue-200'
    case 'CheckedIn': return 'bg-yellow-100 text-yellow-800 border-yellow-200'
    case 'Completed': return 'bg-green-100 text-green-800 border-green-200'
    case 'Cancelled': return 'bg-gray-100 text-gray-600 border-gray-200'
    case 'NoShow': return 'bg-red-100 text-red-800 border-red-200'
    default: return ''
  }
}

const UPCOMING_STATUSES: string[] = ['Scheduled', 'CheckedIn']

// ─── Appointment Card ─────────────────────────────────────────────────────────

interface AppointmentCardProps {
  appointment: BookingAppointmentDto
  onClick: () => void
  t: ReturnType<typeof useTranslations<'portal.booking.appointments'>>
}

function AppointmentCard({ appointment, onClick, t }: AppointmentCardProps) {
  const scheduledAt = new Date(appointment.scheduledAt)
  const dateStr = scheduledAt.toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    weekday: 'short',
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
  const timeStr = scheduledAt.toLocaleTimeString('en-AE', {
    timeZone: 'Asia/Dubai',
    hour: '2-digit',
    minute: '2-digit',
  })

  return (
    <Card
      className="cursor-pointer hover:shadow-md transition-shadow"
      data-testid={`appointment-card-${appointment.id}`}
      onClick={onClick}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') onClick() }}
    >
      <CardContent className="p-4">
        <div className="flex items-start justify-between gap-3">
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-2">
              <span
                className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold border ${getStatusClass(appointment.status)}`}
                data-testid={`appointment-status-${appointment.id}`}
              >
                {t(`status.${appointment.status}` as Parameters<typeof t>[0])}
              </span>
            </div>

            <p className="font-semibold text-gray-900 truncate" data-testid={`appointment-type-${appointment.id}`}>
              {appointment.consultationTypeName}
            </p>

            <div className="mt-2 space-y-1 text-sm text-gray-600">
              <div className="flex items-center gap-1.5" data-testid={`appointment-date-${appointment.id}`}>
                <CalendarDays className="h-3.5 w-3.5 flex-shrink-0" />
                <span>{dateStr}</span>
              </div>
              <div className="flex items-center gap-1.5" data-testid={`appointment-time-${appointment.id}`}>
                <Clock className="h-3.5 w-3.5 flex-shrink-0" />
                <span>{timeStr}</span>
              </div>
              <div className="flex items-center gap-1.5" data-testid={`appointment-vet-${appointment.id}`}>
                <User className="h-3.5 w-3.5 flex-shrink-0" />
                <span className="truncate">{appointment.veterinarianName}</span>
              </div>
              <div className="flex items-center gap-1.5" data-testid={`appointment-pet-${appointment.id}`}>
                <PawPrint className="h-3.5 w-3.5 flex-shrink-0" />
                <span>{appointment.petName}</span>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

// ─── MyAppointments ───────────────────────────────────────────────────────────

type Tab = 'upcoming' | 'past'

export function MyAppointments() {
  const t = useTranslations('portal.booking.appointments')
  const router = useRouter()
  const params = useParams()
  const locale = params.locale as string
  const clinicSlug = params.clinicSlug as string

  const [appointments, setAppointments] = useState<BookingAppointmentDto[]>([])
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<Tab>('upcoming')

  useEffect(() => {
    listBookingAppointments()
      .then(setAppointments)
      .catch(() => setAppointments([]))
      .finally(() => setLoading(false))
  }, [])

  const upcoming = appointments.filter(a => UPCOMING_STATUSES.includes(a.status))
  const past = appointments.filter(a => !UPCOMING_STATUSES.includes(a.status))
  const displayed = activeTab === 'upcoming' ? upcoming : past

  function handleCardClick(id: string) {
    router.push(`/${locale}/portal/${clinicSlug}/book/appointments/${id}`)
  }

  function handleBookNew() {
    router.push(`/${locale}/portal/${clinicSlug}/book`)
  }

  return (
    <div data-testid="my-appointments">
      <h1 className="text-xl font-bold text-gray-900 mb-4" data-testid="my-appointments-title">
        {t('page_title')}
      </h1>

      {/* Tabs */}
      <div className="flex border-b border-gray-200 mb-4" role="tablist">
        <button
          role="tab"
          aria-selected={activeTab === 'upcoming'}
          data-testid="tab-upcoming"
          onClick={() => setActiveTab('upcoming')}
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            activeTab === 'upcoming'
              ? 'border-emerald-600 text-emerald-700'
              : 'border-transparent text-gray-500 hover:text-gray-700'
          }`}
        >
          {t('tab_upcoming')}
          {!loading && upcoming.length > 0 && (
            <span className="ml-1.5 inline-flex items-center justify-center rounded-full bg-emerald-100 text-emerald-800 text-xs font-semibold w-5 h-5">
              {upcoming.length}
            </span>
          )}
        </button>
        <button
          role="tab"
          aria-selected={activeTab === 'past'}
          data-testid="tab-past"
          onClick={() => setActiveTab('past')}
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            activeTab === 'past'
              ? 'border-emerald-600 text-emerald-700'
              : 'border-transparent text-gray-500 hover:text-gray-700'
          }`}
        >
          {t('tab_past')}
        </button>
      </div>

      {/* Content */}
      {loading ? (
        <div className="space-y-3" data-testid="appointments-loading">
          <Skeleton className="h-32 w-full rounded-lg" />
          <Skeleton className="h-32 w-full rounded-lg" />
        </div>
      ) : displayed.length === 0 ? (
        <div className="text-center py-12" data-testid="appointments-empty">
          <PawPrint className="h-12 w-12 text-gray-300 mx-auto mb-3" />
          <p className="text-gray-500 text-sm mb-4">
            {activeTab === 'upcoming' ? t('no_upcoming') : t('no_past')}
          </p>
          {activeTab === 'upcoming' && (
            <Button
              onClick={handleBookNew}
              data-testid="book-new-appointment-btn"
              className="gap-2"
            >
              <Plus className="h-4 w-4" />
              {t('book_new')}
            </Button>
          )}
        </div>
      ) : (
        <div className="space-y-3" data-testid="appointments-list">
          {displayed.map(appt => (
            <AppointmentCard
              key={appt.id}
              appointment={appt}
              onClick={() => handleCardClick(appt.id)}
              t={t}
            />
          ))}
          {activeTab === 'upcoming' && (
            <div className="pt-2">
              <Button
                variant="outline"
                onClick={handleBookNew}
                data-testid="book-new-appointment-btn"
                className="w-full gap-2"
              >
                <Plus className="h-4 w-4" />
                {t('book_new')}
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

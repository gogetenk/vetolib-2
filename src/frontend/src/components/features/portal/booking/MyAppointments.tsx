'use client'

import { useEffect, useState } from 'react'
import { useTranslations, useLocale } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { CalendarDays, ChevronRight, ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { listBookingAppointments } from '@/lib/api/booking'
import { ApiError } from '@/lib/api/client'
import type { BookingAppointmentDto, BookingAppointmentStatus } from '@/lib/api/booking-types'

type Tab = 'upcoming' | 'past'

const UPCOMING_STATUSES: BookingAppointmentStatus[] = ['Scheduled', 'CheckedIn']
const PAST_STATUSES: BookingAppointmentStatus[] = ['Completed', 'Cancelled', 'NoShow']

const STATUS_STYLES: Record<BookingAppointmentStatus, string> = {
  Scheduled: 'bg-blue-100 text-blue-800',
  CheckedIn: 'bg-success/15 text-success',
  Completed: 'bg-muted text-muted-foreground',
  Cancelled: 'bg-red-100 text-red-700',
  NoShow: 'bg-orange-100 text-orange-700',
}

function AppointmentCard({
  appt,
  onClick,
}: {
  appt: BookingAppointmentDto
  onClick: () => void
}) {
  const t = useTranslations('portal.booking.myAppointments')
  const locale = useLocale()

  const date = new Date(appt.scheduledAt)
  const formattedDate = date.toLocaleDateString(locale, {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    timeZone: 'Asia/Dubai',
  })
  const formattedTime = date.toLocaleTimeString(locale, {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Dubai',
    hour12: true,
  })

  return (
    <div
      role="button"
      tabIndex={0}
      data-testid={`appointment-card-${appt.id}`}
      className="flex items-center gap-4 rounded-xl border border-border/80 bg-white p-4 shadow-sm hover:border-primary/40 hover:shadow-md transition-all cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30"
      onClick={onClick}
      onKeyDown={(e) => {
        if (e.key === 'Enter') {
          onClick()
        } else if (e.key === ' ') {
          e.preventDefault()
          onClick()
        }
      }}
    >
      <div className="flex-shrink-0 flex items-center justify-center w-12 h-12 rounded-full bg-muted border border-border/80">
        <CalendarDays className="w-5 h-5 text-primary" />
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <span
            data-testid={`appointment-status-${appt.id}`}
            className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_STYLES[appt.status]}`}
          >
            {t(`status.${appt.status}`)}
          </span>
        </div>
        <p
          className="text-[14px] font-semibold text-foreground truncate"
          data-testid={`appointment-type-${appt.id}`}
        >
          {appt.consultationTypeName}
        </p>
        <p className="text-xs text-muted-foreground mt-0.5" data-testid={`appointment-pet-${appt.id}`}>
          {appt.petName} &middot; {appt.veterinarianName}
        </p>
        <p className="text-xs text-muted-foreground/70 mt-0.5" data-testid={`appointment-date-${appt.id}`}>
          {formattedDate} &middot; {formattedTime}
        </p>
      </div>
      <ChevronRight className="h-4 w-4 text-muted-foreground flex-shrink-0" />
    </div>
  )
}

export function MyAppointments() {
  const t = useTranslations('portal.booking')
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const [appointments, setAppointments] = useState<BookingAppointmentDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [expired, setExpired] = useState(false)
  const [error, setError] = useState(false)
  const [activeTab, setActiveTab] = useState<Tab>('upcoming')

  const base = `/${params.locale}/portal/${params.clinicSlug}/book`

  useEffect(() => {
    listBookingAppointments()
      .then(setAppointments)
      .catch((err) => {
        if (err instanceof ApiError && err.status === 401) {
          setExpired(true)
        } else {
          setError(true)
        }
      })
      .finally(() => setIsLoading(false))
  }, [])

  const upcoming = appointments.filter(a => UPCOMING_STATUSES.includes(a.status))
    .sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime())
  const past = appointments.filter(a => PAST_STATUSES.includes(a.status))
    .sort((a, b) => new Date(b.scheduledAt).getTime() - new Date(a.scheduledAt).getTime())

  const displayed = activeTab === 'upcoming' ? upcoming : past

  function navigateToDetail(id: string) {
    router.push(`${base}/appointments/${id}`)
  }

  if (expired) {
    return (
      <div className="text-center py-12 space-y-3" data-testid="my-appointments-expired">
        <p className="text-foreground">{t('landing.link_expired')}</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12 space-y-3" data-testid="my-appointments-error">
        <p className="text-foreground">{t('myAppointments.loadError')}</p>
      </div>
    )
  }

  return (
    <div className="space-y-5" data-testid="my-appointments">
      {/* Back button */}
      <Button
        variant="ghost"
        size="sm"
        data-testid="my-appointments-back-btn"
        className="text-muted-foreground hover:text-foreground -ms-2"
        onClick={() => router.push(base)}
      >
        <ArrowLeft className="h-4 w-4 me-1" />
        {t('myAppointments.backButton')}
      </Button>

      <h1 className="text-[22px] font-bold text-foreground" data-testid="my-appointments-title">
        {t('myAppointments.title')}
      </h1>

      {/* Tabs */}
      <div className="flex gap-1 bg-white border border-border/80 rounded-xl shadow-sm px-2" data-testid="appointments-tabs" role="tablist">
        <button
          data-testid="tab-upcoming"
          role="tab"
          aria-selected={activeTab === 'upcoming'}
          className={`px-5 py-3 text-[13px] font-semibold transition-colors border-b-2 ${
            activeTab === 'upcoming'
              ? 'border-primary text-primary'
              : 'border-transparent text-muted-foreground hover:text-foreground hover:border-border'
          }`}
          onClick={() => setActiveTab('upcoming')}
        >
          {t('myAppointments.upcoming')}
          {upcoming.length > 0 && (
            <span className="ms-1.5 text-xs bg-primary/10 text-primary px-1.5 py-0.5 rounded-full font-bold">
              {upcoming.length}
            </span>
          )}
        </button>
        <button
          data-testid="tab-past"
          role="tab"
          aria-selected={activeTab === 'past'}
          className={`px-5 py-3 text-[13px] font-semibold transition-colors border-b-2 ${
            activeTab === 'past'
              ? 'border-primary text-primary'
              : 'border-transparent text-muted-foreground hover:text-foreground hover:border-border'
          }`}
          onClick={() => setActiveTab('past')}
        >
          {t('myAppointments.past')}
          {past.length > 0 && (
            <span className="ms-1.5 text-xs bg-muted text-muted-foreground px-1.5 py-0.5 rounded-full font-bold">
              {past.length}
            </span>
          )}
        </button>
      </div>

      {/* Content */}
      {isLoading ? (
        <div className="space-y-3" data-testid="appointments-loading">
          {[1, 2, 3].map(i => (
            <div key={i} className="h-20 rounded-xl bg-muted animate-pulse" />
          ))}
        </div>
      ) : displayed.length === 0 ? (
        <div
          className="text-center py-12 text-muted-foreground"
          data-testid="appointments-empty"
        >
          <CalendarDays className="w-10 h-10 mx-auto mb-2 text-muted-foreground/50" />
          <p className="text-sm">{t('myAppointments.noAppointments')}</p>
        </div>
      ) : (
        <div className="space-y-3" data-testid="appointments-list">
          {displayed.map(appt => (
            <AppointmentCard
              key={appt.id}
              appt={appt}
              onClick={() => navigateToDetail(appt.id)}
            />
          ))}
        </div>
      )}
    </div>
  )
}

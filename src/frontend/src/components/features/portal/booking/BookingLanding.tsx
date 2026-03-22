'use client'

import { useState, useEffect } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { CalendarDays, ListChecks, Clock, MapPin, Phone } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { LtrText } from '@/components/ui/ltr-text'
import { format } from 'date-fns'
import { getPortalClinicInfo, type PortalClinicInfoDto } from '@/lib/api/portal'

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
  const [clinicInfo, setClinicInfo] = useState<PortalClinicInfoDto | null>(null)
  const [nextAppointment] = useState<NextAppointment | null>(() => {
    if (typeof window === 'undefined') return null
    try {
      const stored = sessionStorage.getItem('portal_next_appointment')
      return stored ? JSON.parse(stored) : null
    } catch {
      return null
    }
  })

  useEffect(() => {
    getPortalClinicInfo()
      .then(setClinicInfo)
      .catch(() => {
        /* silently ignore */
      })
  }, [])

  const base = `/${params.locale}/portal/${params.clinicSlug}/book`

  return (
    <div className="space-y-6" data-testid="booking-landing">
      <div>
        <h1
          className="text-[22px] font-bold text-foreground"
          data-testid="booking-landing-title"
        >
          {t('landing.title')}
        </h1>
        <p className="text-muted-foreground text-[13px] mt-1" data-testid="booking-landing-subtitle">
          {t('landing.subtitle')}
        </p>
      </div>

      {/* Clinic Info Card */}
      {clinicInfo && (
        <Card className="rounded-xl shadow-sm border-border/80" data-testid="clinic-info-card">
          <CardContent className="py-4 space-y-2">
            <h2 className="text-[15px] font-semibold text-foreground" data-testid="clinic-info-name">
              {clinicInfo.name}
            </h2>
            <div className="flex items-start gap-2 text-[13px] text-muted-foreground">
              <MapPin className="w-4 h-4 mt-0.5 flex-shrink-0" />
              <span data-testid="clinic-info-address">{clinicInfo.address}</span>
            </div>
            <div className="flex items-center gap-2 text-[13px]">
              <Phone className="w-4 h-4 flex-shrink-0 text-muted-foreground" />
              <a
                href={`tel:${clinicInfo.phone.replace(/\s/g, '')}`}
                className="text-primary hover:underline"
                data-testid="clinic-info-phone"
              >
                <LtrText>{clinicInfo.phone}</LtrText>
              </a>
            </div>
            <div className="flex items-center gap-2 text-[13px] text-muted-foreground">
              <Clock className="w-4 h-4 flex-shrink-0" />
              <span data-testid="clinic-info-hours">{clinicInfo.openingHours}</span>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Next Appointment preview card */}
      {nextAppointment && (
        <Card className="border-primary/20 bg-primary/5 rounded-xl shadow-sm" data-testid="next-appointment-card">
          <CardContent className="flex items-center gap-4 py-4">
            <div className="flex items-center justify-center w-12 h-12 rounded-full bg-primary/10">
              <Clock className="w-6 h-6 text-primary" />
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium text-primary uppercase tracking-wide">
                {t('landing.nextAppointment')}
              </p>
              <p className="text-[14px] font-semibold text-foreground mt-0.5">
                <LtrText>{format(new Date(nextAppointment.scheduledAt), 'EEEE, dd MMM yyyy - HH:mm')}</LtrText>
              </p>
              <div className="flex items-center gap-2 mt-1">
                <span className="text-[13px] text-foreground">{nextAppointment.petName}</span>
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
          className="flex flex-col gap-3 rounded-xl border border-border/80 bg-white p-6 shadow-sm hover:border-primary/40 hover:shadow-md transition-all cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30 min-h-[44px]"
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
          <div className="flex items-center justify-center w-12 h-12 rounded-full bg-primary/10">
            <ListChecks className="w-7 h-7 text-primary" />
          </div>
          <div>
            <h2 className="text-[14px] font-semibold text-foreground">
              {t('myAppointments.title')}
            </h2>
            <p className="text-[13px] text-muted-foreground mt-0.5">
              {t('landing.myAppointmentsSubtitle')}
            </p>
          </div>
        </div>

        {/* Book New Appointment card */}
        <div
          role="button"
          tabIndex={0}
          data-testid="booking-new-appointment-card"
          className="flex flex-col gap-3 rounded-xl border border-border/80 bg-white p-6 shadow-sm hover:border-primary/40 hover:shadow-md transition-all cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30 min-h-[44px]"
          onClick={() => router.push(`${base}/new`)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              router.push(`${base}/new`)
            } else if (e.key === ' ') {
              e.preventDefault()
              router.push(`${base}/new`)
            }
          }}
        >
          <div className="flex items-center justify-center w-12 h-12 rounded-full bg-primary/10">
            <CalendarDays className="w-7 h-7 text-primary" />
          </div>
          <div>
            <h2 className="text-[14px] font-semibold text-foreground">
              {t('wizard.step1Title')}
            </h2>
            <p className="text-[13px] text-muted-foreground mt-0.5">
              {t('landing.bookNewSubtitle')}
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

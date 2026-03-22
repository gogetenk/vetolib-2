'use client'

import { useEffect, useState } from 'react'
import { useTranslations, useLocale } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { ArrowLeft, CalendarDays, Clock, User, MapPin, FileText, XCircle, Calendar } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
  DialogClose,
} from '@/components/ui/dialog'
import { getBookingAppointment, cancelBookingAppointment } from '@/lib/api/booking'
import { ApiError } from '@/lib/api/client'
import type { BookingAppointmentDto, BookingAppointmentStatus } from '@/lib/api/booking-types'

const STATUS_STYLES: Record<BookingAppointmentStatus, string> = {
  Scheduled: 'bg-blue-100 text-blue-800',
  CheckedIn: 'bg-success/15 text-success',
  Completed: 'bg-muted text-muted-foreground',
  Cancelled: 'bg-red-100 text-red-700',
  NoShow: 'bg-orange-100 text-orange-700',
}

function canCancelOrReschedule(appt: BookingAppointmentDto): boolean {
  if (appt.status !== 'Scheduled') return false
  const scheduledAt = new Date(appt.scheduledAt)
  const now = new Date()
  const hoursUntil = (scheduledAt.getTime() - now.getTime()) / (1000 * 60 * 60)
  return hoursUntil >= 24
}

interface Props {
  appointmentId: string
}

export function AppointmentDetail({ appointmentId }: Props) {
  const t = useTranslations('portal.booking')
  const locale = useLocale()
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const [appt, setAppt] = useState<BookingAppointmentDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [expired, setExpired] = useState(false)
  const [notFound, setNotFound] = useState(false)
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false)
  const [isCancelling, setIsCancelling] = useState(false)

  const base = `/${params.locale}/portal/${params.clinicSlug}/book`

  useEffect(() => {
    getBookingAppointment(appointmentId)
      .then(setAppt)
      .catch((err) => {
        if (err instanceof ApiError) {
          if (err.status === 401) setExpired(true)
          else if (err.status === 404) setNotFound(true)
          else setNotFound(true)
        } else {
          setNotFound(true)
        }
      })
      .finally(() => setIsLoading(false))
  }, [appointmentId])

  async function handleCancel() {
    if (!appt) return
    setIsCancelling(true)
    try {
      await cancelBookingAppointment(appt.id, { reason: 'Cancelled by owner' })
      setAppt(prev => prev ? { ...prev, status: 'Cancelled' } : null)
      setCancelDialogOpen(false)
      toast.success(t('myAppointments.cancelSuccess'))
    } catch {
      toast.error(t('myAppointments.cancelError'))
    } finally {
      setIsCancelling(false)
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-4" data-testid="appointment-detail-loading">
        <div className="h-8 w-32 rounded bg-muted animate-pulse" />
        <div className="h-40 rounded-xl bg-muted animate-pulse" />
        <div className="h-20 rounded-xl bg-muted animate-pulse" />
      </div>
    )
  }

  if (expired) {
    return (
      <div className="text-center py-12" data-testid="appointment-detail-expired">
        <p className="text-foreground">{t('landing.link_expired')}</p>
      </div>
    )
  }

  if (notFound || !appt) {
    return (
      <div className="text-center py-12" data-testid="appointment-detail-not-found">
        <p className="text-foreground">{t('myAppointments.notFound')}</p>
        <Button
          variant="ghost"
          data-testid="appointment-detail-back-not-found"
          className="mt-4"
          onClick={() => router.push(`${base}/appointments`)}
        >
          {t('myAppointments.backButton')}
        </Button>
      </div>
    )
  }

  const scheduledDate = new Date(appt.scheduledAt)
  const formattedDate = scheduledDate.toLocaleDateString(locale, {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
    timeZone: 'Asia/Dubai',
  })
  const formattedTime = scheduledDate.toLocaleTimeString(locale, {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Dubai',
    hour12: true,
  })

  const canAct = canCancelOrReschedule(appt)

  return (
    <div className="space-y-6" data-testid="appointment-detail">
      {/* Back button */}
      <Button
        variant="ghost"
        size="sm"
        data-testid="appointment-detail-back-btn"
        className="text-muted-foreground hover:text-foreground -ms-2"
        onClick={() => router.push(`${base}/appointments`)}
      >
        <ArrowLeft className="h-4 w-4 me-1" />
        {t('myAppointments.backButton')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <h1 className="text-[22px] font-bold text-foreground" data-testid="appointment-detail-title">
          {appt.consultationTypeName}
        </h1>
        <span
          data-testid="appointment-detail-status"
          className={`flex-shrink-0 text-xs px-2.5 py-1 rounded-full font-medium ${STATUS_STYLES[appt.status]}`}
        >
          {t(`myAppointments.status.${appt.status}`)}
        </span>
      </div>

      {/* Detail card */}
      <div className="rounded-xl border border-border/80 bg-white p-5 space-y-4 shadow-sm">
        <div className="flex items-start gap-3">
          <CalendarDays className="w-5 h-5 text-muted-foreground mt-0.5 flex-shrink-0" />
          <div>
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">{t('confirmation.date')}</p>
            <p className="text-[14px] font-medium text-foreground" data-testid="appointment-detail-date">
              {formattedDate}
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3">
          <Clock className="w-5 h-5 text-muted-foreground mt-0.5 flex-shrink-0" />
          <div>
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">{t('myAppointments.timeLabel')}</p>
            <p className="text-[14px] font-medium text-foreground" data-testid="appointment-detail-time">
              {formattedTime} &middot; {appt.durationMinutes} min
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3">
          <User className="w-5 h-5 text-muted-foreground mt-0.5 flex-shrink-0" />
          <div>
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">{t('myAppointments.vetLabel')}</p>
            <p className="text-[14px] font-medium text-foreground" data-testid="appointment-detail-vet">
              {appt.veterinarianName}
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3">
          <User className="w-5 h-5 text-muted-foreground mt-0.5 flex-shrink-0" />
          <div>
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">{t('myAppointments.petLabel')}</p>
            <p className="text-[14px] font-medium text-foreground" data-testid="appointment-detail-pet">
              {appt.petName}
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3">
          <MapPin className="w-5 h-5 text-muted-foreground mt-0.5 flex-shrink-0" />
          <div>
            <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">{t('myAppointments.clinicLabel')}</p>
            <p className="text-[14px] font-medium text-foreground" data-testid="appointment-detail-clinic">
              {appt.clinicName}
            </p>
            <p className="text-xs text-muted-foreground" data-testid="appointment-detail-address">
              {appt.clinicAddress}
            </p>
          </div>
        </div>

        {appt.notes && (
          <div className="flex items-start gap-3">
            <FileText className="w-5 h-5 text-muted-foreground mt-0.5 flex-shrink-0" />
            <div>
              <p className="text-xs text-muted-foreground uppercase tracking-wide font-medium">{t('myAppointments.notesLabel')}</p>
              <p className="text-[13px] text-foreground" data-testid="appointment-detail-notes">
                {appt.notes}
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Actions */}
      {canAct && (
        <div className="flex flex-col sm:flex-row gap-3" data-testid="appointment-detail-actions">
          <Button
            variant="outline"
            data-testid="appointment-reschedule-btn"
            className="flex-1 border-border/80 text-foreground hover:border-primary/40 rounded-xl"
            onClick={() => {
              // Reschedule is a future action — placeholder navigation
              toast.info(t('myAppointments.rescheduleComingSoon'))
            }}
          >
            <Calendar className="h-4 w-4 me-2" />
            {t('myAppointments.rescheduleButton')}
          </Button>
          <Button
            variant="outline"
            data-testid="appointment-cancel-btn"
            className="flex-1 border-red-200 text-red-600 hover:bg-red-50 hover:border-red-400 rounded-xl"
            onClick={() => setCancelDialogOpen(true)}
          >
            <XCircle className="h-4 w-4 me-2" />
            {t('myAppointments.cancelButton')}
          </Button>
        </div>
      )}

      {/* Cancel confirmation dialog */}
      <Dialog open={cancelDialogOpen} onOpenChange={setCancelDialogOpen}>
        <DialogContent data-testid="cancel-dialog" showCloseButton={false}>
          <DialogHeader>
            <DialogTitle data-testid="cancel-dialog-title">
              {t('myAppointments.cancelConfirmTitle')}
            </DialogTitle>
            <DialogDescription data-testid="cancel-dialog-description">
              {t('myAppointments.cancelConfirmMessage')}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <DialogClose
              render={
                <Button
                  variant="outline"
                  data-testid="cancel-dialog-dismiss-btn"
                  disabled={isCancelling}
                />
              }
            >
              {t('wizard.cancel')}
            </DialogClose>
            <Button
              data-testid="cancel-dialog-confirm-btn"
              className="bg-red-600 hover:bg-red-700 text-white"
              onClick={handleCancel}
              disabled={isCancelling}
            >
              {isCancelling ? t('myAppointments.cancelling') : t('myAppointments.cancelButton')}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}

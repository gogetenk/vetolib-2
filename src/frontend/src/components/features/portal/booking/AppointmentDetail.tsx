'use client'

import { useState, useEffect } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import {
  CalendarDays,
  Clock,
  User,
  PawPrint,
  ArrowLeft,
  MapPin,
  FileText,
  AlertCircle,
} from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { toast } from 'sonner'
import { getBookingAppointment, cancelBookingAppointment } from '@/lib/api/booking'
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

// ─── Detail Row ───────────────────────────────────────────────────────────────

interface DetailRowProps {
  icon: React.ReactNode
  label: string
  value: React.ReactNode
  testId: string
}

function DetailRow({ icon, label, value, testId }: DetailRowProps) {
  return (
    <div className="flex items-start gap-3 py-3 border-b border-gray-100 last:border-0">
      <div className="text-gray-400 mt-0.5 flex-shrink-0">{icon}</div>
      <div className="flex-1 min-w-0">
        <p className="text-xs text-gray-500 font-medium uppercase tracking-wide mb-0.5">{label}</p>
        <div className="text-sm text-gray-900 font-medium" data-testid={testId}>{value}</div>
      </div>
    </div>
  )
}

// ─── AppointmentDetail ────────────────────────────────────────────────────────

interface AppointmentDetailProps {
  appointmentId: string
}

export function AppointmentDetail({ appointmentId }: AppointmentDetailProps) {
  const t = useTranslations('portal.booking.appointments')
  const router = useRouter()
  const params = useParams()
  const locale = params.locale as string
  const clinicSlug = params.clinicSlug as string

  const [appointment, setAppointment] = useState<BookingAppointmentDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(false)

  const [cancelOpen, setCancelOpen] = useState(false)
  const [cancelReason, setCancelReason] = useState('')
  const [cancelling, setCancelling] = useState(false)

  useEffect(() => {
    getBookingAppointment(appointmentId)
      .then(setAppointment)
      .catch(() => setError(true))
      .finally(() => setLoading(false))
  }, [appointmentId])

  function handleBack() {
    router.push(`/${locale}/portal/${clinicSlug}/book/appointments`)
  }

  function handleReschedule() {
    router.push(`/${locale}/portal/${clinicSlug}/book?reschedule=${appointmentId}`)
  }

  async function handleCancel() {
    if (!appointment) return
    setCancelling(true)
    try {
      const updated = await cancelBookingAppointment(appointmentId, {
        reason: cancelReason.trim() || null,
      })
      setAppointment(updated)
      setCancelOpen(false)
      setCancelReason('')
      toast.success(t('cancel_success'))
    } catch {
      toast.error(t('cancel_error'))
    } finally {
      setCancelling(false)
    }
  }

  // Actions allowed only for Scheduled status AND >24h away
  const canModify = (() => {
    if (!appointment) return false
    if (appointment.status !== 'Scheduled') return false
    const hoursUntil = (new Date(appointment.scheduledAt).getTime() - Date.now()) / (1000 * 60 * 60)
    return hoursUntil > 24
  })()

  // ─── Loading ──────────────────────────────────────────────────────────────

  if (loading) {
    return (
      <div data-testid="appointment-detail-loading">
        <Skeleton className="h-6 w-40 mb-6" />
        <Skeleton className="h-64 w-full rounded-xl" />
      </div>
    )
  }

  // ─── Error ────────────────────────────────────────────────────────────────

  if (error || !appointment) {
    return (
      <div className="text-center py-12" data-testid="appointment-detail-error">
        <AlertCircle className="h-12 w-12 text-red-400 mx-auto mb-3" />
        <p className="text-gray-600 mb-4">{t('error_load')}</p>
        <Button variant="outline" onClick={handleBack} data-testid="back-to-appointments-btn">
          <ArrowLeft className="h-4 w-4 mr-2" />
          {t('back')}
        </Button>
      </div>
    )
  }

  // ─── Format ───────────────────────────────────────────────────────────────

  const dateTimeStr = new Date(appointment.scheduledAt).toLocaleString('en-AE', {
    timeZone: 'Asia/Dubai',
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })

  const isReadOnly = appointment.status !== 'Scheduled'

  return (
    <div data-testid="appointment-detail">
      {/* Back link */}
      <button
        onClick={handleBack}
        data-testid="back-to-appointments-link"
        className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-700 mb-4 transition-colors"
      >
        <ArrowLeft className="h-4 w-4" />
        {t('back')}
      </button>

      {/* Status + Title */}
      <div className="mb-4">
        <span
          className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold border mb-2 ${getStatusClass(appointment.status)}`}
          data-testid="detail-status-badge"
        >
          {t(`status.${appointment.status}` as Parameters<typeof t>[0])}
        </span>
        <h1 className="text-xl font-bold text-gray-900" data-testid="detail-consultation-type">
          {appointment.consultationTypeName}
        </h1>
      </div>

      {/* Read-only notice */}
      {isReadOnly && (
        <div
          className="flex items-start gap-2 bg-gray-50 border border-gray-200 rounded-lg px-3 py-2.5 mb-4 text-sm text-gray-600"
          data-testid="read-only-notice"
        >
          <AlertCircle className="h-4 w-4 flex-shrink-0 mt-0.5" />
          <span>
            {t('read_only_notice', {
              status: t(`status.${appointment.status}` as Parameters<typeof t>[0]),
            })}
          </span>
        </div>
      )}

      {/* Details card */}
      <Card className="mb-4">
        <CardContent className="p-4">
          <DetailRow
            icon={<PawPrint className="h-4 w-4" />}
            label={t('fields.pet')}
            value={appointment.petName}
            testId="detail-pet-name"
          />
          <DetailRow
            icon={<User className="h-4 w-4" />}
            label={t('fields.vet')}
            value={appointment.veterinarianName}
            testId="detail-vet-name"
          />
          <DetailRow
            icon={<CalendarDays className="h-4 w-4" />}
            label={t('fields.date_time')}
            value={dateTimeStr}
            testId="detail-date-time"
          />
          <DetailRow
            icon={<Clock className="h-4 w-4" />}
            label={t('fields.duration')}
            value={t('duration', { minutes: appointment.durationMinutes })}
            testId="detail-duration"
          />
          <DetailRow
            icon={<MapPin className="h-4 w-4" />}
            label={appointment.clinicName}
            value={appointment.clinicAddress}
            testId="detail-clinic-address"
          />
          {appointment.notes && (
            <DetailRow
              icon={<FileText className="h-4 w-4" />}
              label={t('fields.notes')}
              value={appointment.notes}
              testId="detail-notes"
            />
          )}
        </CardContent>
      </Card>

      {/* Action buttons — Scheduled + >24h only */}
      {canModify && (
        <div className="flex flex-col sm:flex-row gap-3" data-testid="appointment-actions">
          {/* Cancel dialog */}
          <Dialog open={cancelOpen} onOpenChange={setCancelOpen}>
            <DialogTrigger
              render={
                <Button
                  variant="outline"
                  className="flex-1 border-red-200 text-red-700 hover:bg-red-50"
                  data-testid="cancel-appointment-btn"
                />
              }
            >
              {t('cancel')}
            </DialogTrigger>
            <DialogContent data-testid="cancel-dialog">
              <DialogHeader>
                <DialogTitle data-testid="cancel-dialog-title">
                  {t('cancel_dialog_title')}
                </DialogTitle>
                <DialogDescription data-testid="cancel-dialog-description">
                  {t('cancel_dialog_description')}
                </DialogDescription>
              </DialogHeader>

              <div>
                <label
                  htmlFor="cancel-reason"
                  className="block text-sm font-medium text-gray-700 mb-1"
                  data-testid="cancel-reason-label"
                >
                  {t('cancel_reason_label')}
                </label>
                <Textarea
                  id="cancel-reason"
                  placeholder={t('cancel_reason_placeholder')}
                  value={cancelReason}
                  onChange={(e) => setCancelReason(e.target.value)}
                  rows={3}
                  data-testid="cancel-reason-input"
                />
              </div>

              <DialogFooter>
                <Button
                  variant="destructive"
                  onClick={handleCancel}
                  disabled={cancelling}
                  data-testid="cancel-confirm-btn"
                >
                  {cancelling ? t('cancel_cancelling') : t('cancel_confirm')}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>

          {/* Reschedule */}
          <Button
            className="flex-1"
            onClick={handleReschedule}
            data-testid="reschedule-appointment-btn"
          >
            {t('reschedule')}
          </Button>
        </div>
      )}
    </div>
  )
}

'use client'

import { useState } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import { toast } from 'sonner'
import Link from 'next/link'
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
  SheetFooter,
} from '@/components/ui/sheet'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { getConsultationColor } from './consultation-colors'
import { transitionAppointment, cancelAppointment } from '@/lib/api/appointments'
import type { AppointmentStatus, AppointmentAction } from '@/lib/api/appointments'
import type { CalendarAppointment } from './types'
import {
  UserIcon,
  PhoneIcon,
  ClockIcon,
  CalendarIcon,
  StickyNoteIcon,
  ExternalLinkIcon,
  CheckCircle2Icon,
  PlayIcon,
  XCircleIcon,
  LogInIcon,
} from 'lucide-react'

const SPECIES_EMOJI: Record<string, string> = {
  Dog: '\uD83D\uDC36',
  Cat: '\uD83D\uDC31',
  Bird: '\uD83D\uDC26',
  Rabbit: '\uD83D\uDC30',
  Horse: '\uD83D\uDC34',
  Exotic: '\uD83E\uDD8E',
}

const STATUS_BADGE_VARIANT: Record<AppointmentStatus, string> = {
  SCHEDULED: 'bg-blue-100 text-blue-700',
  CHECKED_IN: 'bg-yellow-100 text-yellow-700',
  IN_PROGRESS: 'bg-green-100 text-green-700',
  COMPLETED: 'bg-gray-100 text-gray-700',
  CANCELLED: 'bg-red-100 text-red-700',
}

interface AppointmentDetailSheetProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  appointment: CalendarAppointment | null
  onUpdated: () => void
}

interface TransitionConfig {
  action: AppointmentAction
  label: string
  icon: React.ReactNode
  variant: 'default' | 'destructive' | 'outline' | 'secondary' | 'ghost'
  testId: string
}

function getAvailableTransitions(status: AppointmentStatus): TransitionConfig[] {
  switch (status) {
    case 'SCHEDULED':
      return [
        {
          action: 'CHECK_IN',
          label: 'Check In',
          icon: <LogInIcon className="size-4" />,
          variant: 'default',
          testId: 'detail-sheet-checkin-btn',
        },
        {
          action: 'CANCEL',
          label: 'Cancel',
          icon: <XCircleIcon className="size-4" />,
          variant: 'destructive',
          testId: 'detail-sheet-cancel-btn',
        },
      ]
    case 'CHECKED_IN':
      return [
        {
          action: 'START',
          label: 'Start Consultation',
          icon: <PlayIcon className="size-4" />,
          variant: 'default',
          testId: 'detail-sheet-start-btn',
        },
      ]
    case 'IN_PROGRESS':
      return [
        {
          action: 'COMPLETE',
          label: 'Complete',
          icon: <CheckCircle2Icon className="size-4" />,
          variant: 'default',
          testId: 'detail-sheet-complete-btn',
        },
      ]
    default:
      return []
  }
}

export function AppointmentDetailSheet({
  open,
  onOpenChange,
  appointment,
  onUpdated,
}: AppointmentDetailSheetProps) {
  const locale = useLocale()
  const t = useTranslations('calendar.detail')
  const isRtl = locale === 'ar'

  const [confirmAction, setConfirmAction] = useState<TransitionConfig | null>(null)
  const [transitioning, setTransitioning] = useState(false)

  if (!appointment) return null

  const color = getConsultationColor(appointment.consultationType)
  const emoji = SPECIES_EMOJI[appointment.species] ?? '\uD83D\uDC3E'
  const scheduledDate = new Date(appointment.scheduledAt)

  const dateStr = new Intl.DateTimeFormat(locale, {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(scheduledDate)

  const timeStr = new Intl.DateTimeFormat(locale, {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  }).format(scheduledDate)

  const transitions = getAvailableTransitions(appointment.status)

  async function handleTransition(config: TransitionConfig) {
    if (!appointment) return
    setTransitioning(true)
    try {
      if (config.action === 'CANCEL') {
        await cancelAppointment(appointment.id, 'Cancelled from calendar')
      } else {
        await transitionAppointment(appointment.id, config.action)
      }
      toast.success(`Appointment ${config.label.toLowerCase()}ed`)
      setConfirmAction(null)
      onOpenChange(false)
      onUpdated()
    } catch {
      toast.error(`Failed to ${config.label.toLowerCase()} appointment`)
    } finally {
      setTransitioning(false)
    }
  }

  return (
    <>
      <Sheet open={open} onOpenChange={onOpenChange}>
        <SheetContent
          side={isRtl ? 'left' : 'right'}
          data-testid="appointment-detail-sheet"
        >
          <SheetHeader>
            <SheetTitle>
              {emoji} {appointment.patientName}
            </SheetTitle>
            <SheetDescription>
              <Badge className={`${STATUS_BADGE_VARIANT[appointment.status]} border-0`}>
                {appointment.status.replace('_', ' ')}
              </Badge>
            </SheetDescription>
          </SheetHeader>

          <div className="flex flex-col gap-4 px-4 py-2 overflow-y-auto flex-1">
            {/* Consultation type */}
            <div className="flex items-center gap-2">
              <Badge className={`${color.bg} ${color.text} border-0`}>
                {appointment.consultationType}
              </Badge>
            </div>

            {/* Patient */}
            <DetailRow icon={<span className="text-base">{emoji}</span>} label={t('patient')}>
              {appointment.patientName} ({appointment.species})
            </DetailRow>

            {/* Owner */}
            <DetailRow icon={<UserIcon className="size-4 text-muted-foreground" />} label={t('owner')}>
              {appointment.ownerName}
            </DetailRow>

            {/* Phone */}
            <DetailRow icon={<PhoneIcon className="size-4 text-muted-foreground" />} label={t('phone')}>
              {appointment.ownerPhone || '---'}
            </DetailRow>

            {/* Vet */}
            <DetailRow icon={<UserIcon className="size-4 text-muted-foreground" />} label={t('vet')}>
              {appointment.vetName}
            </DetailRow>

            {/* Duration */}
            <DetailRow icon={<ClockIcon className="size-4 text-muted-foreground" />} label={t('duration')}>
              {t('minutes', { count: appointment.durationMinutes })}
            </DetailRow>

            {/* Date */}
            <DetailRow icon={<CalendarIcon className="size-4 text-muted-foreground" />} label={t('date')}>
              {dateStr}
            </DetailRow>

            {/* Time */}
            <DetailRow icon={<ClockIcon className="size-4 text-muted-foreground" />} label={t('time')}>
              {timeStr}
            </DetailRow>

            {/* Reason */}
            {appointment.reason && (
              <DetailRow icon={<StickyNoteIcon className="size-4 text-muted-foreground" />} label={t('reason')}>
                {appointment.reason}
              </DetailRow>
            )}

            {/* Notes */}
            {appointment.notes && (
              <DetailRow icon={<StickyNoteIcon className="size-4 text-muted-foreground" />} label={t('notes')}>
                {appointment.notes}
              </DetailRow>
            )}
          </div>

          <SheetFooter>
            {/* Transition buttons */}
            {transitions.length > 0 && (
              <div className="flex gap-2 w-full">
                {transitions.map((config) => (
                  <Button
                    key={config.action}
                    variant={config.variant}
                    className="flex-1 gap-1.5"
                    onClick={() => setConfirmAction(config)}
                    data-testid={config.testId}
                  >
                    {config.icon}
                    {config.label}
                  </Button>
                ))}
              </div>
            )}

            <Link
              href={`/${locale}/appointments/${appointment.id}`}
              className="inline-flex items-center justify-center gap-1.5 text-sm text-primary hover:underline w-full py-1"
              data-testid="detail-sheet-view-full-link"
            >
              <ExternalLinkIcon className="size-3.5" />
              {t('viewFull')}
            </Link>
          </SheetFooter>
        </SheetContent>
      </Sheet>

      {/* Confirmation dialog */}
      <Dialog open={!!confirmAction} onOpenChange={(open) => !open && setConfirmAction(null)}>
        <DialogContent className="sm:max-w-sm">
          <DialogHeader>
            <DialogTitle>Confirm {confirmAction?.label}</DialogTitle>
            <DialogDescription>
              Are you sure you want to {confirmAction?.label.toLowerCase()} this appointment for{' '}
              {appointment.patientName}?
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="ghost"
              onClick={() => setConfirmAction(null)}
              data-testid="confirm-dialog-cancel-btn"
            >
              Cancel
            </Button>
            <Button
              variant={confirmAction?.variant === 'destructive' ? 'destructive' : 'default'}
              disabled={transitioning}
              onClick={() => confirmAction && handleTransition(confirmAction)}
              data-testid="confirm-dialog-confirm-btn"
            >
              {transitioning ? 'Processing...' : confirmAction?.label}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}

function DetailRow({
  icon,
  label,
  children,
}: {
  icon: React.ReactNode
  label: string
  children: React.ReactNode
}) {
  return (
    <div className="flex items-start gap-3">
      <div className="mt-0.5 flex-shrink-0">{icon}</div>
      <div className="flex flex-col gap-0.5 min-w-0">
        <span className="text-xs text-muted-foreground">{label}</span>
        <span className="text-sm">{children}</span>
      </div>
    </div>
  )
}

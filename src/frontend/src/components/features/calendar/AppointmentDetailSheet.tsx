'use client'

import { useState } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import { toast } from 'sonner'
import Link from 'next/link'
import {
  Dialog,
  DialogContent,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { getConsultationColor } from './consultation-colors'
import { transitionAppointment, cancelAppointment, appointmentToDate } from '@/lib/api/appointments'
import type { AppointmentStatus, AppointmentAction } from '@/lib/api/appointments'
import type { CalendarAppointment } from './types'
import {
  PhoneIcon,
  MailIcon,
  MapPinIcon,
  ExternalLinkIcon,
  CheckCircle2Icon,
  PlayIcon,
  XCircleIcon,
  LogInIcon,
  SendIcon,
  UserIcon,
} from 'lucide-react'

const STATUS_BADGE_VARIANT: Record<AppointmentStatus, string> = {
  Scheduled: 'bg-primary/10 text-primary',
  CheckedIn: 'bg-orange-50 text-orange-500',
  InProgress: 'bg-success/10 text-success',
  Completed: 'bg-muted text-muted-foreground',
  Cancelled: 'bg-red-50 text-red-500',
  NoShow: 'bg-orange-100 text-orange-700',
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
    case 'Scheduled':
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
    case 'CheckedIn':
      return [
        {
          action: 'START',
          label: 'Start Consult',
          icon: <PlayIcon className="size-4" />,
          variant: 'default',
          testId: 'detail-sheet-start-btn',
        },
        {
          action: 'CANCEL',
          label: 'Cancel',
          icon: <XCircleIcon className="size-4" />,
          variant: 'destructive',
          testId: 'detail-sheet-cancel-btn',
        },
      ]
    case 'InProgress':
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
  const t = useTranslations('calendar')
  const locale = useLocale()
  const [transitioning, setTransitioning] = useState(false)

  if (!appointment) return null

  const color = getConsultationColor(appointment.consultationType)

  const dateObj = appointmentToDate(appointment)
  const dateStr = new Intl.DateTimeFormat(locale, {
    weekday: 'short',
    day: 'numeric',
    month: 'short',
    year: 'numeric'
  }).format(dateObj)
  
  const timeStr = new Intl.DateTimeFormat(locale, {
    hour: '2-digit',
    minute: '2-digit'
  }).format(dateObj)

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
      onOpenChange(false)
      onUpdated()
    } catch {
      toast.error(`Failed to ${config.label.toLowerCase()} appointment`)
    } finally {
      setTransitioning(false)
    }
  }

  // Parse owner name to display Last Name uppercase like Weda
  const ownerParts = appointment.ownerName.split(' ')
  const ownerLastName = ownerParts.length > 1 ? ownerParts[ownerParts.length - 1].toUpperCase() : appointment.ownerName.toUpperCase()
  const ownerFirstName = ownerParts.length > 1 ? ownerParts.slice(0, -1).join(' ') : ''

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent 
        className="w-[calc(100%-2rem)] sm:w-full sm:max-w-4xl p-0 overflow-hidden bg-card rounded-2xl border-0 shadow-2xl gap-0"
        data-testid="appointment-detail-dialog"
      >
        <DialogTitle className="sr-only">{t('detail.title')}</DialogTitle>
        
        <div className="flex flex-col md:flex-row min-h-[600px]">
          {/* Left Column: Patient & Owner Info */}
          <div className="w-full md:w-[40%] bg-card p-8 flex flex-col border-r border-border/50">
            <div className="mb-8">
              <h2 className="text-[22px] font-bold text-foreground leading-tight flex items-center gap-2">
                {ownerLastName} <span className="font-semibold text-primary">{ownerFirstName}</span>
              </h2>
              <p className="text-[13px] text-muted-foreground mt-1 font-medium">
                Patient: <span className="text-foreground" data-testid="detail-patient-name">{appointment.animalName}</span>
              </p>
            </div>

            <div className="space-y-6">
              <div>
                <h3 className="text-[13px] font-bold text-foreground mb-3">{t('detail.contactInfo')}</h3>
                <div className="space-y-3.5">
                  <div className="flex items-start gap-3">
                    <MapPinIcon className="w-4 h-4 text-muted-foreground mt-0.5" />
                    <span className="text-[14px] text-foreground font-medium">{t('detail.notProvided')}</span>
                  </div>
                  <div className="flex items-start gap-3">
                    <PhoneIcon className="w-4 h-4 text-muted-foreground mt-0.5" />
                    <span className="text-[14px] text-foreground font-medium">{t('detail.notProvided')}</span>
                  </div>
                  <div className="flex items-start gap-3">
                    <MailIcon className="w-4 h-4 text-muted-foreground mt-0.5" />
                    <span className="text-[14px] text-foreground font-medium">{t('detail.notProvided')}</span>
                  </div>
                </div>
              </div>

              <div className="pt-6 border-t border-border/50">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-[13px] font-bold text-foreground">{t('detail.attendingVet')}</h3>
                    <p className="text-[14px] text-foreground font-medium mt-1" data-testid="detail-vet-name">{appointment.veterinarianName}</p>
                  </div>
                </div>
                
                <Link href={`/${locale}/patients/${appointment.animalId}`}>
                  <Button size="lg" className="w-full font-semibold rounded-xl h-11 shadow-sm mt-2 flex items-center gap-2">
                    {t('detail.viewPatientRecord')}
                    <ExternalLinkIcon className="w-4 h-4" />
                  </Button>
                </Link>
              </div>
            </div>
          </div>

          {/* Right Column: Appointment Details */}
          <div className="w-full md:w-[60%] bg-muted p-8 flex flex-col relative">
            <div className="flex-1 space-y-6">
              {/* Header */}
              <div>
                <div className="flex items-center justify-between mb-1">
                  <h3 className="text-[18px] font-bold text-foreground flex items-center gap-2">
                    <span className="w-1 h-5 bg-primary rounded-full"></span>
                    {t('detail.theAppointment')}
                  </h3>
                  <Badge className={`${STATUS_BADGE_VARIANT[appointment.status]} border-0 px-2.5 py-1 text-[11px] font-bold uppercase tracking-wider`}>
                    {appointment.status.replace('_', ' ')}
                  </Badge>
                </div>
                <p className="text-[13px] text-muted-foreground font-medium ms-3 flex items-center gap-2">
                  <UserIcon className="w-3.5 h-3.5" /> {appointment.veterinarianName} <span className="text-border mx-1">•</span> <span className="text-primary font-bold">{dateStr}</span> <span className="text-border mx-1">•</span> {timeStr}
                </p>
              </div>

              {/* Consultation Type */}
              <div>
                <h4 className="text-[13px] font-bold text-foreground mb-2">{t('detail.consultationType')}</h4>
                <div className="bg-card border border-border/80 rounded-xl p-3.5 flex items-center justify-between shadow-sm">
                  <div className="flex items-center gap-3">
                    <div className={`w-8 h-8 rounded-full flex items-center justify-center ${color.bg}`}>
                      <div className={`w-3 h-3 rounded-full ${color.dot}`}></div>
                    </div>
                    <span className="text-[14px] font-semibold text-foreground" data-testid="detail-consultation-type">{appointment.consultationType}</span>
                  </div>
                  <Badge variant="outline" className="bg-muted text-muted-foreground border-border/50">
                    {t('detail.minutes', { count: appointment.durationMinutes })}
                  </Badge>
                </div>
              </div>

              {/* Reason */}
              <div>
                <h4 className="text-[13px] font-bold text-foreground mb-2">{t('detail.reasonLabel')}</h4>
                <div className="bg-card border border-border/80 rounded-xl p-4 min-h-[80px] shadow-sm text-[14px] font-medium text-foreground" data-testid="detail-reason">
                  {appointment.reason ?? <span className="text-muted-foreground italic">{t('detail.noReasonProvided')}</span>}
                </div>
              </div>

              {/* Notes */}
              <div>
                <h4 className="text-[13px] font-bold text-foreground mb-2">{t('detail.notesAndDiscussion')}</h4>
                <div className="bg-card border border-border/80 rounded-xl p-4 min-h-[100px] shadow-sm flex flex-col justify-between">
                  <div className="text-[14px] font-medium text-foreground mb-4" data-testid="detail-notes">
                    <span className="text-muted-foreground italic">{t('detail.noNotes')}</span>
                  </div>
                  {/* Fake input for discussion like Weda */}
                  <div className="relative mt-auto border-t border-border/50 pt-3">
                    <Input 
                      type="text" 
                      placeholder={t('detail.writeANote')} 
                      className="w-full bg-muted/50 rounded-xl ps-4 pe-12 py-2.5 text-[13px]"
                      disabled
                    />
                    <Button size="icon-sm" className="absolute right-1.5 top-[18px] rounded-lg" disabled>
                      <SendIcon className="w-3.5 h-3.5 ms-0.5" />
                    </Button>
                  </div>
                </div>
              </div>
            </div>

            {/* Bottom Actions */}
            {transitions.length > 0 && (
              <div className="mt-8 pt-4 border-t border-border/50 flex gap-3 justify-end">
                {transitions.map((config) => (
                  <Button
                    key={config.action}
                    variant={config.variant === 'destructive' ? 'outline' : config.variant}
                    className={`rounded-xl font-semibold shadow-sm px-8 h-11 text-[14px] ${config.variant === 'destructive' ? 'text-destructive border-destructive hover:bg-destructive/5' : ''}`}
                    onClick={() => handleTransition(config)}
                    disabled={transitioning}
                    data-testid={config.testId}
                  >
                    {transitioning ? 'Processing...' : config.label}
                  </Button>
                ))}
              </div>
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}

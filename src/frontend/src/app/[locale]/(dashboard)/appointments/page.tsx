import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { WeekCalendar } from '@/components/features/calendar/WeekCalendar'
import { getTranslations } from 'next-intl/server'

export default async function AppointmentsPage() {
  const t = await getTranslations('appointments')

  return (
    <div className="space-y-4" data-testid="appointments-page">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold" data-testid="appointments-title">
          {t('title')}
        </h1>
        <Link href="appointments/new">
          <Button data-testid="new-appointment-btn">
            {t('new')}
          </Button>
        </Link>
      </div>
      <WeekCalendar />
    </div>
  )
}

import { CalendarContainer } from '@/components/features/calendar/CalendarContainer'

export default async function AppointmentsPage() {
  return (
    <div className="h-full w-full flex flex-col p-6 lg:p-8" data-testid="appointments-page">
      <CalendarContainer />
    </div>
  )
}

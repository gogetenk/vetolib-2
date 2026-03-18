import { CalendarContainer } from '@/components/features/calendar/CalendarContainer'

export default async function AppointmentsPage() {
  return (
    <div className="h-full w-full flex flex-col p-4 lg:p-6" data-testid="appointments-page">
      <CalendarContainer />
    </div>
  )
}

import type { Metadata } from "next";
import { CalendarContainer } from '@/components/features/calendar/CalendarContainer'

export const metadata: Metadata = {
  title: "Appointments",
};

export default async function AppointmentsPage() {
  return (
    <div className="h-full w-full flex flex-col p-6 lg:p-8" data-testid="appointments-page">
      <h1 className="sr-only">Appointments</h1>
      <CalendarContainer />
    </div>
  )
}

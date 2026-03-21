import type { Metadata } from "next";
import { CalendarContainer } from '@/components/features/calendar/CalendarContainer'
import { PageContainer } from '@/components/ui/page-container'

export const metadata: Metadata = {
  title: "Appointments",
};

export default async function AppointmentsPage() {
  return (
    <PageContainer className="h-full w-full flex flex-col !max-w-screen-2xl" data-testid="appointments-page">
      <h1 className="sr-only">Appointments</h1>
      <CalendarContainer />
    </PageContainer>
  )
}

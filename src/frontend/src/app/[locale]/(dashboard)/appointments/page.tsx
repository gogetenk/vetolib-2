import { CalendarContainer } from '@/components/features/calendar/CalendarContainer'
import { PageContainer } from '@/components/ui/page-container'

export default async function AppointmentsPage() {
  return (
    <PageContainer className="h-full w-full flex flex-col" data-testid="appointments-page">
      <CalendarContainer />
    </PageContainer>
  )
}

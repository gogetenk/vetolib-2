import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { PageContainer } from '@/components/ui/page-container'
import { AppointmentForm } from '@/components/features/appointments/AppointmentForm'
import { getTranslations } from 'next-intl/server'

export default async function NewAppointmentPage() {
  const t = await getTranslations('appointments')

  return (
    <PageContainer variant="narrow" data-testid="new-appointment-page">
      <Link href="../appointments">
        <Button
          variant="ghost"
          size="sm"
          data-testid="back-to-appointments-btn"
          className="-ms-2 group/back text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4 me-1 transition-transform duration-200 ease-in-out group-hover/back:-translate-x-0.5 rtl:group-hover/back:translate-x-0.5" />
          {t('title')}
        </Button>
      </Link>
      <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2" data-testid="new-appointment-title">
        <span className="w-1 h-5 bg-primary rounded-full"></span>
        {t('form.title')}
      </h1>
      <AppointmentForm />
    </PageContainer>
  )
}

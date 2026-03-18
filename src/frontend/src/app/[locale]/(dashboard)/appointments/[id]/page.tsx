import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { PageContainer } from '@/components/ui/page-container'
import { AppointmentDetailLoader } from '@/components/features/appointments/AppointmentDetailLoader'
import { getTranslations } from 'next-intl/server'

interface Props {
  params: Promise<{ id: string }>
}

export default async function AppointmentDetailPage({ params }: Props) {
  const { id } = await params
  const t = await getTranslations('appointments')

  return (
    <PageContainer variant="default" data-testid="appointment-detail-page">
      <Link href="../appointments">
        <Button
          variant="ghost"
          size="sm"
          data-testid="back-to-appointments-btn"
          className="-ms-2 group/back text-muted-foreground hover:text-[#061e44]"
        >
          <ArrowLeft className="h-4 w-4 me-1 transition-transform duration-200 ease-in-out group-hover/back:-translate-x-0.5 rtl:group-hover/back:translate-x-0.5" />
          {t('title')}
        </Button>
      </Link>
      <AppointmentDetailLoader id={id} />
    </PageContainer>
  )
}

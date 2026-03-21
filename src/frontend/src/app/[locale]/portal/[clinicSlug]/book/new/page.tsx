import { getTranslations } from 'next-intl/server'
import { BookingWizard } from '@/components/features/portal/booking/BookingWizard'

interface Props {
  params: Promise<{ locale: string; clinicSlug: string }>
}

export default async function BookNewAppointmentPage({ params }: Props) {
  const { locale, clinicSlug } = await params
  const t = await getTranslations('portal.booking')

  return (
    <div className="mx-auto max-w-lg px-4 py-8">
      {/* Page header */}
      <div className="mb-6">
        <h1 className="text-[22px] font-bold text-foreground">{t('landing.title')}</h1>
        <p className="mt-1 text-[13px] text-muted-foreground">
          {t('landing.subtitle')}
        </p>
      </div>

      {/* Wizard */}
      <BookingWizard locale={locale} clinicSlug={clinicSlug} />
    </div>
  )
}

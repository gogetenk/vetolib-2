'use client'

import { useLocale } from 'next-intl'
import Link from 'next/link'
import { FileText } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { PageContainer } from '@/components/ui/page-container'

export default function MedicalRecordsPage() {
  const locale = useLocale()

  return (
    <PageContainer variant="default" data-testid="medical-records-page">
      <div className="flex min-h-[60vh] items-center justify-center py-16">
        <div className="flex flex-col items-center text-center">
          <div
            className="mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-primary/10"
            data-testid="medical-records-icon"
          >
            <FileText className="h-8 w-8 text-primary" />
          </div>
          <h1
            className="text-[22px] font-bold text-foreground"
            data-testid="medical-records-title"
          >
            Medical Records
          </h1>
          <p
            className="mt-2 max-w-sm text-[13px] text-muted-foreground"
            data-testid="medical-records-description"
          >
            Access patient records from each patient&apos;s profile page.
          </p>
          <Link href={`/${locale}/patients`}>
            <Button
              data-testid="go-to-patients-btn"
              className="mt-6 bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl h-11 px-6 text-[14px] shadow-sm"
            >
              Go to Patients
            </Button>
          </Link>
        </div>
      </div>
    </PageContainer>
  )
}

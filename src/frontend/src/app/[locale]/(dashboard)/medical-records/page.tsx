'use client'

import { useLocale } from 'next-intl'
import Link from 'next/link'
import { FileText } from 'lucide-react'

export default function MedicalRecordsPage() {
  const locale = useLocale()

  return (
    <div className="flex min-h-[60vh] items-center justify-center py-16" data-testid="medical-records-page">
      <div className="flex flex-col items-center text-center">
        <div
          className="mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-[#eef2fd]"
          data-testid="medical-records-icon"
        >
          <FileText className="h-8 w-8 text-[#303ef5]" />
        </div>
        <h1
          className="text-[18px] font-bold text-[#061e44]"
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
          <button
            className="mt-6 bg-[#303ef5] hover:bg-[#2530c4] text-white font-semibold rounded-xl h-11 px-6 text-[14px] shadow-sm transition-colors"
            data-testid="go-to-patients-btn"
          >
            Go to Patients
          </button>
        </Link>
      </div>
    </div>
  )
}

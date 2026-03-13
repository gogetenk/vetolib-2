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
          className="mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-stone-100"
          data-testid="medical-records-icon"
        >
          <FileText className="h-8 w-8 text-stone-400" />
        </div>
        <h1
          className="text-lg font-semibold text-stone-700"
          data-testid="medical-records-title"
        >
          Medical Records
        </h1>
        <p
          className="mt-2 max-w-sm text-sm text-stone-500"
          data-testid="medical-records-description"
        >
          Access patient records from each patient&apos;s profile page.
        </p>
        <Link href={`/${locale}/patients`}>
          <button
            className="mt-6 rounded-md bg-emerald-700 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-emerald-800"
            data-testid="go-to-patients-btn"
          >
            Go to Patients
          </button>
        </Link>
      </div>
    </div>
  )
}

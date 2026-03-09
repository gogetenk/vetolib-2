'use client'

import { useEffect, useState } from 'react'
import { useParams } from 'next/navigation'
import Link from 'next/link'
import { ArrowLeft, Phone, Mail, Pencil } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet'
import { MedicalRecordsList } from '@/components/features/patients/MedicalRecordsList'
import { SpeciesIcon } from '@/components/features/patients/SpeciesIcon'
import { PatientForm } from '@/components/features/patients/PatientForm'
import { getPatient, getPatientVaccinations, getPatientPrescriptions } from '@/lib/api/patients'
import type { PatientDto, VaccinationDto, PrescriptionDto } from '@/lib/api/patients'
import { getPatientMedicalRecords } from '@/lib/api/medical-records'
import type { MedicalRecordDto } from '@/lib/api/medical-records'
import { useRole } from '@/hooks/use-role'

type TabId = 'medical-records' | 'prescriptions' | 'vaccinations'

/** Returns a human-readable age string like "3 years 2 months" */
function calculateAge(dateOfBirth: string): string {
  const birth = new Date(dateOfBirth)
  const now = new Date()

  let years = now.getFullYear() - birth.getFullYear()
  let months = now.getMonth() - birth.getMonth()

  if (months < 0) {
    years -= 1
    months += 12
  }

  if (years === 0) {
    return months === 1 ? '1 month' : `${months} months`
  }
  if (months === 0) {
    return years === 1 ? '1 year' : `${years} years`
  }
  return `${years} year${years !== 1 ? 's' : ''} ${months} month${months !== 1 ? 's' : ''}`
}

function formatDate(dateStr: string | null): string {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

function VaccinationsTab({ vaccinations }: { vaccinations: VaccinationDto[] }) {
  if (vaccinations.length === 0) {
    return (
      <p className="text-muted-foreground text-sm py-8 text-center" data-testid="vaccinations-empty">
        No vaccination records found.
      </p>
    )
  }
  return (
    <div data-testid="vaccinations-list" className="space-y-2">
      <div className="grid grid-cols-4 gap-4 text-xs font-medium text-muted-foreground uppercase tracking-wide px-3 py-2">
        <span>Vaccine</span>
        <span>Date Given</span>
        <span>Next Due</span>
        <span>Vet</span>
      </div>
      <Separator />
      {vaccinations.map((vac) => (
        <div
          key={vac.id}
          data-testid={`vaccination-${vac.id}`}
          className="grid grid-cols-4 gap-4 text-sm px-3 py-3 rounded-md hover:bg-muted/50"
        >
          <span className="font-medium" data-testid={`vaccination-name-${vac.id}`}>{vac.name}</span>
          <span data-testid={`vaccination-date-${vac.id}`}>{formatDate(vac.administeredDate)}</span>
          <span data-testid={`vaccination-next-due-${vac.id}`}>{formatDate(vac.nextDueDate)}</span>
          <span data-testid={`vaccination-vet-${vac.id}`}>{vac.vetName}</span>
        </div>
      ))}
    </div>
  )
}

function PrescriptionsTab({ prescriptions }: { prescriptions: PrescriptionDto[] }) {
  if (prescriptions.length === 0) {
    return (
      <p className="text-muted-foreground text-sm py-8 text-center" data-testid="prescriptions-empty">
        No prescriptions found.
      </p>
    )
  }
  return (
    <div data-testid="prescriptions-list" className="space-y-3">
      {prescriptions.map((presc) => (
        <div
          key={presc.id}
          data-testid={`prescription-${presc.id}`}
          className="flex items-start justify-between gap-4 rounded-md border p-4"
        >
          <div>
            <p className="font-medium text-sm" data-testid={`presc-medication-${presc.id}`}>{presc.medication}</p>
            <p className="text-xs text-muted-foreground mt-1">
              {presc.dosage} &bull; {presc.duration}
            </p>
            <p className="text-xs text-muted-foreground">{presc.vetName} &bull; {formatDate(presc.prescribedDate)}</p>
          </div>
          <Badge
            variant={presc.status === 'active' ? 'default' : 'secondary'}
            data-testid={`presc-status-${presc.id}`}
          >
            {presc.status}
          </Badge>
        </div>
      ))}
    </div>
  )
}

export default function PatientDetailPage() {
  const { id } = useParams<{ id: string }>()
  const role = useRole()
  const [activeTab, setActiveTab] = useState<TabId>('medical-records')
  const [patient, setPatient] = useState<PatientDto | null>(null)
  const [records, setRecords] = useState<MedicalRecordDto[]>([])
  const [vaccinations, setVaccinations] = useState<VaccinationDto[]>([])
  const [prescriptions, setPrescriptions] = useState<PrescriptionDto[]>([])
  const [isLoadingPatient, setIsLoadingPatient] = useState(true)
  const [isLoadingRecords, setIsLoadingRecords] = useState(false)
  const [isEditOpen, setIsEditOpen] = useState(false)

  useEffect(() => {
    if (!id) return
    setIsLoadingPatient(true)
    getPatient(id)
      .then(setPatient)
      .catch(() => setPatient(null))
      .finally(() => setIsLoadingPatient(false))

    // Preload records
    setIsLoadingRecords(true)
    getPatientMedicalRecords(id)
      .then((res) => setRecords(res.items))
      .catch(() => setRecords([]))
      .finally(() => setIsLoadingRecords(false))

    getPatientVaccinations(id)
      .then(setVaccinations)
      .catch(() => setVaccinations([]))

    getPatientPrescriptions(id)
      .then(setPrescriptions)
      .catch(() => setPrescriptions([]))
  }, [id])

  const canWrite = role === 'VET' || role === 'ADMIN'

  const tabs: { id: TabId; label: string; testId: string }[] = [
    { id: 'medical-records', label: 'Medical Records', testId: 'tab-medical-records' },
    { id: 'prescriptions', label: 'Prescriptions', testId: 'tab-prescriptions' },
    { id: 'vaccinations', label: 'Vaccinations', testId: 'tab-vaccinations' },
  ]

  if (isLoadingPatient) {
    return (
      <div data-testid="patient-detail-loading" className="space-y-6">
        <div className="h-8 w-32 rounded bg-muted animate-pulse" />
        <div className="h-40 rounded-lg bg-muted animate-pulse" />
      </div>
    )
  }

  if (!patient) {
    return (
      <div data-testid="patient-not-found" className="py-12 text-center">
        <p className="text-muted-foreground">Patient not found.</p>
        <Button render={<Link href="/patients" />} variant="outline" className="mt-4" data-testid="back-to-patients-fallback-btn">
          Back to Patients
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6" data-testid="patient-detail-page">
      {/* Back navigation */}
      <Button
        render={<Link href="/patients" />}
        variant="ghost"
        size="sm"
        data-testid="back-to-patients-btn"
        className="-ml-2"
      >
        <ArrowLeft className="h-4 w-4 mr-1" />
        Patients
      </Button>

      {/* Patient header */}
      <div
        className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between"
        data-testid="patient-header"
      >
        <div className="flex items-center gap-4">
          {/* Species avatar */}
          <div
            className="flex h-16 w-16 items-center justify-center rounded-full bg-muted text-muted-foreground"
            data-testid="patient-species-avatar"
            aria-label={patient.species}
          >
            <SpeciesIcon species={patient.species} className="h-8 w-8" />
          </div>

          {/* Patient info */}
          <div>
            <h1
              className="text-2xl font-bold"
              data-testid="patient-detail-name"
            >
              {patient.name}
            </h1>
            <p
              className="text-muted-foreground"
              data-testid="patient-detail-species"
            >
              {patient.species} &bull; {patient.breed} &bull;{' '}
              <span data-testid="patient-detail-age">{calculateAge(patient.dateOfBirth)}</span>
              {' '}&bull; {patient.gender}
            </p>
            <div className="mt-2 flex flex-col gap-1 text-sm text-muted-foreground">
              <span data-testid="patient-detail-owner">
                <span className="font-medium text-foreground">Owner: </span>
                {patient.ownerName}
              </span>
              <span className="flex items-center gap-1" data-testid="patient-detail-phone">
                <Phone className="h-3.5 w-3.5" />
                {patient.ownerPhone}
              </span>
              <span className="flex items-center gap-1" data-testid="patient-detail-email">
                <Mail className="h-3.5 w-3.5" />
                {patient.ownerEmail}
              </span>
            </div>
          </div>
        </div>

        {/* Action buttons */}
        <div className="flex items-center gap-2">
          {/* Edit button — VET and ADMIN only */}
          {canWrite && (
            <>
              <Button
                variant="outline"
                data-testid="edit-patient-btn"
                onClick={() => setIsEditOpen(true)}
              >
                <Pencil className="h-4 w-4 mr-1" />
                Edit
              </Button>
              <Sheet open={isEditOpen} onOpenChange={setIsEditOpen}>
                <SheetContent side="right" className="w-full sm:max-w-xl overflow-y-auto">
                  <SheetHeader>
                    <SheetTitle>Edit Patient</SheetTitle>
                  </SheetHeader>
                  <div className="mt-4">
                    <PatientForm
                      patient={patient}
                      onSuccess={(updated) => {
                        setPatient(updated)
                        setIsEditOpen(false)
                      }}
                    />
                  </div>
                </SheetContent>
              </Sheet>
            </>
          )}

          {/* New Medical Record — VET only */}
          {role === 'VET' && (
            <Button
              render={<Link href={`/patients/${id}/records/new`} />}
              data-testid="new-medical-record-btn"
            >
              New Medical Record
            </Button>
          )}
        </div>
      </div>

      <Separator />

      {/* Tabs */}
      <div data-testid="patient-tabs">
        {/* Tab navigation */}
        <div
          className="flex gap-1 border-b"
          role="tablist"
          data-testid="tabs-nav"
        >
          {tabs.map((tab) => (
            <button
              key={tab.id}
              role="tab"
              aria-selected={activeTab === tab.id}
              data-testid={tab.testId}
              onClick={() => setActiveTab(tab.id)}
              className={[
                'px-4 py-2 text-sm font-medium border-b-2 transition-colors',
                activeTab === tab.id
                  ? 'border-primary text-primary'
                  : 'border-transparent text-muted-foreground hover:text-foreground hover:border-muted-foreground',
              ].join(' ')}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* Tab panels */}
        <div className="mt-4">
          {activeTab === 'medical-records' && (
            <div role="tabpanel" data-testid="tabpanel-medical-records">
              <MedicalRecordsList records={records} isLoading={isLoadingRecords} />
            </div>
          )}
          {activeTab === 'prescriptions' && (
            <div role="tabpanel" data-testid="tabpanel-prescriptions">
              <PrescriptionsTab prescriptions={prescriptions} />
            </div>
          )}
          {activeTab === 'vaccinations' && (
            <div role="tabpanel" data-testid="tabpanel-vaccinations">
              <VaccinationsTab vaccinations={vaccinations} />
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

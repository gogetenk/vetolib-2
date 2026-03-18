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
import { SpeciesIcon, getSpeciesColor } from '@/components/features/patients/SpeciesIcon'
import { PatientForm } from '@/components/features/patients/PatientForm'
import { getPatient, getPatientVaccinations, getPatientPrescriptions } from '@/lib/api/patients'
import type { PatientDto, VaccinationDto, PrescriptionDto } from '@/lib/api/patients'
import { getPatientMedicalRecords } from '@/lib/api/medical-records'
import type { MedicalRecordDto } from '@/lib/api/medical-records'
import { PageContainer } from '@/components/ui/page-container'
import { useRole } from '@/hooks/use-role'
import { useTranslations } from 'next-intl'

type TabId = 'medical-records' | 'prescriptions' | 'vaccinations'

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
      <p className="text-muted-foreground text-[13px] py-8 text-center" data-testid="vaccinations-empty">
        No vaccination records found.
      </p>
    )
  }
  return (
    <div data-testid="vaccinations-list" className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden w-full">
      <div className="grid grid-cols-4 gap-4 text-[11px] font-bold text-[#061e44] uppercase tracking-wider px-4 py-3 bg-[#f4f6f9] border-b border-border/50">
        <span>Vaccine</span>
        <span>Date Given</span>
        <span>Next Due</span>
        <span>Vet</span>
      </div>
      {vaccinations.map((vac) => (
        <div
          key={vac.id}
          data-testid={`vaccination-${vac.id}`}
          className="grid grid-cols-4 gap-4 text-[13px] px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-[#f4f6f9]/50 transition-colors"
        >
          <span className="font-semibold text-[#061e44]" data-testid={`vaccination-name-${vac.id}`}>{vac.name}</span>
          <span className="text-muted-foreground" data-testid={`vaccination-date-${vac.id}`}>{formatDate(vac.administeredDate)}</span>
          <span className="text-muted-foreground" data-testid={`vaccination-next-due-${vac.id}`}>{formatDate(vac.nextDueDate)}</span>
          <span className="text-muted-foreground" data-testid={`vaccination-vet-${vac.id}`}>{vac.vetName}</span>
        </div>
      ))}
    </div>
  )
}

function PrescriptionsTab({
  prescriptions,
  patientId,
  canPrescribe,
}: {
  prescriptions: PrescriptionDto[]
  patientId: string
  canPrescribe: boolean
}) {
  return (
    <div className="space-y-4">
      {canPrescribe && (
        <div className="flex justify-end">
          <Link href={`/patients/${patientId}/records/new`}>
            <Button
              size="sm"
              data-testid="new-prescription-btn"
              className="bg-[#303ef5] hover:bg-[#2530c4] text-white font-semibold rounded-xl shadow-sm"
            >
              New Medical Record
            </Button>
          </Link>
        </div>
      )}

      {prescriptions.length === 0 ? (
        <p className="text-muted-foreground text-[13px] py-8 text-center" data-testid="prescriptions-empty">
          No prescriptions found.
        </p>
      ) : (
        <div data-testid="prescriptions-list" className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden w-full">
          <div className="grid grid-cols-5 gap-4 text-[11px] font-bold text-[#061e44] uppercase tracking-wider px-4 py-3 bg-[#f4f6f9] border-b border-border/50">
            <span>Medication</span>
            <span>Dosage</span>
            <span>Duration</span>
            <span>Vet / Date</span>
            <span>Status</span>
          </div>
          {prescriptions.map((presc) => (
            <div
              key={presc.id}
              data-testid={`prescription-${presc.id}`}
              className="grid grid-cols-5 gap-4 items-center px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-[#f4f6f9]/50 transition-colors"
            >
              <p className="font-semibold text-[13px] text-[#061e44] truncate" data-testid={`presc-medication-${presc.id}`}>{presc.medication}</p>
              <p className="text-[13px] text-muted-foreground truncate">{presc.dosage}</p>
              <p className="text-[13px] text-muted-foreground truncate">{presc.duration}</p>
              <p className="text-[12px] text-muted-foreground truncate">{presc.vetName} &bull; {formatDate(presc.prescribedDate)}</p>
              <Badge
                variant={presc.status === 'active' ? 'default' : 'secondary'}
                data-testid={`presc-status-${presc.id}`}
                className={presc.status === 'active' ? 'bg-[#303ef5] text-white rounded-md text-[10px] font-bold uppercase tracking-wider' : 'rounded-md text-[10px] font-bold uppercase tracking-wider'}
              >
                {presc.status}
              </Badge>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

export default function PatientDetailPage() {
  const { id } = useParams<{ id: string }>()
  const role = useRole()
  const t = useTranslations('patients')
  const [activeTab, setActiveTab] = useState<TabId>('medical-records')
  const [patient, setPatient] = useState<PatientDto | null>(null)
  const [records, setRecords] = useState<MedicalRecordDto[]>([])
  const [vaccinations, setVaccinations] = useState<VaccinationDto[]>([])
  const [prescriptions, setPrescriptions] = useState<PrescriptionDto[]>([])
  const [isLoadingPatient, setIsLoadingPatient] = useState(true)
  const [isLoadingRecords, setIsLoadingRecords] = useState(true)
  const [isEditOpen, setIsEditOpen] = useState(false)

  useEffect(() => {
    if (!id) return
    getPatient(id)
      .then(setPatient)
      .catch(() => setPatient(null))
      .finally(() => setIsLoadingPatient(false))

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
        <Link href="../patients">
          <Button variant="outline" className="mt-4" data-testid="back-to-patients-fallback-btn">
            {t('title')}
          </Button>
        </Link>
      </div>
    )
  }

  return (
    <PageContainer variant="default" data-testid="patient-detail-page">
      <Link href="../patients">
        <Button
          variant="ghost"
          size="sm"
          data-testid="back-to-patients-btn"
          className="-ms-2 group/back text-muted-foreground hover:text-[#061e44]"
        >
          <ArrowLeft className="h-4 w-4 me-1 transition-transform duration-200 ease-in-out group-hover/back:-translate-x-0.5 rtl:group-hover/back:translate-x-0.5" />
          {t('title')}
        </Button>
      </Link>

      {(() => {
        const speciesColor = getSpeciesColor(patient.species)
        return (
          <div
            className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden"
            data-testid="patient-header"
          >
            <div className="flex flex-col md:flex-row">
              {/* Left: Patient & Owner Info */}
              <div className="flex-1 p-6 md:p-8">
                <div className="flex items-start gap-4 mb-6">
                  <div
                    className={`flex h-14 w-14 shrink-0 items-center justify-center rounded-full ${speciesColor.bg} ${speciesColor.text}`}
                    data-testid="patient-species-avatar"
                    aria-label={patient.species}
                  >
                    <SpeciesIcon species={patient.species} className="h-7 w-7" />
                  </div>
                  <div>
                    <h1
                      className="text-[22px] font-bold text-[#061e44]"
                      data-testid="patient-detail-name"
                    >
                      {patient.name}
                    </h1>
                    <p
                      className="text-[13px] text-muted-foreground mt-0.5 font-medium"
                      data-testid="patient-detail-species"
                    >
                      {patient.species} &bull; {patient.breed} &bull;{' '}
                      <span data-testid="patient-detail-age">{calculateAge(patient.dateOfBirth)}</span>
                      {' '}&bull; {patient.gender}
                    </p>
                    <p className="text-[12px] text-muted-foreground mt-0.5" data-testid="patient-weight-display">
                      Weight: {patient.weightKg != null ? `${patient.weightKg} kg` : 'Not recorded'}
                    </p>
                  </div>
                </div>

                {/* Actions row */}
                <div className="flex items-center gap-3">
                  {canWrite && (
                    <>
                      <Button
                        variant="outline"
                        data-testid="edit-patient-btn"
                        onClick={() => setIsEditOpen(true)}
                        className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-[#f4f6f9]"
                      >
                        <Pencil className="h-4 w-4 me-1.5" />
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

                  {role === 'VET' && (
                    <Link href={`/patients/${id}/records/new`}>
                      <Button
                        data-testid="new-medical-record-btn"
                        className="bg-[#303ef5] hover:bg-[#2530c4] text-white font-semibold rounded-xl h-10 px-5 shadow-sm"
                      >
                        New Medical Record
                      </Button>
                    </Link>
                  )}
                </div>
              </div>

              {/* Right: Owner info */}
              <div className="md:w-[280px] bg-[#f4f6f9] p-6 md:p-8 border-t md:border-t-0 md:border-l border-border/50" data-testid="patient-owner-section">
                <h3 className="text-[13px] font-bold text-[#061e44] mb-3">Owner</h3>
                <div className="space-y-3">
                  <span className="text-[14px] font-semibold text-[#061e44] block" data-testid="patient-detail-owner">
                    {patient.ownerName}
                  </span>
                  <div className="flex items-center gap-2.5 text-[13px] text-muted-foreground" data-testid="patient-detail-phone">
                    <Phone className="h-4 w-4 text-muted-foreground" />
                    <span className="font-medium">{patient.ownerPhone}</span>
                  </div>
                  <div className="flex items-center gap-2.5 text-[13px] text-muted-foreground" data-testid="patient-detail-email">
                    <Mail className="h-4 w-4 text-muted-foreground" />
                    <span className="font-medium">{patient.ownerEmail}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )
      })()}

      <div data-testid="patient-tabs">
        <div
          className="flex gap-1 bg-white border border-border/80 rounded-xl shadow-sm px-2"
          role="tablist"
          data-testid="tabs-nav"
        >
          {tabs.map((tab) => (
            <button
              key={tab.id}
              id={`tab-${tab.id}`}
              role="tab"
              aria-selected={activeTab === tab.id}
              aria-controls={`panel-${tab.id}`}
              data-testid={tab.testId}
              onClick={() => setActiveTab(tab.id)}
              className={[
                'px-5 py-3 text-[13px] font-semibold border-b-2 transition-colors',
                activeTab === tab.id
                  ? 'border-[#303ef5] text-[#303ef5]'
                  : 'border-transparent text-muted-foreground hover:text-[#061e44] hover:border-border',
              ].join(' ')}
            >
              {tab.label}
            </button>
          ))}
        </div>

        <div className="mt-4">
          {activeTab === 'medical-records' && (
            <div id="panel-medical-records" role="tabpanel" aria-labelledby="tab-medical-records" data-testid="tabpanel-medical-records" className="animate-in fade-in duration-200">
              <MedicalRecordsList records={records} isLoading={isLoadingRecords} />
            </div>
          )}
          {activeTab === 'prescriptions' && (
            <div id="panel-prescriptions" role="tabpanel" aria-labelledby="tab-prescriptions" data-testid="tabpanel-prescriptions" className="animate-in fade-in duration-200">
              <PrescriptionsTab
                prescriptions={prescriptions}
                patientId={id}
                canPrescribe={role === 'VET'}
              />
            </div>
          )}
          {activeTab === 'vaccinations' && (
            <div id="panel-vaccinations" role="tabpanel" aria-labelledby="tab-vaccinations" data-testid="tabpanel-vaccinations" className="animate-in fade-in duration-200">
              <VaccinationsTab vaccinations={vaccinations} />
            </div>
          )}
        </div>
      </div>
    </PageContainer>
  )
}

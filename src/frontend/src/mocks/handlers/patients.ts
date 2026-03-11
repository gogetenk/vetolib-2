import { http, HttpResponse, delay } from 'msw'
import type { PatientDto, PagedResult, CreatePatientRequest, ImportReportDto } from '@/lib/api/patients'
import type { MedicalRecordDto, CreateMedicalRecordRequest } from '@/lib/api/medical-records'

// UAE-realistic patient data
const MOCK_PATIENTS: PatientDto[] = [
  {
    id: 'pat-0000-0000-0000-000000000001',
    name: 'Max',
    species: 'Dog',
    breed: 'Golden Retriever',
    dateOfBirth: '2019-03-15',
    ageYears: 6,
    gender: 'Male',
    weightKg: 32.5,
    ownerName: 'Ahmed Al-Rashid',
    ownerPhone: '+971 50 123 4567',
    ownerEmail: 'ahmed.alrashid@email.ae',
    clinicId: 'clinic-001',
    lastVisitDate: '2025-11-20',
    nextAppointmentDate: '2026-03-15',
  },
  {
    id: 'pat-0000-0000-0000-000000000002',
    name: 'Luna',
    species: 'Cat',
    breed: 'Siamese',
    dateOfBirth: '2021-07-22',
    ageYears: 4,
    gender: 'Female',
    weightKg: 3.8,
    ownerName: 'Fatima Hassan',
    ownerPhone: '+971 55 987 6543',
    ownerEmail: 'fatima.hassan@email.ae',
    clinicId: 'clinic-001',
    lastVisitDate: '2026-02-10',
    nextAppointmentDate: '2026-04-10',
  },
  {
    id: 'pat-0000-0000-0000-000000000003',
    name: 'Simba',
    species: 'Cat',
    breed: 'Persian',
    dateOfBirth: '2020-01-05',
    ageYears: 6,
    gender: 'Male',
    weightKg: null,
    ownerName: 'Mohammed Al-Farsi',
    ownerPhone: '+971 52 345 6789',
    ownerEmail: 'mohammed.alfarsi@email.ae',
    clinicId: 'clinic-001',
    lastVisitDate: '2026-01-28',
    nextAppointmentDate: null,
  },
  {
    id: 'pat-0000-0000-0000-000000000004',
    name: 'Bella',
    species: 'Dog',
    breed: 'Labrador',
    dateOfBirth: '2022-05-10',
    ageYears: 3,
    gender: 'Female',
    weightKg: 28.0,
    ownerName: 'Sara Al-Mansoori',
    ownerPhone: '+971 56 789 0123',
    ownerEmail: 'sara.almansoori@email.ae',
    clinicId: 'clinic-001',
    lastVisitDate: '2026-02-15',
    nextAppointmentDate: '2026-03-20',
  },
  {
    id: 'pat-0000-0000-0000-000000000005',
    name: 'Zayed',
    species: 'Camel',
    breed: 'Dromedary',
    dateOfBirth: '2018-07-20',
    ageYears: 7,
    gender: 'Male',
    weightKg: 520.0,
    ownerName: 'Khalid Al-Mazrouei',
    ownerPhone: '+971 55 987 6543',
    ownerEmail: 'khalid.almazrouei@email.ae',
    clinicId: 'clinic-001',
    lastVisitDate: '2026-01-10',
    nextAppointmentDate: null,
  },
]

const MOCK_MEDICAL_RECORDS: MedicalRecordDto[] = [
  {
    id: 'rec-0000-0000-0000-000000000001',
    patientId: 'pat-0000-0000-0000-000000000001',
    patientName: 'Max',
    vetName: 'Dr. Sarah Johnson',
    visitDate: '2025-11-20T09:00:00.000Z',
    reason: 'Annual vaccination',
    anamnesis: 'Patient is up to date on vaccinations. Owner reports normal appetite and activity.',
    weight: 32.5,
    temperature: 38.6,
    heartRate: 80,
    diagnosis: 'Healthy — routine annual check',
    treatment: 'Administered DHPP booster and rabies vaccine',
    prescription: null,
    nextVisitDate: '2026-11-20',
    clinicId: 'clinic-001',
  },
  {
    id: 'rec-0000-0000-0000-000000000002',
    patientId: 'pat-0000-0000-0000-000000000001',
    patientName: 'Max',
    vetName: 'Dr. Ahmed Khalil',
    visitDate: '2025-06-10T10:30:00.000Z',
    reason: 'Limping — left hind leg',
    anamnesis: 'Owner noticed limping for 2 days. Dog is otherwise active and eating well.',
    weight: 33.0,
    temperature: 38.8,
    heartRate: 85,
    diagnosis: 'Mild sprain — left hind leg',
    treatment: 'Rest for 5 days, NSAID pain relief',
    prescription: 'Meloxicam 15mg — once daily for 5 days',
    nextVisitDate: '2025-06-20',
    clinicId: 'clinic-001',
  },
  {
    id: 'rec-0000-0000-0000-000000000003',
    patientId: 'pat-0000-0000-0000-000000000001',
    patientName: 'Max',
    vetName: 'Dr. Sarah Johnson',
    visitDate: '2024-12-05T11:00:00.000Z',
    reason: 'Skin irritation',
    anamnesis: 'Persistent scratching on flank area for a week.',
    weight: 32.0,
    temperature: 38.5,
    heartRate: 78,
    diagnosis: 'Allergic dermatitis — environmental allergens',
    treatment: 'Medicated shampoo twice weekly, antihistamine course',
    prescription: 'Diphenhydramine 25mg — twice daily for 10 days',
    nextVisitDate: null,
    clinicId: 'clinic-001',
  },
  {
    id: 'rec-0000-0000-0000-000000000004',
    patientId: 'pat-0000-0000-0000-000000000002',
    patientName: 'Luna',
    vetName: 'Dr. Ahmed Khalil',
    visitDate: '2026-02-10T14:00:00.000Z',
    reason: 'Skin condition follow-up',
    anamnesis: 'Follow-up after previous skin treatment. Improvement noted.',
    weight: 3.2,
    temperature: 38.5,
    heartRate: 140,
    diagnosis: 'Mild dehydration',
    treatment: 'IV fluids, recheck in 3 days',
    prescription: null,
    nextVisitDate: '2026-02-13',
    clinicId: 'clinic-001',
  },
]

const MOCK_VACCINATIONS: Record<string, VaccinationDto[]> = {
  'pat-0000-0000-0000-000000000001': [
    { id: 'vac-001', name: 'DHPP (Distemper, Hepatitis, Parvovirus, Parainfluenza)', administeredDate: '2025-11-20', nextDueDate: '2026-11-20', vetName: 'Dr. Sarah Johnson' },
    { id: 'vac-002', name: 'Rabies', administeredDate: '2025-11-20', nextDueDate: '2026-11-20', vetName: 'Dr. Sarah Johnson' },
    { id: 'vac-003', name: 'Bordetella', administeredDate: '2025-05-15', nextDueDate: '2026-05-15', vetName: 'Dr. Ahmed Khalil' },
  ],
  'pat-0000-0000-0000-000000000002': [
    { id: 'vac-004', name: 'FVRCP (Feline Viral Rhinotracheitis, Calicivirus, Panleukopenia)', administeredDate: '2025-09-01', nextDueDate: '2026-09-01', vetName: 'Dr. Sarah Johnson' },
    { id: 'vac-005', name: 'Rabies', administeredDate: '2025-09-01', nextDueDate: '2026-09-01', vetName: 'Dr. Sarah Johnson' },
  ],
}

const MOCK_PRESCRIPTIONS: Record<string, PrescriptionDto[]> = {
  'pat-0000-0000-0000-000000000001': [
    { id: 'presc-001', medication: 'Meloxicam 15mg', dosage: 'Once daily', duration: '5 days', prescribedDate: '2025-06-10', status: 'completed', vetName: 'Dr. Ahmed Khalil' },
    { id: 'presc-002', medication: 'Diphenhydramine 25mg', dosage: 'Twice daily', duration: '10 days', prescribedDate: '2024-12-05', status: 'completed', vetName: 'Dr. Sarah Johnson' },
  ],
  'pat-0000-0000-0000-000000000002': [],
}

// Types for mock data (not exported — internal to handlers)
interface VaccinationDto {
  id: string
  name: string
  administeredDate: string
  nextDueDate: string
  vetName: string
}

interface PrescriptionDto {
  id: string
  medication: string
  dosage: string
  duration: string
  prescribedDate: string
  status: 'active' | 'completed'
  vetName: string
}

export const patientHandlers = [
  // GET /api/patients
  http.get('/api/patients', ({ request }) => {
    const url = new URL(request.url)
    const search = url.searchParams.get('search')?.toLowerCase()
    const items = search
      ? MOCK_PATIENTS.filter(
          p =>
            p.name.toLowerCase().includes(search) ||
            p.ownerName.toLowerCase().includes(search)
        )
      : MOCK_PATIENTS

    return HttpResponse.json<PagedResult<PatientDto>>({
      items,
      totalCount: items.length,
      page: 1,
      pageSize: 20,
    })
  }),

  // GET /api/patients/:id
  http.get('/api/patients/:id', ({ params }) => {
    const patient = MOCK_PATIENTS.find(p => p.id === params.id)
    if (!patient) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(patient)
  }),

  // GET /api/patients/:id/medical-records
  http.get('/api/patients/:id/medical-records', ({ params }) => {
    const records = MOCK_MEDICAL_RECORDS.filter(r => r.patientId === params.id)
    return HttpResponse.json<PagedResult<MedicalRecordDto>>({
      items: records,
      totalCount: records.length,
      page: 1,
      pageSize: 20,
    })
  }),

  // POST /api/patients/:id/medical-records
  http.post('/api/patients/:id/medical-records', async ({ params, request }) => {
    const body = await request.json() as CreateMedicalRecordRequest
    const patient = MOCK_PATIENTS.find(p => p.id === params.id)
    if (!patient) return new HttpResponse(null, { status: 404 })

    const newRecord: MedicalRecordDto = {
      id: crypto.randomUUID(),
      patientId: params.id as string,
      patientName: patient.name,
      vetName: 'Dr. Sarah Johnson',
      visitDate: new Date().toISOString(),
      reason: body.reason,
      anamnesis: body.anamnesis,
      weight: body.weight,
      temperature: body.temperature,
      heartRate: body.heartRate,
      diagnosis: body.diagnosis,
      treatment: body.treatment,
      prescription: body.prescription ?? null,
      nextVisitDate: body.nextVisitDate ?? null,
      clinicId: 'clinic-001',
    }
    MOCK_MEDICAL_RECORDS.unshift(newRecord)
    return HttpResponse.json(newRecord, { status: 201 })
  }),

  // GET /api/patients/:id/vaccinations
  http.get('/api/patients/:id/vaccinations', ({ params }) => {
    const vaccinations = MOCK_VACCINATIONS[params.id as string] ?? []
    return HttpResponse.json(vaccinations)
  }),

  // GET /api/patients/:id/prescriptions
  http.get('/api/patients/:id/prescriptions', ({ params }) => {
    const prescriptions = MOCK_PRESCRIPTIONS[params.id as string] ?? []
    return HttpResponse.json(prescriptions)
  }),

  // POST /api/patients
  http.post('/api/patients', async ({ request }) => {
    const body = await request.json() as CreatePatientRequest
    const birthDate = new Date(body.dateOfBirth)
    const now = new Date()
    const ageYears = now.getFullYear() - birthDate.getFullYear() -
      (now < new Date(now.getFullYear(), birthDate.getMonth(), birthDate.getDate()) ? 1 : 0)

    const newPatient: PatientDto = {
      id: crypto.randomUUID(),
      name: body.name,
      species: body.species,
      breed: body.breed ?? '',
      dateOfBirth: body.dateOfBirth,
      ageYears,
      gender: body.gender,
      weightKg: body.weightKg ?? null,
      ownerName: body.ownerName,
      ownerPhone: body.ownerPhone,
      ownerEmail: body.ownerEmail ?? '',
      clinicId: 'clinic-001',
      lastVisitDate: null,
      nextAppointmentDate: null,
    }
    MOCK_PATIENTS.push(newPatient)
    return HttpResponse.json(newPatient, { status: 201 })
  }),

  // POST /api/patients/import — upload CSV, returns import report
  http.post('/api/patients/import', async () => {
    await delay(800)
    const report: ImportReportDto = {
      imported: 12,
      skipped: 2,
      errors: [
        'Row 4: Missing required field "ownerPhone"',
        'Row 9: Invalid species "Tortoise" — must be one of: Dog, Cat, Bird, Rabbit, Horse, Camel, Exotic',
      ],
    }
    return HttpResponse.json<ImportReportDto>(report, { status: 200 })
  }),

  // GET /api/patients/import/template — download CSV template
  http.get('/api/patients/import/template', async () => {
    await delay(200)
    const csvContent = [
      'name,species,breed,dateOfBirth,gender,weightKg,ownerName,ownerPhone,ownerEmail',
      'Max,Dog,Golden Retriever,2019-03-15,Male,32.5,Ahmed Al-Rashid,+971501234567,ahmed@email.ae',
      'Luna,Cat,Siamese,2021-07-22,Female,3.8,Fatima Hassan,+971559876543,fatima@email.ae',
    ].join('\n')

    return new HttpResponse(csvContent, {
      status: 200,
      headers: {
        'Content-Type': 'text/csv',
        'Content-Disposition': 'attachment; filename="patient-import-template.csv"',
      },
    })
  }),

  // PATCH /api/patients/:id
  http.patch('/api/patients/:id', async ({ params, request }) => {
    const body = await request.json() as Partial<CreatePatientRequest>
    const patient = MOCK_PATIENTS.find(p => p.id === params.id)
    if (!patient) return new HttpResponse(null, { status: 404 })

    if (body.name !== undefined) patient.name = body.name
    if (body.species !== undefined) patient.species = body.species
    if (body.breed !== undefined) patient.breed = body.breed ?? ''
    if (body.gender !== undefined) patient.gender = body.gender
    if (body.weightKg !== undefined) patient.weightKg = body.weightKg ?? null
    if (body.ownerName !== undefined) patient.ownerName = body.ownerName
    if (body.ownerPhone !== undefined) patient.ownerPhone = body.ownerPhone
    if (body.ownerEmail !== undefined) patient.ownerEmail = body.ownerEmail ?? ''
    if (body.dateOfBirth !== undefined) {
      patient.dateOfBirth = body.dateOfBirth
      const birthDate = new Date(body.dateOfBirth)
      const now = new Date()
      patient.ageYears = now.getFullYear() - birthDate.getFullYear() -
        (now < new Date(now.getFullYear(), birthDate.getMonth(), birthDate.getDate()) ? 1 : 0)
    }

    return HttpResponse.json(patient)
  }),
]

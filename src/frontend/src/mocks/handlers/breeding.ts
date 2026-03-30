import { http, HttpResponse } from 'msw'
import type {
  LitterDto,
  OffspringDto,
  CreateLitterRequest,
  AddOffspringRequest,
  LineageDto,
  PedigreeNodeDto,
  PregnancyDto,
  PregnancyCheckDto,
  CreatePregnancyRequest,
  RecordDeliveryRequest,
  RecordLossRequest,
  CreatePregnancyCheckRequest,
  UpdatePregnancyCheckRequest,
  HeatCycleDto,
  HeatCyclePredictionDto,
  CreateHeatCycleRequest,
  SetLineageRequest,
} from '@/lib/api/breeding'

// --- Mock Data: Litters ---
const MOCK_LITTERS: LitterDto[] = [
  {
    id: 'lit-0000-0000-0000-000000000001',
    motherId: 'pat-0000-0000-0000-000000000004', // Bella (Labrador, Female)
    motherName: 'Bella',
    fatherId: 'pat-0000-0000-0000-000000000001', // Max (Golden Retriever, Male)
    fatherName: 'Max',
    dateOfBirth: '2025-09-10',
    species: 'Dog',
    breed: 'Golden Labrador Mix',
    offspringCount: 6,
    notes: 'Healthy litter, all puppies strong',
    clinicId: 'clinic-001',
  },
  {
    id: 'lit-0000-0000-0000-000000000002',
    motherId: 'pat-0000-0000-0000-000000000002', // Luna (Siamese, Female)
    motherName: 'Luna',
    fatherId: null,
    fatherName: null,
    dateOfBirth: '2026-01-20',
    species: 'Cat',
    breed: 'Siamese',
    offspringCount: 4,
    notes: 'Father unknown — stray mating',
    clinicId: 'clinic-001',
  },
]

// --- Mock Data: Offspring ---
const MOCK_OFFSPRING: Record<string, OffspringDto[]> = {
  'lit-0000-0000-0000-000000000001': [
    { id: 'off-001', litterId: 'lit-0000-0000-0000-000000000001', patientId: null, name: 'Buddy', sex: 'Male', colorMarkings: 'Golden', weightAtBirthKg: 0.45, status: 'Adopted' },
    { id: 'off-002', litterId: 'lit-0000-0000-0000-000000000001', patientId: null, name: 'Daisy', sex: 'Female', colorMarkings: 'Light brown', weightAtBirthKg: 0.42, status: 'Adopted' },
    { id: 'off-003', litterId: 'lit-0000-0000-0000-000000000001', patientId: null, name: 'Rocky', sex: 'Male', colorMarkings: 'Golden', weightAtBirthKg: 0.48, status: 'Alive' },
    { id: 'off-004', litterId: 'lit-0000-0000-0000-000000000001', patientId: null, name: 'Luna Jr', sex: 'Female', colorMarkings: 'Cream', weightAtBirthKg: 0.40, status: 'Alive' },
    { id: 'off-005', litterId: 'lit-0000-0000-0000-000000000001', patientId: null, name: 'Charlie', sex: 'Male', colorMarkings: 'Dark golden', weightAtBirthKg: 0.50, status: 'Alive' },
    { id: 'off-006', litterId: 'lit-0000-0000-0000-000000000001', patientId: null, name: 'Nala', sex: 'Female', colorMarkings: 'Tan', weightAtBirthKg: 0.38, status: 'Deceased' },
  ],
  'lit-0000-0000-0000-000000000002': [
    { id: 'off-010', litterId: 'lit-0000-0000-0000-000000000002', patientId: null, name: 'Misty', sex: 'Female', colorMarkings: 'Seal point', weightAtBirthKg: 0.10, status: 'Alive' },
    { id: 'off-011', litterId: 'lit-0000-0000-0000-000000000002', patientId: null, name: 'Shadow', sex: 'Male', colorMarkings: 'Chocolate point', weightAtBirthKg: 0.11, status: 'Alive' },
    { id: 'off-012', litterId: 'lit-0000-0000-0000-000000000002', patientId: null, name: 'Pearl', sex: 'Female', colorMarkings: 'Blue point', weightAtBirthKg: 0.09, status: 'Adopted' },
    { id: 'off-013', litterId: 'lit-0000-0000-0000-000000000002', patientId: null, name: 'Storm', sex: 'Male', colorMarkings: 'Seal point', weightAtBirthKg: 0.12, status: 'Alive' },
  ],
}

// --- Mock Data: Lineage ---
const MOCK_LINEAGES: Record<string, LineageDto> = {
  'pat-0000-0000-0000-000000000001': {
    patientId: 'pat-0000-0000-0000-000000000001',
    patientName: 'Max',
    motherId: null,
    motherName: null,
    fatherId: null,
    fatherName: null,
  },
  'pat-0000-0000-0000-000000000004': {
    patientId: 'pat-0000-0000-0000-000000000004',
    patientName: 'Bella',
    motherId: null,
    motherName: null,
    fatherId: null,
    fatherName: null,
  },
}

// --- Mock Data: Pedigree (3-gen tree) ---
function getMockPedigree(patientId: string): PedigreeNodeDto | null {
  if (patientId === 'pat-0000-0000-0000-000000000001') {
    return {
      id: 'pat-0000-0000-0000-000000000001',
      name: 'Max',
      species: 'Dog',
      breed: 'Golden Retriever',
      sex: 'Male',
      mother: {
        id: 'ped-mother-max',
        name: 'Layla',
        species: 'Dog',
        breed: 'Golden Retriever',
        sex: 'Female',
        mother: {
          id: 'ped-gm-max-m',
          name: 'Sahara',
          species: 'Dog',
          breed: 'Golden Retriever',
          sex: 'Female',
          mother: null,
          father: null,
        },
        father: {
          id: 'ped-gf-max-m',
          name: 'Sultan',
          species: 'Dog',
          breed: 'Golden Retriever',
          sex: 'Male',
          mother: null,
          father: null,
        },
      },
      father: {
        id: 'ped-father-max',
        name: 'Rashid',
        species: 'Dog',
        breed: 'Golden Retriever',
        sex: 'Male',
        mother: {
          id: 'ped-gm-max-f',
          name: 'Jasmine',
          species: 'Dog',
          breed: 'Golden Retriever',
          sex: 'Female',
          mother: null,
          father: null,
        },
        father: {
          id: 'ped-gf-max-f',
          name: 'Thunder',
          species: 'Dog',
          breed: 'Golden Retriever',
          sex: 'Male',
          mother: null,
          father: null,
        },
      },
    }
  }
  if (patientId === 'pat-0000-0000-0000-000000000004') {
    return {
      id: 'pat-0000-0000-0000-000000000004',
      name: 'Bella',
      species: 'Dog',
      breed: 'Labrador',
      sex: 'Female',
      mother: {
        id: 'ped-mother-bella',
        name: 'Amber',
        species: 'Dog',
        breed: 'Labrador',
        sex: 'Female',
        mother: null,
        father: null,
      },
      father: {
        id: 'ped-father-bella',
        name: 'Duke',
        species: 'Dog',
        breed: 'Labrador',
        sex: 'Male',
        mother: null,
        father: null,
      },
    }
  }
  return null
}

// --- Mock Data: Pregnancies ---
const MOCK_PREGNANCIES: PregnancyDto[] = [
  {
    id: 'preg-0000-0000-0000-000000000001',
    patientId: 'pat-0000-0000-0000-000000000004', // Bella
    patientName: 'Bella',
    matingDate: '2025-07-05',
    expectedDueDate: '2025-09-10',
    actualDeliveryDate: '2025-09-10',
    status: 'Delivered',
    notes: 'Successful delivery of 6 puppies',
    checks: [
      { id: 'pc-001', pregnancyId: 'preg-0000-0000-0000-000000000001', checkDate: '2025-07-25', notes: 'Ultrasound confirmed pregnancy', result: 'Positive', vetName: 'Dr. Sarah Johnson' },
      { id: 'pc-002', pregnancyId: 'preg-0000-0000-0000-000000000001', checkDate: '2025-08-15', notes: 'X-ray — 6 fetuses visible', result: 'Normal', vetName: 'Dr. Ahmed Khalil' },
      { id: 'pc-003', pregnancyId: 'preg-0000-0000-0000-000000000001', checkDate: '2025-09-01', notes: 'Pre-delivery check — all healthy', result: 'Normal', vetName: 'Dr. Sarah Johnson' },
    ],
    clinicId: 'clinic-001',
  },
  {
    id: 'preg-0000-0000-0000-000000000002',
    patientId: 'pat-0000-0000-0000-000000000002', // Luna
    patientName: 'Luna',
    matingDate: '2025-11-15',
    expectedDueDate: '2026-01-20',
    actualDeliveryDate: '2026-01-20',
    status: 'Delivered',
    notes: 'Delivered 4 kittens naturally',
    checks: [
      { id: 'pc-004', pregnancyId: 'preg-0000-0000-0000-000000000002', checkDate: '2025-12-05', notes: 'Ultrasound — 4 kittens visible', result: 'Normal', vetName: 'Dr. Ahmed Khalil' },
    ],
    clinicId: 'clinic-001',
  },
  {
    id: 'preg-0000-0000-0000-000000000003',
    patientId: 'pat-0000-0000-0000-000000000004', // Bella — active pregnancy
    patientName: 'Bella',
    matingDate: '2026-02-01',
    expectedDueDate: '2026-04-05',
    actualDeliveryDate: null,
    status: 'Active',
    notes: 'Second pregnancy, monitoring closely',
    checks: [
      { id: 'pc-005', pregnancyId: 'preg-0000-0000-0000-000000000003', checkDate: '2026-02-20', notes: 'Early ultrasound — pregnancy confirmed', result: 'Positive', vetName: 'Dr. Sarah Johnson' },
    ],
    clinicId: 'clinic-001',
  },
]

// --- Mock Data: Heat Cycles ---
const MOCK_HEAT_CYCLES: HeatCycleDto[] = [
  {
    id: 'hc-0000-0000-0000-000000000001',
    patientId: 'pat-0000-0000-0000-000000000004', // Bella
    patientName: 'Bella',
    startDate: '2025-01-05',
    endDate: '2025-01-26',
    phase: 'Anestrus',
    intensity: 'Medium',
    notes: 'Regular cycle',
    recordedBy: 'Dr. Sarah Johnson',
    clinicId: 'clinic-001',
  },
  {
    id: 'hc-0000-0000-0000-000000000002',
    patientId: 'pat-0000-0000-0000-000000000004', // Bella
    patientName: 'Bella',
    startDate: '2025-07-01',
    endDate: '2025-07-22',
    phase: 'Anestrus',
    intensity: 'High',
    notes: 'Mating occurred during estrus',
    recordedBy: 'Dr. Ahmed Khalil',
    clinicId: 'clinic-001',
  },
  {
    id: 'hc-0000-0000-0000-000000000003',
    patientId: 'pat-0000-0000-0000-000000000002', // Luna
    patientName: 'Luna',
    startDate: '2025-11-10',
    endDate: '2025-11-17',
    phase: 'Anestrus',
    intensity: 'Low',
    notes: 'Short cycle',
    recordedBy: 'Dr. Sarah Johnson',
    clinicId: 'clinic-001',
  },
  {
    id: 'hc-0000-0000-0000-000000000004',
    patientId: 'pat-0000-0000-0000-000000000004', // Bella — recent
    patientName: 'Bella',
    startDate: '2026-01-15',
    endDate: '2026-02-05',
    phase: 'Anestrus',
    intensity: 'Medium',
    notes: 'Pre-mating cycle',
    recordedBy: 'Dr. Sarah Johnson',
    clinicId: 'clinic-001',
  },
]

const MOCK_PREDICTIONS: Record<string, HeatCyclePredictionDto> = {
  'pat-0000-0000-0000-000000000004': {
    patientId: 'pat-0000-0000-0000-000000000004',
    predictedNextStartDate: '2026-07-15',
    averageCycleDays: 183,
    confidence: 'Medium',
  },
  'pat-0000-0000-0000-000000000002': {
    patientId: 'pat-0000-0000-0000-000000000002',
    predictedNextStartDate: '2026-05-10',
    averageCycleDays: 180,
    confidence: 'Low',
  },
}

export const breedingHandlers = [
  // === Litters ===

  // POST /api/v1/litters
  http.post('/api/v1/litters', async ({ request }) => {
    const body = (await request.json()) as CreateLitterRequest
    const newLitter: LitterDto = {
      id: `lit-${crypto.randomUUID().slice(0, 8)}`,
      motherId: body.motherPatientId,
      motherName: 'Unknown',
      fatherId: body.fatherPatientId ?? null,
      fatherName: body.fatherPatientId ? 'Unknown' : (body.externalFatherName ?? null),
      dateOfBirth: body.birthDate,
      species: 'Dog',
      breed: null,
      offspringCount: body.bornCount,
      notes: body.notes ?? null,
      clinicId: 'clinic-001',
    }
    MOCK_LITTERS.push(newLitter)
    return HttpResponse.json(newLitter, { status: 201 })
  }),

  // GET /api/v1/litters
  http.get('/api/v1/litters', () => {
    return HttpResponse.json(MOCK_LITTERS)
  }),

  // GET /api/v1/patients/:id/litters
  http.get('/api/v1/patients/:id/litters', ({ params }) => {
    const patientId = params.id as string
    const filtered = MOCK_LITTERS.filter(l => l.motherId === patientId || l.fatherId === patientId)
    return HttpResponse.json(filtered)
  }),

  // GET /api/v1/litters/:id
  http.get('/api/v1/litters/:id', ({ params }) => {
    const litter = MOCK_LITTERS.find(l => l.id === params.id)
    if (!litter) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(litter)
  }),

  // POST /api/v1/litters/:id/offspring
  http.post('/api/v1/litters/:id/offspring', async ({ params, request }) => {
    const litterId = params.id as string
    const litter = MOCK_LITTERS.find(l => l.id === litterId)
    if (!litter) return new HttpResponse(null, { status: 404 })

    const body = (await request.json()) as AddOffspringRequest
    const newOffspring: OffspringDto = {
      id: `off-${crypto.randomUUID().slice(0, 8)}`,
      litterId,
      patientId: null,
      name: body.name,
      sex: body.sex,
      colorMarkings: body.colorMarkings ?? null,
      weightAtBirthKg: body.weightAtBirthKg ?? null,
      status: 'Alive',
    }

    if (!MOCK_OFFSPRING[litterId]) {
      MOCK_OFFSPRING[litterId] = []
    }
    MOCK_OFFSPRING[litterId].push(newOffspring)
    litter.offspringCount += 1

    return HttpResponse.json(newOffspring, { status: 201 })
  }),

  // GET /api/v1/litters/:id/offspring
  http.get('/api/v1/litters/:id/offspring', ({ params }) => {
    const offspring = MOCK_OFFSPRING[params.id as string] ?? []
    return HttpResponse.json(offspring)
  }),

  // === Lineage ===

  // GET /api/v1/patients/:id/lineage
  http.get('/api/v1/patients/:id/lineage', ({ params }) => {
    const patientId = params.id as string
    const lineage = MOCK_LINEAGES[patientId] ?? {
      patientId,
      patientName: 'Unknown',
      motherId: null,
      motherName: null,
      fatherId: null,
      fatherName: null,
    }
    return HttpResponse.json(lineage)
  }),

  // PUT /api/v1/patients/:id/lineage
  http.put('/api/v1/patients/:id/lineage', async ({ params, request }) => {
    const patientId = params.id as string
    const body = (await request.json()) as SetLineageRequest
    const existing = MOCK_LINEAGES[patientId] ?? {
      patientId,
      patientName: 'Unknown',
      motherId: null,
      motherName: null,
      fatherId: null,
      fatherName: null,
    }
    if (body.motherId !== undefined) {
      existing.motherId = body.motherId ?? null
      existing.motherName = body.motherId ? 'Set Mother' : null
    }
    if (body.fatherId !== undefined) {
      existing.fatherId = body.fatherId ?? null
      existing.fatherName = body.fatherId ? 'Set Father' : null
    }
    MOCK_LINEAGES[patientId] = existing
    return HttpResponse.json(existing)
  }),

  // GET /api/v1/patients/:id/pedigree
  http.get('/api/v1/patients/:id/pedigree', ({ params }) => {
    const pedigree = getMockPedigree(params.id as string)
    if (!pedigree) {
      return HttpResponse.json({
        id: params.id as string,
        name: 'Unknown',
        species: 'Dog',
        breed: null,
        sex: 'Unknown' as const,
        mother: null,
        father: null,
      })
    }
    return HttpResponse.json(pedigree)
  }),

  // GET /api/v1/patients/:id/descendants
  http.get('/api/v1/patients/:id/descendants', ({ params }) => {
    const patientId = params.id as string
    // Find litters where this patient is a parent
    const litters = MOCK_LITTERS.filter(l => l.motherId === patientId || l.fatherId === patientId)
    const descendants: PedigreeNodeDto[] = litters.flatMap(l => {
      const offspring = MOCK_OFFSPRING[l.id] ?? []
      return offspring.map(o => ({
        id: o.id,
        name: o.name,
        species: l.species,
        breed: l.breed,
        sex: o.sex,
        mother: null,
        father: null,
      }))
    })
    return HttpResponse.json(descendants)
  }),

  // === Pregnancy ===

  // POST /api/v1/breeding/pregnancies
  http.post('/api/v1/breeding/pregnancies', async ({ request }) => {
    const body = (await request.json()) as CreatePregnancyRequest
    const newPregnancy: PregnancyDto = {
      id: `preg-${crypto.randomUUID().slice(0, 8)}`,
      patientId: body.patientId,
      patientName: 'Patient',
      matingDate: body.matingDate,
      expectedDueDate: body.expectedDueDate,
      actualDeliveryDate: null,
      status: 'Active',
      notes: body.notes ?? null,
      checks: [],
      clinicId: 'clinic-001',
    }
    MOCK_PREGNANCIES.push(newPregnancy)
    return HttpResponse.json(newPregnancy, { status: 201 })
  }),

  // GET /api/v1/breeding/pregnancies/active (must be before :id to avoid conflict)
  http.get('/api/v1/breeding/pregnancies/active', () => {
    const active = MOCK_PREGNANCIES.filter(p => p.status === 'Active')
    return HttpResponse.json(active)
  }),

  // GET /api/v1/breeding/pregnancies/:id
  http.get('/api/v1/breeding/pregnancies/:id', ({ params }) => {
    const pregnancy = MOCK_PREGNANCIES.find(p => p.id === params.id)
    if (!pregnancy) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(pregnancy)
  }),

  // GET /api/v1/breeding/pregnancies (with patientId filter)
  http.get('/api/v1/breeding/pregnancies', ({ request }) => {
    const url = new URL(request.url)
    const patientId = url.searchParams.get('patientId')
    const filtered = patientId
      ? MOCK_PREGNANCIES.filter(p => p.patientId === patientId)
      : MOCK_PREGNANCIES
    return HttpResponse.json(filtered)
  }),

  // PUT /api/v1/breeding/pregnancies/:id/delivery
  http.put('/api/v1/breeding/pregnancies/:id/delivery', async ({ params, request }) => {
    const pregnancy = MOCK_PREGNANCIES.find(p => p.id === params.id)
    if (!pregnancy) return new HttpResponse(null, { status: 404 })
    const body = (await request.json()) as RecordDeliveryRequest
    pregnancy.status = 'Delivered'
    pregnancy.actualDeliveryDate = body.actualDeliveryDate
    if (body.notes) pregnancy.notes = body.notes
    return HttpResponse.json(pregnancy)
  }),

  // PUT /api/v1/breeding/pregnancies/:id/loss
  http.put('/api/v1/breeding/pregnancies/:id/loss', async ({ params, request }) => {
    const pregnancy = MOCK_PREGNANCIES.find(p => p.id === params.id)
    if (!pregnancy) return new HttpResponse(null, { status: 404 })
    const body = (await request.json()) as RecordLossRequest
    pregnancy.status = 'Lost'
    if (body.notes) pregnancy.notes = body.notes
    return HttpResponse.json(pregnancy)
  }),

  // POST /api/v1/breeding/pregnancies/:id/checks
  http.post('/api/v1/breeding/pregnancies/:id/checks', async ({ params, request }) => {
    const pregnancy = MOCK_PREGNANCIES.find(p => p.id === params.id)
    if (!pregnancy) return new HttpResponse(null, { status: 404 })
    const body = (await request.json()) as CreatePregnancyCheckRequest
    const newCheck: PregnancyCheckDto = {
      id: `pc-${crypto.randomUUID().slice(0, 8)}`,
      pregnancyId: params.id as string,
      checkDate: body.checkDate,
      notes: body.notes,
      result: body.result,
      vetName: 'Dr. Sarah Johnson',
    }
    pregnancy.checks.push(newCheck)
    return HttpResponse.json(newCheck, { status: 201 })
  }),

  // PUT /api/v1/breeding/pregnancies/:id/checks/:checkId
  http.put('/api/v1/breeding/pregnancies/:id/checks/:checkId', async ({ params, request }) => {
    const pregnancy = MOCK_PREGNANCIES.find(p => p.id === params.id)
    if (!pregnancy) return new HttpResponse(null, { status: 404 })
    const check = pregnancy.checks.find(c => c.id === params.checkId)
    if (!check) return new HttpResponse(null, { status: 404 })
    const body = (await request.json()) as UpdatePregnancyCheckRequest
    if (body.notes !== undefined) check.notes = body.notes
    if (body.result !== undefined) check.result = body.result
    return HttpResponse.json(check)
  }),

  // === Heat Cycles ===

  // POST /api/v1/patients/heat-cycles
  http.post('/api/v1/patients/heat-cycles', async ({ request }) => {
    const body = (await request.json()) as CreateHeatCycleRequest
    const newCycle: HeatCycleDto = {
      id: `hc-${crypto.randomUUID().slice(0, 8)}`,
      patientId: body.patientId,
      patientName: 'Patient',
      startDate: body.startDate,
      endDate: body.endDate ?? null,
      phase: body.phase,
      intensity: body.intensity,
      notes: body.notes ?? null,
      recordedBy: 'Dr. Sarah Johnson',
      clinicId: 'clinic-001',
    }
    MOCK_HEAT_CYCLES.push(newCycle)
    return HttpResponse.json(newCycle, { status: 201 })
  }),

  // GET /api/v1/patients/:id/heat-cycles
  http.get('/api/v1/patients/:id/heat-cycles', ({ params }) => {
    const patientId = params.id as string
    // Check if this is a prediction sub-route (handled separately)
    const filtered = MOCK_HEAT_CYCLES.filter(c => c.patientId === patientId)
    const sorted = [...filtered].sort(
      (a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime()
    )
    return HttpResponse.json(sorted)
  }),

  // GET /api/v1/patients/heat-cycles (all heat cycles)
  http.get('/api/v1/patients/heat-cycles', () => {
    const sorted = [...MOCK_HEAT_CYCLES].sort(
      (a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime()
    )
    return HttpResponse.json(sorted)
  }),

  // GET /api/v1/patients/:id/heat-cycles/prediction
  http.get('/api/v1/patients/:id/heat-cycles/prediction', ({ params }) => {
    const patientId = params.id as string
    if (!MOCK_PREDICTIONS[patientId]) {
      return HttpResponse.json(
        { title: 'Not enough data for prediction' },
        { status: 404 }
      )
    }
    return HttpResponse.json(MOCK_PREDICTIONS[patientId])
  }),
]

import { http, HttpResponse } from 'msw'
import type {
  DrugCatalogEntryDto,
  PrescriptionPreflightResult,
  PreflightRequest,
  StockAvailabilityResult,
} from '@/lib/api/types'

// Realistic UAE veterinary drug catalog (INN names)
const MOCK_DRUGS: DrugCatalogEntryDto[] = [
  {
    id: 'drug-0000-0000-0000-000000000001',
    innName: 'amoxicillin',
    displayName: 'Amoxicillin 250mg',
    category: 'Antibiotic',
    commonDosage: '10–20 mg/kg twice daily for 5–7 days',
    contraindicatedSpecies: [
      { species: 'Rabbit', reason: 'Disrupts cecal flora — potentially fatal' },
    ],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 15, unit: 'mg', frequency: 'BID', maxDurationDays: 7 },
      { species: 'Cat', dosePerKg: 10, unit: 'mg', frequency: 'BID', maxDurationDays: 7 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000002',
    innName: 'meloxicam',
    displayName: 'Meloxicam 1.5mg/ml',
    category: 'AntiInflammatory',
    commonDosage: '0.1 mg/kg once daily — taper after 3 days',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 0.1, unit: 'mg', frequency: 'SID', maxDurationDays: 5 },
      { species: 'Cat', dosePerKg: 0.05, unit: 'mg', frequency: 'SID', maxDurationDays: 3 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000003',
    innName: 'metronidazole',
    displayName: 'Metronidazole 250mg',
    category: 'Antibiotic',
    commonDosage: '10–25 mg/kg twice daily for 5 days',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 15, unit: 'mg', frequency: 'BID', maxDurationDays: 5 },
      { species: 'Cat', dosePerKg: 10, unit: 'mg', frequency: 'BID', maxDurationDays: 5 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000004',
    innName: 'ivermectin',
    displayName: 'Ivermectin 1% injection',
    category: 'Antiparasitic',
    commonDosage: '0.2 mg/kg SC once, repeat in 2 weeks if needed',
    contraindicatedSpecies: [
      { species: 'Dog', reason: 'Collie breeds: MDR1 mutation risk — use cautiously' },
    ],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 0.2, unit: 'mg', frequency: 'once', maxDurationDays: null },
      { species: 'Cat', dosePerKg: 0.2, unit: 'mg', frequency: 'once', maxDurationDays: null },
      { species: 'Camel', dosePerKg: 0.2, unit: 'mg', frequency: 'once', maxDurationDays: null },
    ],
    interactionSeverity: 'Moderate',
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000005',
    innName: 'prednisolone',
    displayName: 'Prednisolone 5mg',
    category: 'AntiInflammatory',
    commonDosage: '1–2 mg/kg once daily, taper over 2 weeks',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 1, unit: 'mg', frequency: 'SID', maxDurationDays: 14 },
      { species: 'Cat', dosePerKg: 2, unit: 'mg', frequency: 'SID', maxDurationDays: 14 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000006',
    innName: 'diphenhydramine',
    displayName: 'Diphenhydramine 25mg',
    category: 'AntiInflammatory',
    commonDosage: '1 mg/kg twice daily for 5–10 days',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 1, unit: 'mg', frequency: 'BID', maxDurationDays: 10 },
    ],
    interactionSeverity: null,
    requiresPrescription: false,
  },
  {
    id: 'drug-0000-0000-0000-000000000007',
    innName: 'enrofloxacin',
    displayName: 'Enrofloxacin 50mg',
    category: 'Antibiotic',
    commonDosage: '5–10 mg/kg once daily for 7 days',
    contraindicatedSpecies: [
      { species: 'Rabbit', reason: 'Use with extreme caution — monitor closely' },
    ],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 5, unit: 'mg', frequency: 'SID', maxDurationDays: 7 },
      { species: 'Cat', dosePerKg: 5, unit: 'mg', frequency: 'SID', maxDurationDays: 7 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000008',
    innName: 'ketoconazole',
    displayName: 'Ketoconazole 200mg',
    category: 'Antifungal',
    commonDosage: '5–10 mg/kg once daily for 30 days',
    contraindicatedSpecies: [
      { species: 'Cat', reason: 'Hepatotoxic in cats — prefer itraconazole' },
    ],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 10, unit: 'mg', frequency: 'SID', maxDurationDays: 30 },
    ],
    interactionSeverity: 'Moderate',
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000009',
    innName: 'atenolol',
    displayName: 'Atenolol 25mg',
    category: 'Cardiac',
    commonDosage: '0.2–1 mg/kg once daily — titrate to effect',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 0.5, unit: 'mg', frequency: 'SID', maxDurationDays: null },
      { species: 'Cat', dosePerKg: 1, unit: 'mg', frequency: 'SID', maxDurationDays: null },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000010',
    innName: 'dexamethasone',
    displayName: 'Dexamethasone 2mg/ml injection',
    category: 'AntiInflammatory',
    commonDosage: '0.1–0.2 mg/kg IV/IM for acute inflammation',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 0.15, unit: 'mg', frequency: 'once', maxDurationDays: 1 },
      { species: 'Cat', dosePerKg: 0.1, unit: 'mg', frequency: 'once', maxDurationDays: 1 },
      { species: 'Camel', dosePerKg: 0.05, unit: 'mg', frequency: 'once', maxDurationDays: 1 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000011',
    innName: 'furosemide',
    displayName: 'Furosemide 40mg',
    category: 'Cardiac',
    commonDosage: '1–2 mg/kg once or twice daily',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 1, unit: 'mg', frequency: 'SID-BID', maxDurationDays: null },
      { species: 'Cat', dosePerKg: 1, unit: 'mg', frequency: 'SID', maxDurationDays: null },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000012',
    innName: 'tramadol',
    displayName: 'Tramadol 50mg',
    category: 'Analgesic',
    commonDosage: '1–5 mg/kg every 8–12 hours',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 2, unit: 'mg', frequency: 'TID', maxDurationDays: 7 },
      { species: 'Cat', dosePerKg: 1, unit: 'mg', frequency: 'BID', maxDurationDays: 5 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000013',
    innName: 'cephalexin',
    displayName: 'Cephalexin 500mg',
    category: 'Antibiotic',
    commonDosage: '10–30 mg/kg twice daily for 7 days',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 20, unit: 'mg', frequency: 'BID', maxDurationDays: 7 },
      { species: 'Cat', dosePerKg: 15, unit: 'mg', frequency: 'BID', maxDurationDays: 7 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
  {
    id: 'drug-0000-0000-0000-000000000014',
    innName: 'chlorhexidine',
    displayName: 'Chlorhexidine 2% solution',
    category: 'Dermatological',
    commonDosage: 'Apply to affected area twice daily for 14 days',
    contraindicatedSpecies: [],
    dosageGuidelines: [],
    interactionSeverity: null,
    requiresPrescription: false,
  },
  {
    id: 'drug-0000-0000-0000-000000000015',
    innName: 'maropitant',
    displayName: 'Maropitant 16mg',
    category: 'Other',
    commonDosage: '1 mg/kg once daily for up to 5 days',
    contraindicatedSpecies: [],
    dosageGuidelines: [
      { species: 'Dog', dosePerKg: 1, unit: 'mg', frequency: 'SID', maxDurationDays: 5 },
      { species: 'Cat', dosePerKg: 1, unit: 'mg', frequency: 'SID', maxDurationDays: 5 },
    ],
    interactionSeverity: null,
    requiresPrescription: true,
  },
]

// Mutable list so POST can add entries at runtime
const drugStore: DrugCatalogEntryDto[] = [...MOCK_DRUGS]

export const drugHandlers = [
  // GET /api/medical-records/drugs/catalog — full list for catalog page
  http.get('/api/medical-records/drugs/catalog', async ({ request }) => {
    await new Promise(resolve => setTimeout(resolve, 100))

    const url = new URL(request.url)
    const search = url.searchParams.get('search')?.toLowerCase().trim()
    const category = url.searchParams.get('category')
    const species = url.searchParams.get('species')?.toLowerCase().trim()

    let results = [...drugStore]

    if (search) {
      results = results.filter(
        d =>
          d.innName.toLowerCase().includes(search) ||
          d.displayName.toLowerCase().includes(search)
      )
    }

    if (category && category !== 'all') {
      results = results.filter(d => d.category === category)
    }

    if (species && species !== 'all') {
      results = results.filter(d =>
        d.dosageGuidelines.some(g => g.species.toLowerCase() === species)
      )
    }

    return HttpResponse.json<DrugCatalogEntryDto[]>(results)
  }),

  // POST /api/medical-records/drugs/catalog — add a new drug
  http.post('/api/medical-records/drugs/catalog', async ({ request }) => {
    await new Promise(resolve => setTimeout(resolve, 150))
    const body = await request.json() as Omit<DrugCatalogEntryDto, 'id'>
    const newDrug: DrugCatalogEntryDto = {
      ...body,
      id: `drug-0000-0000-0000-${String(drugStore.length + 1).padStart(12, '0')}`,
    }
    drugStore.push(newDrug)
    return HttpResponse.json<DrugCatalogEntryDto>(newDrug, { status: 201 })
  }),

  // GET /api/medical-records/drugs?search={term} — search (used by prescription)
  http.get('/api/medical-records/drugs', async ({ request }) => {
    await new Promise(resolve => setTimeout(resolve, 100))

    const url = new URL(request.url)
    const search = url.searchParams.get('search')?.toLowerCase().trim()

    if (!search) {
      return HttpResponse.json<DrugCatalogEntryDto[]>([])
    }

    const results = drugStore.filter(
      d =>
        d.innName.toLowerCase().includes(search) ||
        d.displayName.toLowerCase().includes(search)
    ).slice(0, 10)

    return HttpResponse.json<DrugCatalogEntryDto[]>(results)
  }),

  // GET /api/medical-records/drugs/:id
  http.get('/api/medical-records/drugs/:id', async ({ params }) => {
    await new Promise(resolve => setTimeout(resolve, 100))

    const drug = MOCK_DRUGS.find(d => d.id === params.id)
    if (!drug) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json<DrugCatalogEntryDto>(drug)
  }),

  // POST /api/medical-records/prescriptions/preflight
  // Simulates interaction checking based on drug + patient context
  http.post('/api/medical-records/prescriptions/preflight', async ({ request }) => {
    await new Promise(resolve => setTimeout(resolve, 150))

    const body = await request.json() as PreflightRequest
    const { drugCatalogEntryId, patientSpecies, dosageAmount } = body

    // Drug IDs used in mock scenarios:
    // drug-...001 = Amoxicillin
    // drug-...003 = Metronidazole
    // drug-...008 = Ketoconazole (cat contraindication)

    const AMOXICILLIN_ID = 'drug-0000-0000-0000-000000000001'
    const MELOXICAM_ID = 'drug-0000-0000-0000-000000000002'
    const METRONIDAZOLE_ID = 'drug-0000-0000-0000-000000000003'
    const IVERMECTIN_ID = 'drug-0000-0000-0000-000000000004'
    const IBUPROFEN_ID = 'drug-ibuprofen-0000-0000-000000000099' // fictional for demo

    // ── Stock scenarios ────────────────────────────────────────────────────────
    // Amoxicillin: normal stock (100 tablets)
    const STOCK_IN: StockAvailabilityResult = {
      available: true,
      quantity: 100,
      unit: 'tablets',
      isLowStock: false,
      isExpiringSoon: false,
      alternatives: [],
    }
    // Meloxicam: low stock (5 tablets, threshold 20)
    const STOCK_LOW: StockAvailabilityResult = {
      available: true,
      quantity: 5,
      unit: 'tablets',
      isLowStock: true,
      isExpiringSoon: false,
      alternatives: [],
    }
    // Ivermectin: out of stock with alternatives
    const STOCK_OUT: StockAvailabilityResult = {
      available: false,
      quantity: 0,
      unit: 'vials',
      isLowStock: false,
      isExpiringSoon: false,
      alternatives: [
        {
          stockItemId: 'stock-alt-0000-0001',
          name: 'Selamectin 6% spot-on',
          drugCatalogEntryId: null,
          quantity: 12,
          unit: 'pipettes',
        },
        {
          stockItemId: 'stock-alt-0000-0002',
          name: 'Doramectin 1% injection',
          drugCatalogEntryId: null,
          quantity: 4,
          unit: 'vials',
        },
      ],
    }
    // Ketoconazole: expiring soon
    const STOCK_EXPIRING: StockAvailabilityResult = {
      available: true,
      quantity: 30,
      unit: 'tablets',
      isLowStock: false,
      isExpiringSoon: true,
      alternatives: [],
    }

    function stockFor(id: string): StockAvailabilityResult {
      if (id === AMOXICILLIN_ID) return STOCK_IN
      if (id === MELOXICAM_ID) return STOCK_LOW
      if (id === IVERMECTIN_ID) return STOCK_OUT
      if (id === 'drug-0000-0000-0000-000000000008') return STOCK_EXPIRING // ketoconazole
      return STOCK_IN
    }

    // Scenario 1: Ibuprofen + Cat -> Critical (species contraindication)
    if (drugCatalogEntryId === IBUPROFEN_ID && patientSpecies === 'Cat') {
      return HttpResponse.json<PrescriptionPreflightResult>({
        interactionAlerts: [
          {
            severity: 'Critical',
            type: 'SpeciesContraindication',
            message: 'Ibuprofen is contraindicated in cats — can cause acute renal failure and GI ulceration.',
            alternativeDrugIds: [AMOXICILLIN_ID],
          },
        ],
        stockAvailability: STOCK_IN,
        safeAlternatives: [
          {
            id: AMOXICILLIN_ID,
            displayName: 'Meloxicam 1.5mg/ml',
            innName: 'meloxicam',
            commonDosage: '0.05 mg/kg once daily for cats',
          },
        ],
        dosageRange: null,
      })
    }

    // Scenario 2: Metronidazole -> Moderate interaction (concurrent Amoxicillin)
    if (drugCatalogEntryId === METRONIDAZOLE_ID) {
      return HttpResponse.json<PrescriptionPreflightResult>({
        interactionAlerts: [
          {
            severity: 'Moderate',
            type: 'DrugInteraction',
            message: 'Concurrent use of Metronidazole with Amoxicillin may enhance antibacterial effect but increases risk of GI adverse effects. Monitor closely.',
            alternativeDrugIds: [],
          },
        ],
        stockAvailability: stockFor(METRONIDAZOLE_ID),
        safeAlternatives: [],
        dosageRange: {
          minDose: 10,
          maxDose: 25,
          unit: 'mg/kg',
          dosePerKg: 15,
          recommendedDose: body.patientWeightKg ? body.patientWeightKg * 15 : undefined,
        },
      })
    }

    // Scenario 3: Amoxicillin + dosage out of range -> Info
    if (drugCatalogEntryId === AMOXICILLIN_ID && dosageAmount !== undefined && dosageAmount > 25) {
      return HttpResponse.json<PrescriptionPreflightResult>({
        interactionAlerts: [
          {
            severity: 'Info',
            type: 'DosageOutOfRange',
            message: `Dosage ${dosageAmount} mg/kg exceeds recommended range for Amoxicillin (10–20 mg/kg). Verify weight and recalculate.`,
            alternativeDrugIds: [],
          },
        ],
        stockAvailability: STOCK_IN,
        safeAlternatives: [],
        dosageRange: {
          minDose: 10,
          maxDose: 20,
          unit: 'mg/kg',
          dosePerKg: 15,
          recommendedDose: body.patientWeightKg ? body.patientWeightKg * 15 : undefined,
        },
      })
    }

    // Scenario 4: Amoxicillin with weight -> show dosage range (no alert)
    if (drugCatalogEntryId === AMOXICILLIN_ID) {
      return HttpResponse.json<PrescriptionPreflightResult>({
        interactionAlerts: [],
        stockAvailability: STOCK_IN,
        safeAlternatives: [],
        dosageRange: body.patientWeightKg ? {
          minDose: 10,
          maxDose: 20,
          unit: 'mg/kg',
          dosePerKg: 15,
          recommendedDose: body.patientWeightKg * 15,
        } : null,
      })
    }

    // Default: use stock based on drug id, no alerts
    return HttpResponse.json<PrescriptionPreflightResult>({
      interactionAlerts: [],
      stockAvailability: stockFor(drugCatalogEntryId),
      safeAlternatives: [],
      dosageRange: null,
    })
  }),
]

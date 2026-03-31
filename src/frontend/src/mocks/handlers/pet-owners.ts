import { http, HttpResponse, delay } from 'msw'

export interface ClinicSearchResult {
  id: string
  name: string
  address: string
  emirate: string
  phone: string
  rating: number
  reviewCount: number
}

export interface AskVetRequest {
  vetEmail: string
  ownerName: string
  petName: string
  message?: string
}

const MOCK_CLINICS: ClinicSearchResult[] = [
  {
    id: 'clinic-001',
    name: 'Desert Paws Veterinary Clinic',
    address: 'Al Wasl Road, Jumeirah, Dubai',
    emirate: 'Dubai',
    phone: '+971 4 123 4567',
    rating: 4.8,
    reviewCount: 127,
  },
  {
    id: 'clinic-002',
    name: 'Al Barsha Pet Hospital',
    address: 'Al Barsha 1, Near Mall of the Emirates, Dubai',
    emirate: 'Dubai',
    phone: '+971 4 234 5678',
    rating: 4.6,
    reviewCount: 89,
  },
  {
    id: 'clinic-003',
    name: 'Abu Dhabi Falcon & Exotic Animal Hospital',
    address: 'Al Maqtaa, Abu Dhabi',
    emirate: 'Abu Dhabi',
    phone: '+971 2 345 6789',
    rating: 4.9,
    reviewCount: 203,
  },
  {
    id: 'clinic-004',
    name: 'Sharjah Veterinary Center',
    address: 'King Faisal Street, Sharjah',
    emirate: 'Sharjah',
    phone: '+971 6 456 7890',
    rating: 4.5,
    reviewCount: 64,
  },
  {
    id: 'clinic-005',
    name: 'RAK Animal Care Clinic',
    address: 'Al Nakheel, Ras Al Khaimah',
    emirate: 'Ras Al Khaimah',
    phone: '+971 7 567 8901',
    rating: 4.7,
    reviewCount: 42,
  },
]

const BASE = '/api/v1/pet-owners'

export const petOwnerHandlers = [
  // GET /api/v1/pet-owners/clinics?q=search_term
  http.get(`${BASE}/clinics`, async ({ request }) => {
    await delay(300)
    const url = new URL(request.url)
    const query = (url.searchParams.get('q') ?? '').toLowerCase().trim()

    if (!query) {
      return HttpResponse.json(MOCK_CLINICS)
    }

    const filtered = MOCK_CLINICS.filter(
      (c) =>
        c.name.toLowerCase().includes(query) ||
        c.address.toLowerCase().includes(query) ||
        c.emirate.toLowerCase().includes(query)
    )

    return HttpResponse.json(filtered)
  }),

  // POST /api/v1/pet-owners/ask-vet
  http.post(`${BASE}/ask-vet`, async ({ request }) => {
    await delay(500)
    const body = (await request.json()) as AskVetRequest

    if (!body.vetEmail || !body.ownerName || !body.petName) {
      return HttpResponse.json(
        { title: 'vetEmail, ownerName, and petName are required.' },
        { status: 422 }
      )
    }

    return HttpResponse.json({ success: true }, { status: 200 })
  }),
]

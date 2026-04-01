import { http, HttpResponse } from 'msw'
import type { ReportingPeriodDto } from '@/lib/api/e-reporting'

const MOCK_REPORTS: ReportingPeriodDto[] = [
  {
    id: 'rpt-0000-0000-0000-000000000001',
    month: 1,
    year: 2026,
    status: 'ACCEPTED',
    totalRevenue: 45200,
    totalVat: 2260,
    invoiceCount: 38,
    submittedAt: '2026-02-05T10:00:00Z',
    acceptedAt: '2026-02-10T14:30:00Z',
    rejectedAt: null,
    rejectionReason: null,
    lineItems: [
      { id: 'li-001', description: 'Consultations', category: 'Services', quantity: 25, unitPrice: 200, vatAmount: 250, total: 5250 },
      { id: 'li-002', description: 'Vaccinations', category: 'Services', quantity: 18, unitPrice: 150, vatAmount: 135, total: 2985 },
      { id: 'li-003', description: 'Surgeries', category: 'Procedures', quantity: 5, unitPrice: 3500, vatAmount: 875, total: 18375 },
      { id: 'li-004', description: 'Medications', category: 'Products', quantity: 42, unitPrice: 80, vatAmount: 168, total: 3528 },
      { id: 'li-005', description: 'Diagnostic imaging', category: 'Services', quantity: 12, unitPrice: 350, vatAmount: 210, total: 4410 },
      { id: 'li-006', description: 'Lab tests', category: 'Services', quantity: 30, unitPrice: 120, vatAmount: 180, total: 3780 },
      { id: 'li-007', description: 'Dental cleaning', category: 'Procedures', quantity: 8, unitPrice: 500, vatAmount: 200, total: 4200 },
      { id: 'li-008', description: 'Boarding', category: 'Services', quantity: 4, unitPrice: 300, vatAmount: 60, total: 1260 },
    ],
    clinicId: 'clinic-001',
    createdAt: '2026-02-01T00:00:00Z',
  },
  {
    id: 'rpt-0000-0000-0000-000000000002',
    month: 2,
    year: 2026,
    status: 'SUBMITTED',
    totalRevenue: 52800,
    totalVat: 2640,
    invoiceCount: 45,
    submittedAt: '2026-03-04T09:15:00Z',
    acceptedAt: null,
    rejectedAt: null,
    rejectionReason: null,
    lineItems: [
      { id: 'li-010', description: 'Consultations', category: 'Services', quantity: 30, unitPrice: 200, vatAmount: 300, total: 6300 },
      { id: 'li-011', description: 'Vaccinations', category: 'Services', quantity: 22, unitPrice: 150, vatAmount: 165, total: 3465 },
      { id: 'li-012', description: 'Surgeries', category: 'Procedures', quantity: 7, unitPrice: 3500, vatAmount: 1225, total: 25725 },
      { id: 'li-013', description: 'Medications', category: 'Products', quantity: 55, unitPrice: 80, vatAmount: 220, total: 4620 },
      { id: 'li-014', description: 'Diagnostic imaging', category: 'Services', quantity: 15, unitPrice: 350, vatAmount: 262.5, total: 5512.5 },
      { id: 'li-015', description: 'Lab tests', category: 'Services', quantity: 28, unitPrice: 120, vatAmount: 168, total: 3528 },
    ],
    clinicId: 'clinic-001',
    createdAt: '2026-03-01T00:00:00Z',
  },
  {
    id: 'rpt-0000-0000-0000-000000000003',
    month: 3,
    year: 2026,
    status: 'DRAFT',
    totalRevenue: 38500,
    totalVat: 1925,
    invoiceCount: 31,
    submittedAt: null,
    acceptedAt: null,
    rejectedAt: null,
    rejectionReason: null,
    lineItems: [
      { id: 'li-020', description: 'Consultations', category: 'Services', quantity: 20, unitPrice: 200, vatAmount: 200, total: 4200 },
      { id: 'li-021', description: 'Vaccinations', category: 'Services', quantity: 15, unitPrice: 150, vatAmount: 112.5, total: 2362.5 },
      { id: 'li-022', description: 'Surgeries', category: 'Procedures', quantity: 4, unitPrice: 3500, vatAmount: 700, total: 14700 },
      { id: 'li-023', description: 'Medications', category: 'Products', quantity: 35, unitPrice: 80, vatAmount: 140, total: 2940 },
    ],
    clinicId: 'clinic-001',
    createdAt: '2026-03-15T00:00:00Z',
  },
  {
    id: 'rpt-0000-0000-0000-000000000004',
    month: 12,
    year: 2025,
    status: 'REJECTED',
    totalRevenue: 41000,
    totalVat: 2050,
    invoiceCount: 34,
    submittedAt: '2026-01-06T08:00:00Z',
    acceptedAt: null,
    rejectedAt: '2026-01-12T16:45:00Z',
    rejectionReason: 'Missing VAT registration reference on 3 invoices',
    lineItems: [
      { id: 'li-030', description: 'Consultations', category: 'Services', quantity: 22, unitPrice: 200, vatAmount: 220, total: 4620 },
      { id: 'li-031', description: 'Surgeries', category: 'Procedures', quantity: 6, unitPrice: 3500, vatAmount: 1050, total: 22050 },
      { id: 'li-032', description: 'Medications', category: 'Products', quantity: 40, unitPrice: 80, vatAmount: 160, total: 3360 },
    ],
    clinicId: 'clinic-001',
    createdAt: '2026-01-01T00:00:00Z',
  },
]

export const eReportingHandlers = [
  // GET /api/e-reporting
  http.get('/api/e-reporting', ({ request }) => {
    const url = new URL(request.url)
    const year = url.searchParams.get('year')
    const status = url.searchParams.get('status')

    let items = [...MOCK_REPORTS]
    if (year) {
      items = items.filter((r) => r.year === parseInt(year))
    }
    if (status) {
      items = items.filter((r) => r.status === status)
    }

    // Sort by year desc, month desc
    items.sort((a, b) => b.year - a.year || b.month - a.month)

    return HttpResponse.json(items)
  }),

  // GET /api/e-reporting/:id
  http.get('/api/e-reporting/:id', ({ params }) => {
    const report = MOCK_REPORTS.find((r) => r.id === params.id)
    if (!report) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(report)
  }),

  // POST /api/e-reporting/:id/submit
  http.post('/api/e-reporting/:id/submit', ({ params }) => {
    const report = MOCK_REPORTS.find((r) => r.id === params.id)
    if (!report) return new HttpResponse(null, { status: 404 })
    if (report.status !== 'DRAFT' && report.status !== 'REJECTED') {
      return HttpResponse.json(
        { title: 'Report can only be submitted when in DRAFT or REJECTED status' },
        { status: 422 }
      )
    }
    report.status = 'SUBMITTED'
    report.submittedAt = new Date().toISOString()
    report.rejectedAt = null
    report.rejectionReason = null
    return HttpResponse.json(report)
  }),
]

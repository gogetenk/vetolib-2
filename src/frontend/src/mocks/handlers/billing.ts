import { http, HttpResponse } from 'msw'
import type { InvoiceDto, PagedResult } from '@/lib/api/billing'

const MOCK_INVOICES: InvoiceDto[] = [
  {
    id: 'inv-0000-0000-0000-000000000001',
    invoiceNumber: 'INV-2026-001',
    patientId: 'pat-0000-0000-0000-000000000001',
    patientName: 'Max',
    ownerName: 'Ahmed Al-Rashid',
    ownerPhone: '+971 50 123 4567',
    appointmentId: null,
    status: 'DRAFT',
    items: [
      { id: 'item-001', description: 'Consultation', quantity: 1, unitPrice: 200, subtotal: 200 },
    ],
    subtotal: 200,
    vatRate: 5,
    vatAmount: 10,
    total: 210,
    notes: null,
    createdAt: new Date('2026-03-01T09:00:00Z').toISOString(),
    paidAt: null,
    dueDate: null,
    clinicId: 'clinic-001',
  },
  {
    id: 'inv-0000-0000-0000-000000000002',
    invoiceNumber: 'INV-2026-002',
    patientId: 'pat-0000-0000-0000-000000000002',
    patientName: 'Luna',
    ownerName: 'Fatima Hassan',
    ownerPhone: '+971 55 987 6543',
    appointmentId: null,
    status: 'SENT',
    items: [
      { id: 'item-002', description: 'Vaccination', quantity: 1, unitPrice: 150, subtotal: 150 },
      { id: 'item-003', description: 'Medicines', quantity: 1, unitPrice: 80, subtotal: 80 },
    ],
    subtotal: 230,
    vatRate: 5,
    vatAmount: 11.5,
    total: 241.5,
    notes: 'Follow up in 2 weeks',
    createdAt: new Date('2026-03-03T10:30:00Z').toISOString(),
    paidAt: null,
    dueDate: new Date('2026-04-02T10:30:00Z').toISOString(),
    clinicId: 'clinic-001',
  },
  {
    id: 'inv-0000-0000-0000-000000000003',
    invoiceNumber: 'INV-2026-003',
    patientId: 'pat-0000-0000-0000-000000000003',
    patientName: 'Rocky',
    ownerName: 'Mohammed Al-Zaabi',
    ownerPhone: '+971 54 321 0987',
    appointmentId: null,
    status: 'PAID',
    items: [
      { id: 'item-004', description: 'Surgery', quantity: 1, unitPrice: 1800, subtotal: 1800 },
      { id: 'item-005', description: 'Anaesthesia', quantity: 1, unitPrice: 400, subtotal: 400 },
    ],
    subtotal: 2200,
    vatRate: 5,
    vatAmount: 110,
    total: 2310,
    notes: null,
    createdAt: new Date('2026-03-05T08:00:00Z').toISOString(),
    paidAt: new Date('2026-03-07T14:00:00Z').toISOString(),
    dueDate: new Date('2026-04-04T08:00:00Z').toISOString(),
    clinicId: 'clinic-001',
  },
]

let invoiceCounter = MOCK_INVOICES.length

export const billingHandlers = [
  // GET /api/invoices
  http.get('/api/invoices', ({ request }) => {
    const url = new URL(request.url)
    const status = url.searchParams.get('status')
    const page = parseInt(url.searchParams.get('page') ?? '1')
    const pageSize = parseInt(url.searchParams.get('pageSize') ?? '10')

    let items = [...MOCK_INVOICES]
    if (status) {
      items = items.filter((inv) => inv.status === status)
    }

    const start = (page - 1) * pageSize
    const paged = items.slice(start, start + pageSize)

    return HttpResponse.json<PagedResult<InvoiceDto>>({
      items: paged,
      totalCount: items.length,
      page,
      pageSize,
    })
  }),

  // GET /api/invoices/:id
  http.get('/api/invoices/:id', ({ params }) => {
    const invoice = MOCK_INVOICES.find((inv) => inv.id === params.id)
    if (!invoice) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(invoice)
  }),

  // POST /api/invoices
  http.post('/api/invoices', async ({ request }) => {
    const body = await request.json() as {
      patientId: string
      patientName: string
      ownerName: string
      ownerPhone: string
      appointmentId?: string | null
      items: Array<{ description: string; quantity: number; unitPrice: number }>
      notes?: string | null
    }

    invoiceCounter++
    const year = new Date().getFullYear()
    const subtotal = body.items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0)
    const vatAmount = Math.round(subtotal * 0.05 * 100) / 100
    const total = Math.round((subtotal + vatAmount) * 100) / 100

    const newInvoice: InvoiceDto = {
      id: crypto.randomUUID(),
      invoiceNumber: `INV-${year}-${String(invoiceCounter).padStart(3, '0')}`,
      patientId: body.patientId,
      patientName: body.patientName,
      ownerName: body.ownerName,
      ownerPhone: body.ownerPhone,
      appointmentId: body.appointmentId ?? null,
      status: 'DRAFT',
      items: body.items.map((item) => ({
        id: crypto.randomUUID(),
        description: item.description,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        subtotal: item.quantity * item.unitPrice,
      })),
      subtotal,
      vatRate: 5,
      vatAmount,
      total,
      notes: body.notes ?? null,
      createdAt: new Date().toISOString(),
      paidAt: null,
      dueDate: null,
      clinicId: 'clinic-001',
    }

    MOCK_INVOICES.push(newInvoice)
    return HttpResponse.json(newInvoice, { status: 201 })
  }),

  // PATCH /api/invoices/:id/send
  http.patch('/api/invoices/:id/send', ({ params }) => {
    const invoice = MOCK_INVOICES.find((inv) => inv.id === params.id)
    if (!invoice) return new HttpResponse(null, { status: 404 })
    if (invoice.status !== 'DRAFT') {
      return HttpResponse.json({ title: 'Invoice is not in DRAFT status' }, { status: 422 })
    }
    invoice.status = 'SENT'
    const dueDate = new Date()
    dueDate.setDate(dueDate.getDate() + 30)
    invoice.dueDate = dueDate.toISOString()
    return HttpResponse.json(invoice)
  }),

  // PATCH /api/invoices/:id/pay
  http.patch('/api/invoices/:id/pay', async ({ params, request }) => {
    const invoice = MOCK_INVOICES.find((inv) => inv.id === params.id)
    if (!invoice) return new HttpResponse(null, { status: 404 })
    if (invoice.status !== 'SENT') {
      return HttpResponse.json({ title: 'Invoice is not in SENT status' }, { status: 422 })
    }
    const body = await request.json().catch(() => ({})) as { paidAt?: string }
    invoice.status = 'PAID'
    invoice.paidAt = body.paidAt ?? new Date().toISOString()
    return HttpResponse.json(invoice)
  }),

  // PATCH /api/invoices/:id/cancel
  http.patch('/api/invoices/:id/cancel', ({ params }) => {
    const invoice = MOCK_INVOICES.find((inv) => inv.id === params.id)
    if (!invoice) return new HttpResponse(null, { status: 404 })
    if (!['DRAFT', 'SENT'].includes(invoice.status)) {
      return HttpResponse.json({ title: 'Invoice cannot be cancelled' }, { status: 422 })
    }
    invoice.status = 'CANCELLED'
    return HttpResponse.json(invoice)
  }),

  // DELETE /api/invoices/:id
  http.delete('/api/invoices/:id', ({ params }) => {
    const idx = MOCK_INVOICES.findIndex((inv) => inv.id === params.id)
    if (idx === -1) return new HttpResponse(null, { status: 404 })
    if (MOCK_INVOICES[idx].status !== 'DRAFT') {
      return HttpResponse.json({ title: 'Only DRAFT invoices can be deleted' }, { status: 422 })
    }
    MOCK_INVOICES.splice(idx, 1)
    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/invoices/:id/pdf
  http.get('/api/invoices/:id/pdf', ({ params }) => {
    const invoice = MOCK_INVOICES.find((inv) => inv.id === params.id)
    if (!invoice) return new HttpResponse(null, { status: 404 })
    // Return a tiny placeholder PDF blob
    const pdfContent = `%PDF-1.4 mock invoice ${invoice.invoiceNumber}`
    return new HttpResponse(pdfContent, {
      status: 200,
      headers: {
        'Content-Type': 'application/pdf',
        'Content-Disposition': `attachment; filename="${invoice.invoiceNumber}.pdf"`,
      },
    })
  }),
]

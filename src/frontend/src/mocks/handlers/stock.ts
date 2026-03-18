import { http, HttpResponse } from 'msw'
import type {
  StockItemDto,
  StockAlertDto,
  StockMovementDto,
  StockMovementHistoryDto,
  FullMovementType,
  CreateStockItemRequest,
  UpdateStockItemRequest,
  CreateStockMovementRequest,
} from '@/lib/api/stock'

const CLINIC_ID = 'clinic-001'

const MOCK_STOCK_ITEMS: StockItemDto[] = [
  {
    id: 'stock-0000-0000-0000-000000000001',
    name: 'Meloxicam 1.5mg/ml',
    category: 'Medication',
    quantity: 5,
    unit: 'bottles',
    threshold: 20,
    expiryDate: '2026-08-15',
    clinicId: CLINIC_ID,
    isLowStock: true,
    isExpiringSoon: false,
  },
  {
    id: 'stock-0000-0000-0000-000000000002',
    name: 'Amoxicillin 250mg',
    category: 'Medication',
    quantity: 120,
    unit: 'tablets',
    threshold: 30,
    expiryDate: '2027-03-01',
    clinicId: CLINIC_ID,
    isLowStock: false,
    isExpiringSoon: false,
  },
  {
    id: 'stock-0000-0000-0000-000000000003',
    name: 'Ketamine 100mg/ml',
    category: 'Medication',
    quantity: 8,
    unit: 'vials',
    threshold: 5,
    expiryDate: '2026-04-30',
    clinicId: CLINIC_ID,
    isLowStock: false,
    isExpiringSoon: true,
  },
  {
    id: 'stock-0000-0000-0000-000000000004',
    name: 'Surgical Gloves (M)',
    category: 'Supply',
    quantity: 12,
    unit: 'boxes',
    threshold: 15,
    expiryDate: null,
    clinicId: CLINIC_ID,
    isLowStock: true,
    isExpiringSoon: false,
  },
  {
    id: 'stock-0000-0000-0000-000000000005',
    name: 'Syringes 5ml',
    category: 'Supply',
    quantity: 200,
    unit: 'units',
    threshold: 50,
    expiryDate: null,
    clinicId: CLINIC_ID,
    isLowStock: false,
    isExpiringSoon: false,
  },
  {
    id: 'stock-0000-0000-0000-000000000006',
    name: 'Rabies Vaccine',
    category: 'Vaccine',
    quantity: 24,
    unit: 'doses',
    threshold: 10,
    expiryDate: '2026-12-31',
    clinicId: CLINIC_ID,
    isLowStock: false,
    isExpiringSoon: false,
  },
]

const MOCK_MOVEMENTS: StockMovementDto[] = []

const MOCK_MOVEMENT_HISTORY: StockMovementHistoryDto[] = [
  {
    id: 'mov-0000-0000-0000-000000000001',
    stockItemId: 'stock-0000-0000-0000-000000000001',
    stockItemName: 'Meloxicam 1.5mg/ml',
    type: 'INCOMING',
    quantity: 50,
    previousQuantity: 5,
    newQuantity: 55,
    reason: 'Monthly restock from Al Ain Pharma',
    performedBy: 'Dr. Fatima Al Maktoum',
    patientName: null,
    createdAt: '2026-03-17T09:30:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000002',
    stockItemId: 'stock-0000-0000-0000-000000000002',
    stockItemName: 'Amoxicillin 250mg',
    type: 'OUTGOING',
    quantity: 10,
    previousQuantity: 130,
    newQuantity: 120,
    reason: 'Prescribed for post-surgery infection prevention',
    performedBy: 'Dr. Ahmed Hassan',
    patientName: 'Buddy (Golden Retriever)',
    createdAt: '2026-03-17T11:15:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000003',
    stockItemId: 'stock-0000-0000-0000-000000000003',
    stockItemName: 'Ketamine 100mg/ml',
    type: 'OUTGOING',
    quantity: 2,
    previousQuantity: 10,
    newQuantity: 8,
    reason: 'Used for anesthesia during dental cleaning',
    performedBy: 'Dr. Fatima Al Maktoum',
    patientName: 'Simba (Maine Coon)',
    createdAt: '2026-03-16T14:00:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000004',
    stockItemId: 'stock-0000-0000-0000-000000000004',
    stockItemName: 'Surgical Gloves (M)',
    type: 'LOSS',
    quantity: 3,
    previousQuantity: 15,
    newQuantity: 12,
    reason: 'Damaged packaging — water leak in storage',
    performedBy: 'Nurse Layla Osman',
    patientName: null,
    createdAt: '2026-03-16T08:45:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000005',
    stockItemId: 'stock-0000-0000-0000-000000000005',
    stockItemName: 'Syringes 5ml',
    type: 'ADJUSTMENT',
    quantity: 200,
    previousQuantity: 180,
    newQuantity: 200,
    reason: 'Inventory recount correction',
    performedBy: 'Dr. Ahmed Hassan',
    patientName: null,
    createdAt: '2026-03-15T16:30:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000006',
    stockItemId: 'stock-0000-0000-0000-000000000006',
    stockItemName: 'Rabies Vaccine',
    type: 'OUTGOING',
    quantity: 1,
    previousQuantity: 25,
    newQuantity: 24,
    reason: 'Annual vaccination',
    performedBy: 'Dr. Fatima Al Maktoum',
    patientName: 'Rex (German Shepherd)',
    createdAt: '2026-03-15T10:00:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000007',
    stockItemId: 'stock-0000-0000-0000-000000000002',
    stockItemName: 'Amoxicillin 250mg',
    type: 'RETURN',
    quantity: 20,
    previousQuantity: 140,
    newQuantity: 120,
    reason: 'Return to supplier — wrong batch number',
    performedBy: 'Nurse Layla Osman',
    patientName: null,
    createdAt: '2026-03-14T13:00:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000008',
    stockItemId: 'stock-0000-0000-0000-000000000001',
    stockItemName: 'Meloxicam 1.5mg/ml',
    type: 'OUTGOING',
    quantity: 3,
    previousQuantity: 8,
    newQuantity: 5,
    reason: 'Pain management for arthritis',
    performedBy: 'Dr. Ahmed Hassan',
    patientName: 'Luna (Persian Cat)',
    createdAt: '2026-03-14T09:20:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000009',
    stockItemId: 'stock-0000-0000-0000-000000000003',
    stockItemName: 'Ketamine 100mg/ml',
    type: 'INCOMING',
    quantity: 10,
    previousQuantity: 0,
    newQuantity: 10,
    reason: 'Emergency restock from Dubai Vet Supplies',
    performedBy: 'Nurse Layla Osman',
    patientName: null,
    createdAt: '2026-03-13T07:30:00Z',
  },
  {
    id: 'mov-0000-0000-0000-000000000010',
    stockItemId: 'stock-0000-0000-0000-000000000004',
    stockItemName: 'Surgical Gloves (M)',
    type: 'INCOMING',
    quantity: 20,
    previousQuantity: 0,
    newQuantity: 20,
    reason: 'Quarterly restock order',
    performedBy: 'Nurse Layla Osman',
    patientName: null,
    createdAt: '2026-03-12T11:00:00Z',
  },
]

export const stockHandlers = [
  // GET /api/v1/stock/movements
  http.get('/api/v1/stock/movements', ({ request }) => {
    const url = new URL(request.url)
    const type = url.searchParams.get('type') as FullMovementType | null
    const stockItemId = url.searchParams.get('stockItemId')
    const dateFrom = url.searchParams.get('dateFrom')
    const dateTo = url.searchParams.get('dateTo')
    const search = url.searchParams.get('search')

    let movements = [...MOCK_MOVEMENT_HISTORY]

    if (type) {
      movements = movements.filter(m => m.type === type)
    }
    if (stockItemId) {
      movements = movements.filter(m => m.stockItemId === stockItemId)
    }
    if (dateFrom) {
      movements = movements.filter(m => m.createdAt >= dateFrom)
    }
    if (dateTo) {
      const endDate = new Date(dateTo)
      endDate.setDate(endDate.getDate() + 1)
      movements = movements.filter(m => m.createdAt < endDate.toISOString())
    }
    if (search) {
      const q = search.toLowerCase()
      movements = movements.filter(m =>
        m.stockItemName.toLowerCase().includes(q) ||
        (m.reason?.toLowerCase().includes(q) ?? false) ||
        m.performedBy.toLowerCase().includes(q) ||
        (m.patientName?.toLowerCase().includes(q) ?? false)
      )
    }

    // Sort by createdAt descending
    movements.sort((a, b) => b.createdAt.localeCompare(a.createdAt))

    return HttpResponse.json<StockMovementHistoryDto[]>(movements)
  }),

  // GET /api/v1/stock
  http.get('/api/v1/stock', ({ request }) => {
    const url = new URL(request.url)
    const category = url.searchParams.get('category')
    const status = url.searchParams.get('status')

    let items = [...MOCK_STOCK_ITEMS]

    if (category) {
      items = items.filter(item => item.category === category)
    }

    if (status === 'low-stock') {
      items = items.filter(item => item.isLowStock)
    } else if (status === 'expiring-soon') {
      items = items.filter(item => item.isExpiringSoon)
    }

    return HttpResponse.json<StockItemDto[]>(items)
  }),

  // GET /api/v1/stock/alerts
  http.get('/api/v1/stock/alerts', () => {
    const alerts: StockAlertDto[] = MOCK_STOCK_ITEMS
      .filter(item => item.isLowStock || item.isExpiringSoon)
      .map(item => ({
        id: item.id,
        name: item.name,
        category: item.category,
        quantity: item.quantity,
        threshold: item.threshold,
        expiryDate: item.expiryDate,
        alertType: item.isLowStock ? 'low-stock' : 'expiring-soon',
      }))

    return HttpResponse.json<StockAlertDto[]>(alerts)
  }),

  // POST /api/v1/stock
  http.post('/api/v1/stock', async ({ request }) => {
    const body = await request.json() as CreateStockItemRequest

    const newItem: StockItemDto = {
      id: crypto.randomUUID(),
      name: body.name,
      category: body.category,
      quantity: body.quantity,
      unit: body.unit,
      threshold: body.threshold,
      expiryDate: body.expiryDate ?? null,
      clinicId: CLINIC_ID,
      isLowStock: body.quantity <= body.threshold,
      isExpiringSoon: false,
    }

    MOCK_STOCK_ITEMS.push(newItem)
    return HttpResponse.json<StockItemDto>(newItem, { status: 201 })
  }),

  // PATCH /api/v1/stock/:id
  http.patch('/api/v1/stock/:id', async ({ params, request }) => {
    const item = MOCK_STOCK_ITEMS.find(s => s.id === params.id)
    if (!item) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as UpdateStockItemRequest

    if (body.name !== undefined) item.name = body.name
    if (body.category !== undefined) item.category = body.category
    if (body.quantity !== undefined) item.quantity = body.quantity
    if (body.unit !== undefined) item.unit = body.unit
    if (body.threshold !== undefined) item.threshold = body.threshold
    if (Object.prototype.hasOwnProperty.call(body, 'expiryDate')) {
      item.expiryDate = body.expiryDate ?? null
    }

    item.isLowStock = item.quantity <= item.threshold

    return HttpResponse.json<StockItemDto>(item)
  }),

  // POST /api/v1/stock/:stockItemId/movements
  http.post('/api/v1/stock/:stockItemId/movements', async ({ params, request }) => {
    const stockItemId = params.stockItemId as string
    const item = MOCK_STOCK_ITEMS.find(s => s.id === stockItemId)
    if (!item) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as CreateStockMovementRequest

    const delta =
      body.type === 'IN' ? body.quantity :
      body.type === 'OUT' ? -body.quantity :
      body.quantity // ADJUSTMENT: set directly (treat as delta)

    const newQuantity = Math.max(0, item.quantity + delta)
    item.quantity = newQuantity
    item.isLowStock = newQuantity <= item.threshold

    const movement: StockMovementDto = {
      id: crypto.randomUUID(),
      stockItemId,
      type: body.type,
      quantity: body.quantity,
      reason: body.reason ?? null,
      newQuantity,
      createdAt: new Date().toISOString(),
    }

    MOCK_MOVEMENTS.push(movement)
    return HttpResponse.json<StockMovementDto>(movement, { status: 201 })
  }),
]

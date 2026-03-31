import { http, HttpResponse } from 'msw'
import type { AuditEntryDto, AuditPagedResult } from '@/lib/api/audit'

function daysAgo(days: number, hour: number = 10): string {
  const d = new Date()
  d.setDate(d.getDate() - days)
  d.setHours(hour, Math.floor(Math.random() * 60), 0, 0)
  return d.toISOString()
}

const MOCK_AUDIT_ENTRIES: AuditEntryDto[] = [
  {
    id: 'audit-001',
    timestamp: daysAgo(0, 9),
    action: 'CREATE',
    entityType: 'Appointment',
    entityId: 'apt-0000-0000-0000-000000000001',
    changedBy: 'Dr. Sarah Johnson',
    oldValues: null,
    newValues: { patientName: 'Max', species: 'Dog', reason: 'Annual vaccination', status: 'SCHEDULED' },
  },
  {
    id: 'audit-002',
    timestamp: daysAgo(0, 11),
    action: 'UPDATE',
    entityType: 'Appointment',
    entityId: 'apt-0000-0000-0000-000000000002',
    changedBy: 'Dr. Omar Al-Rashid',
    oldValues: { status: 'SCHEDULED' },
    newValues: { status: 'CHECKED_IN' },
  },
  {
    id: 'audit-003',
    timestamp: daysAgo(1, 8),
    action: 'CREATE',
    entityType: 'Patient',
    entityId: 'pat-0000-0000-0000-000000000010',
    changedBy: 'Fatima Hassan',
    oldValues: null,
    newValues: { name: 'Bella', species: 'Cat', breed: 'Persian', ownerName: 'Ahmed Al-Rashid' },
  },
  {
    id: 'audit-004',
    timestamp: daysAgo(1, 14),
    action: 'UPDATE',
    entityType: 'Invoice',
    entityId: 'inv-0000-0000-0000-000000000005',
    changedBy: 'Dr. Layla Al-Mansoori',
    oldValues: { status: 'DRAFT', totalAmount: 350 },
    newValues: { status: 'SENT', totalAmount: 420 },
  },
  {
    id: 'audit-005',
    timestamp: daysAgo(2, 10),
    action: 'DELETE',
    entityType: 'Appointment',
    entityId: 'apt-0000-0000-0000-000000000005',
    changedBy: 'Dr. Khalid Ibrahim',
    oldValues: { patientName: 'Oreo', status: 'CANCELLED', reason: 'Routine check' },
    newValues: null,
  },
  {
    id: 'audit-006',
    timestamp: daysAgo(2, 16),
    action: 'UPDATE',
    entityType: 'User',
    entityId: 'usr-0000-0000-0000-000000000003',
    changedBy: 'Dr. Sarah Johnson',
    oldValues: { role: 'ASSISTANT' },
    newValues: { role: 'VET' },
  },
  {
    id: 'audit-007',
    timestamp: daysAgo(3, 9),
    action: 'CREATE',
    entityType: 'Prescription',
    entityId: 'presc-0000-0000-0000-000000000001',
    changedBy: 'Dr. Omar Al-Rashid',
    oldValues: null,
    newValues: { drugName: 'Amoxicillin 250mg', dosage: '10mg/kg twice daily', duration: '7 days' },
  },
  {
    id: 'audit-008',
    timestamp: daysAgo(3, 13),
    action: 'UPDATE',
    entityType: 'ClinicPreference',
    entityId: 'pref-consultation-duration',
    changedBy: 'Dr. Layla Al-Mansoori',
    oldValues: { value: '30' },
    newValues: { value: '45' },
  },
  {
    id: 'audit-009',
    timestamp: daysAgo(4, 11),
    action: 'CREATE',
    entityType: 'Invoice',
    entityId: 'inv-0000-0000-0000-000000000006',
    changedBy: 'Fatima Hassan',
    oldValues: null,
    newValues: { patientName: 'Sultan', totalAmount: 1200, currency: 'AED' },
  },
  {
    id: 'audit-010',
    timestamp: daysAgo(4, 15),
    action: 'UPDATE',
    entityType: 'Patient',
    entityId: 'pat-0000-0000-0000-000000000003',
    changedBy: 'Dr. Khalid Ibrahim',
    oldValues: { weight: 12.5 },
    newValues: { weight: 13.2 },
  },
  {
    id: 'audit-011',
    timestamp: daysAgo(5, 8),
    action: 'CREATE',
    entityType: 'Appointment',
    entityId: 'apt-0000-0000-0000-000000000011',
    changedBy: 'Dr. Sarah Johnson',
    oldValues: null,
    newValues: { patientName: 'Simba', species: 'Cat', reason: 'Blood work', status: 'SCHEDULED' },
  },
  {
    id: 'audit-012',
    timestamp: daysAgo(5, 12),
    action: 'DELETE',
    entityType: 'Drug',
    entityId: 'drug-0000-0000-0000-000000000002',
    changedBy: 'Dr. Omar Al-Rashid',
    oldValues: { name: 'Expired Vaccine Batch A', category: 'Vaccine' },
    newValues: null,
  },
  {
    id: 'audit-013',
    timestamp: daysAgo(6, 10),
    action: 'UPDATE',
    entityType: 'WorkingHours',
    entityId: 'wh-sunday',
    changedBy: 'Dr. Layla Al-Mansoori',
    oldValues: { openTime: '08:00', closeTime: '17:00' },
    newValues: { openTime: '09:00', closeTime: '18:00' },
  },
  {
    id: 'audit-014',
    timestamp: daysAgo(6, 14),
    action: 'CREATE',
    entityType: 'User',
    entityId: 'usr-0000-0000-0000-000000000005',
    changedBy: 'Dr. Sarah Johnson',
    oldValues: null,
    newValues: { name: 'Noura Al-Ketbi', role: 'RECEPTIONIST', email: 'noura@clinic.ae' },
  },
  {
    id: 'audit-015',
    timestamp: daysAgo(7, 9),
    action: 'UPDATE',
    entityType: 'Appointment',
    entityId: 'apt-0000-0000-0000-000000000009',
    changedBy: 'Dr. Khalid Ibrahim',
    oldValues: { status: 'IN_PROGRESS' },
    newValues: { status: 'COMPLETED' },
  },
  {
    id: 'audit-016',
    timestamp: daysAgo(8, 11),
    action: 'CREATE',
    entityType: 'Patient',
    entityId: 'pat-0000-0000-0000-000000000012',
    changedBy: 'Fatima Hassan',
    oldValues: null,
    newValues: { name: 'Kira', species: 'Dog', breed: 'German Shepherd' },
  },
  {
    id: 'audit-017',
    timestamp: daysAgo(9, 15),
    action: 'UPDATE',
    entityType: 'Invoice',
    entityId: 'inv-0000-0000-0000-000000000003',
    changedBy: 'Dr. Layla Al-Mansoori',
    oldValues: { status: 'SENT' },
    newValues: { status: 'PAID' },
  },
  {
    id: 'audit-018',
    timestamp: daysAgo(10, 10),
    action: 'CREATE',
    entityType: 'Appointment',
    entityId: 'apt-0000-0000-0000-000000000015',
    changedBy: 'Dr. Omar Al-Rashid',
    oldValues: null,
    newValues: { patientName: 'Rocky', species: 'Dog', reason: 'Hip dysplasia check', status: 'SCHEDULED' },
  },
  {
    id: 'audit-019',
    timestamp: daysAgo(11, 13),
    action: 'DELETE',
    entityType: 'Prescription',
    entityId: 'presc-0000-0000-0000-000000000003',
    changedBy: 'Dr. Sarah Johnson',
    oldValues: { drugName: 'Metacam', dosage: '0.1mg/kg once daily' },
    newValues: null,
  },
  {
    id: 'audit-020',
    timestamp: daysAgo(12, 9),
    action: 'UPDATE',
    entityType: 'ClinicPreference',
    entityId: 'pref-reminder-sms',
    changedBy: 'Dr. Khalid Ibrahim',
    oldValues: { value: 'false' },
    newValues: { value: 'true' },
  },
  {
    id: 'audit-021',
    timestamp: daysAgo(13, 10),
    action: 'CREATE',
    entityType: 'Invoice',
    entityId: 'inv-0000-0000-0000-000000000008',
    changedBy: 'Fatima Hassan',
    oldValues: null,
    newValues: { patientName: 'Luna', totalAmount: 275, currency: 'AED' },
  },
  {
    id: 'audit-022',
    timestamp: daysAgo(14, 14),
    action: 'UPDATE',
    entityType: 'Patient',
    entityId: 'pat-0000-0000-0000-000000000001',
    changedBy: 'Dr. Omar Al-Rashid',
    oldValues: { vaccinationStatus: 'Due' },
    newValues: { vaccinationStatus: 'Up to date' },
  },
  {
    id: 'audit-023',
    timestamp: daysAgo(15, 11),
    action: 'UPDATE',
    entityType: 'User',
    entityId: 'usr-0000-0000-0000-000000000002',
    changedBy: 'Dr. Sarah Johnson',
    oldValues: { active: true },
    newValues: { active: false },
  },
  {
    id: 'audit-024',
    timestamp: daysAgo(16, 8),
    action: 'CREATE',
    entityType: 'Appointment',
    entityId: 'apt-0000-0000-0000-000000000020',
    changedBy: 'Dr. Layla Al-Mansoori',
    oldValues: null,
    newValues: { patientName: 'Cleo', species: 'Cat', reason: 'Dental cleaning', status: 'SCHEDULED' },
  },
  {
    id: 'audit-025',
    timestamp: daysAgo(17, 16),
    action: 'UPDATE',
    entityType: 'Appointment',
    entityId: 'apt-0000-0000-0000-000000000020',
    changedBy: 'Dr. Layla Al-Mansoori',
    oldValues: { status: 'SCHEDULED' },
    newValues: { status: 'COMPLETED' },
  },
]

export const auditHandlers = [
  http.get('/api/v1/audit', ({ request }) => {
    const url = new URL(request.url)
    const page = Number.parseInt(url.searchParams.get('page') ?? '1', 10)
    const pageSize = Number.parseInt(url.searchParams.get('pageSize') ?? '20', 10)
    const startDate = url.searchParams.get('startDate')
    const endDate = url.searchParams.get('endDate')
    const user = url.searchParams.get('user')
    const action = url.searchParams.get('action')

    let items = [...MOCK_AUDIT_ENTRIES]

    if (startDate) {
      const start = new Date(startDate)
      items = items.filter((e) => new Date(e.timestamp) >= start)
    }
    if (endDate) {
      const end = new Date(endDate)
      end.setHours(23, 59, 59, 999)
      items = items.filter((e) => new Date(e.timestamp) <= end)
    }
    if (user) {
      items = items.filter((e) => e.changedBy.toLowerCase().includes(user.toLowerCase()))
    }
    if (action) {
      items = items.filter((e) => e.action === action)
    }

    // Sort by timestamp descending (most recent first)
    items.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime())

    const start_idx = (page - 1) * pageSize
    const paged = items.slice(start_idx, start_idx + pageSize)

    return HttpResponse.json<AuditPagedResult>({
      items: paged,
      totalCount: items.length,
      page,
      pageSize,
    })
  }),
]

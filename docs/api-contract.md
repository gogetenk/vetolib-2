# Vetolib API Contract

**Base URL**: `/api`
**Auth**: Bearer JWT (all endpoints except `/api/auth/login` and `/api/auth/refresh`)
**Serialization**: camelCase JSON
**Convention**: All endpoints under `/api/` (no `/v1/` prefix)

---

## Auth

### POST /api/auth/login
Request: `{ email: string, password: string }`
Response: `AuthTokenDto { accessToken, refreshToken, expiresIn: 900, user: UserDto }`

### POST /api/auth/refresh
Request: `{ refreshToken: string }`
Response: `AuthTokenDto { accessToken, refreshToken, expiresIn: 900, user: UserDto }`

### POST /api/auth/logout
Response: 200 OK

### GET /api/auth/me
Response: `UserDto`

**UserDto**: `{ id: string, email: string, fullName: string, role: string, clinicId: string, clinicName: string, vetLicenseNumber?: string }`

---

## Users

### GET /api/users
Response: `UserListItemDto[] { id, email, fullName, role, isActive }`

### POST /api/users
Invite a new team member.
Request: `{ email: string, fullName: string, role: 'Vet' | 'Receptionist' | 'Assistant' }`
Response: `InviteUserResponse { user: UserDto, temporaryPassword: string }`

### PATCH /api/users/{id}/role
Request: `{ role: 'Vet' | 'Receptionist' | 'Assistant' }`
Response: 200 OK

### DELETE /api/users/{id}
Deactivates the user. Response: 200 OK

---

## Appointments

### GET /api/appointments?date=YYYY-MM-DD
Response: `AppointmentDto[]`

### POST /api/appointments
Request:
```json
{
  "patientName": "Max",
  "species": "Dog",
  "ownerName": "Ahmed Al-Rashid",
  "ownerPhone": "+971501234567",
  "vetId": "uuid",
  "scheduledAt": "2026-04-01T09:00:00",
  "reason": "Vaccination",
  "notes": null
}
```
Response: `AppointmentDto`

### GET /api/appointments/{id}
Response: `AppointmentDto`

### PATCH /api/appointments/{id}/transition
Request: `{ action: 'CHECK_IN' | 'START' | 'COMPLETE' | 'CANCEL' | 'NO_SHOW', reason?: string }`
Response: `AppointmentDto`

### GET /api/appointments/availability?veterinarianId=uuid&date=YYYY-MM-DD&durationMinutes=30
Response: `AvailabilitySlotDto[]`

**AppointmentDto**:
```json
{
  "id": "uuid",
  "clinicId": "uuid",
  "patientName": "Max",
  "species": "Dog",
  "ownerName": "Ahmed Al-Rashid",
  "ownerPhone": null,
  "vetId": "uuid",
  "vetName": "Dr. Sarah Johnson",
  "status": "SCHEDULED",
  "scheduledAt": "2026-04-01T09:00:00",
  "reason": "Vaccination",
  "notes": null,
  "cancellationReason": null
}
```

**Status values**: `SCHEDULED | CHECKED_IN | IN_PROGRESS | COMPLETED | CANCELLED`

---

## Patients

### GET /api/patients?search=&page=1&pageSize=20
Response: `PatientPagedResultDto { items: PatientDto[], totalCount, page, pageSize }`

### POST /api/patients
Request: `CreatePatientRequest { name, species, breed, dateOfBirth, gender, ownerName, ownerPhone, ownerEmail? }`
Response: `PatientDto`

### GET /api/patients/{id}
Response: `PatientDto`

### GET /api/patients/{id}/detail
Response: `PatientDetailDto { patient: PatientDto, recentRecords: MedicalRecordSummaryDto[] }`

### PATCH /api/patients/{id}
Request: `UpdatePatientRequest` (same fields as Create, all optional)
Response: `PatientDto`

**PatientDto**: `{ id, name, species, breed, dateOfBirth, ageYears, gender: 'Male' | 'Female' | 'Unknown', ownerName, ownerPhone, ownerEmail, clinicId, lastVisitDate, nextAppointmentDate }`

**Species values**: `Dog | Cat | Bird | Rabbit | Horse | Camel | Exotic`

---

## Medical Records

### GET /api/patients/{patientId}/medical-records
Response: `MedicalRecordDto[]`

### POST /api/patients/{patientId}/medical-records
Request: `AddMedicalRecordRequest { reason, anamnesis, weight, temperature, heartRate, diagnosis, treatment, prescription?, nextVisitDate? }`
Response: `MedicalRecordDto`

**MedicalRecordDto**: `{ id, patientId, patientName, clinicId, vetName, visitDate, reason, anamnesis, weight, temperature, heartRate, diagnosis, treatment, prescription: string | null, nextVisitDate: string | null }`

---

## Invoices

### GET /api/invoices?status=&dateFrom=&dateTo=&page=1&pageSize=20
Response: `InvoiceDto[]`

### POST /api/invoices
Request:
```json
{
  "patientId": "uuid",
  "patientName": "Max",
  "ownerName": "Ahmed Al-Rashid",
  "ownerPhone": "+971501234567",
  "appointmentId": null,
  "items": [{ "description": "Consultation", "quantity": 1, "unitPrice": 200 }],
  "notes": null
}
```
Response: `InvoiceDto`

### GET /api/invoices/{id}
Response: `InvoiceDto`

### PATCH /api/invoices/{id}/status
Request: `{ status: 'DRAFT' | 'SENT' | 'PAID' | 'CANCELLED' }`
Response: `InvoiceDto`

### PATCH /api/invoices/{id}/send
Shortcut: transitions invoice to Sent. Response: `InvoiceDto`

### PATCH /api/invoices/{id}/pay
Shortcut: transitions invoice to Paid. Response: `InvoiceDto`

### PATCH /api/invoices/{id}/cancel
Shortcut: transitions invoice to Cancelled. Response: `InvoiceDto`

### POST /api/invoices/{id}/items
Request: `{ description: string, unitPrice: number, quantity?: number }`
Response: `InvoiceDto`

**InvoiceDto**:
```json
{
  "id": "uuid",
  "invoiceNumber": "INV-2026-001",
  "patientId": "uuid",
  "patientName": "Max",
  "ownerName": "Ahmed Al-Rashid",
  "ownerPhone": "+971501234567",
  "appointmentId": null,
  "status": "DRAFT",
  "items": [{ "id": "uuid", "description": "Consultation", "quantity": 1, "unitPrice": 200, "subtotal": 200 }],
  "subtotal": 200,
  "vatRate": 0.05,
  "vatAmount": 10,
  "total": 210,
  "notes": null,
  "createdAt": "2026-04-01T10:00:00Z",
  "paidAt": null,
  "dueDate": null,
  "clinicId": "uuid"
}
```

**Status values**: `DRAFT | SENT | PAID | CANCELLED`

---

## Dashboard

### GET /api/dashboard/stats
Response:
```json
{
  "appointmentsToday": 12,
  "pendingCheckin": 3,
  "unpaidInvoicesAed": 4500.00,
  "totalPatients": 248
}
```

### GET /api/dashboard/today-appointments
Response: `TodayAppointmentDto[]`
```json
[{
  "id": "uuid",
  "patientName": "Max",
  "species": "Dog",
  "ownerName": "Ahmed Al-Rashid",
  "vetName": "Dr. Sarah",
  "vetId": "uuid",
  "status": "SCHEDULED",
  "scheduledAt": "2026-04-01T09:00:00"
}]
```

### GET /api/dashboard/recent-activity
Response:
```json
[{
  "id": "uuid",
  "type": "APPOINTMENT",
  "message": "Appointment for Max — Scheduled",
  "occurredAt": "2026-04-01T09:00:00",
  "relatedId": "uuid"
}]
```

---

## Owners

### POST /api/owners
Request: `CreateOwnerRequest { firstName, lastName, email, phone }`
Response: `OwnerDto`

---

## Divergences resolved in this alignment

| Frontend call | Old backend route | New backend route | Change |
|---|---|---|---|
| `GET /api/appointments` | `GET /api/v1/appointments` | `GET /api/appointments` | Removed `/v1/` |
| `GET /api/patients` | `GET /api/v1/patients` | `GET /api/patients` | Removed `/v1/` |
| `GET /api/invoices` | `GET /api/v1/invoices` | `GET /api/invoices` | Removed `/v1/` |
| `GET /api/patients/{id}/medical-records` | `GET /api/v1/patients/{id}/records` | `GET /api/patients/{id}/medical-records` | Removed `/v1/`, renamed segment |
| `PATCH /api/appointments/{id}/transition` | `PATCH /api/v1/appointments/{id}/status` | `PATCH /api/appointments/{id}/transition` | Action-based request |
| `POST /api/users` | `POST /api/users/invite` | `POST /api/users` | Aligned to frontend |
| `GET /api/dashboard/stats` | (missing) | `GET /api/dashboard/stats` | New endpoint |
| `GET /api/dashboard/today-appointments` | (missing) | `GET /api/dashboard/today-appointments` | New endpoint |
| `GET /api/dashboard/recent-activity` | (missing) | `GET /api/dashboard/recent-activity` | New endpoint |

## VAT
All prices are in AED (UAE Dirham). VAT rate: 5% applied automatically to invoice items.
`TotalInclTax = UnitPrice * Quantity * 1.05`

/**
 * Realistic mock data for portal medical records endpoints.
 * UAE context: Arabic/English names, AED currency, Asia/Dubai timezone.
 */
import type {
  PortalAnimalDto,
  PortalMedicalRecordDto,
  PortalVaccinationDto,
  PortalPrescriptionDto,
  PortalWeightEntryDto,
} from '@/lib/api/portal'

// ─── My Animals ──────────────────────────────────────────────────────────────

export const MOCK_PORTAL_ANIMALS: PortalAnimalDto[] = [
  {
    id: 'animal-001',
    name: 'Max',
    species: 'Dog',
    breed: 'Golden Retriever',
    dateOfBirth: '2020-04-15',
    lastVisitDate: '2026-03-10',
  },
  {
    id: 'animal-002',
    name: 'Luna',
    species: 'Cat',
    breed: 'Siamese',
    dateOfBirth: '2022-08-20',
    lastVisitDate: '2026-02-28',
  },
  {
    id: 'animal-003',
    name: 'Simba',
    species: 'Cat',
    breed: 'Persian',
    dateOfBirth: '2021-01-10',
    lastVisitDate: null,
  },
]

// ─── Medical Records (keyed by animal ID) ────────────────────────────────────

export const MOCK_PORTAL_MEDICAL_RECORDS: Record<string, PortalMedicalRecordDto[]> = {
  'animal-001': [
    {
      id: 'rec-001',
      visitDate: '2026-03-10',
      reason: 'Limping on front right leg',
      diagnosis: 'Mild soft tissue sprain',
      treatment: 'Rest for 7 days, anti-inflammatory medication',
      vetName: 'Dr. Sarah Johnson',
    },
    {
      id: 'rec-002',
      visitDate: '2025-12-15',
      reason: 'Annual checkup',
      diagnosis: 'Healthy — no concerns',
      treatment: 'Routine blood work completed, all values normal',
      vetName: 'Dr. Ahmed Al-Rashidi',
    },
    {
      id: 'rec-003',
      visitDate: '2025-06-01',
      reason: 'Ear infection',
      diagnosis: 'Otitis externa — bacterial',
      treatment: 'Ear drops (otic solution) for 10 days',
      vetName: 'Dr. Sarah Johnson',
    },
  ],
  'animal-002': [
    {
      id: 'rec-004',
      visitDate: '2026-02-28',
      reason: 'Post-spay follow-up',
      diagnosis: 'Healing well, sutures intact',
      treatment: 'Continue e-collar for 3 more days',
      vetName: 'Dr. Sarah Johnson',
    },
    {
      id: 'rec-005',
      visitDate: '2026-02-20',
      reason: 'Spay surgery',
      diagnosis: 'Ovariohysterectomy performed',
      treatment: 'Surgery successful, post-op antibiotics prescribed',
      vetName: 'Dr. Ahmed Al-Rashidi',
    },
  ],
}

// ─── Vaccinations ────────────────────────────────────────────────────────────

export const MOCK_PORTAL_VACCINATIONS: Record<string, PortalVaccinationDto[]> = {
  'animal-001': [
    {
      id: 'vax-001',
      name: 'Rabies',
      administeredAt: '2025-12-15',
      nextDueAt: '2026-12-15',
      vetName: 'Dr. Ahmed Al-Rashidi',
    },
    {
      id: 'vax-002',
      name: 'DHPP (Distemper, Hepatitis, Parainfluenza, Parvovirus)',
      administeredAt: '2025-12-15',
      nextDueAt: '2026-12-15',
      vetName: 'Dr. Ahmed Al-Rashidi',
    },
    {
      id: 'vax-003',
      name: 'Bordetella (Kennel Cough)',
      administeredAt: '2025-06-01',
      nextDueAt: '2026-06-01',
      vetName: 'Dr. Sarah Johnson',
    },
  ],
  'animal-002': [
    {
      id: 'vax-004',
      name: 'FVRCP (Feline Distemper)',
      administeredAt: '2025-10-10',
      nextDueAt: '2026-10-10',
      vetName: 'Dr. Sarah Johnson',
    },
    {
      id: 'vax-005',
      name: 'Rabies',
      administeredAt: '2025-10-10',
      nextDueAt: '2026-10-10',
      vetName: 'Dr. Sarah Johnson',
    },
  ],
}

// ─── Prescriptions ───────────────────────────────────────────────────────────

export const MOCK_PORTAL_PRESCRIPTIONS: Record<string, PortalPrescriptionDto[]> = {
  'animal-001': [
    {
      id: 'rx-001',
      drugName: 'Meloxicam 1.5mg',
      dosage: '0.1 mg/kg once daily',
      frequency: 'Once daily with food',
      startDate: '2026-03-10',
      endDate: '2026-03-17',
      prescribedBy: 'Dr. Sarah Johnson',
    },
    {
      id: 'rx-002',
      drugName: 'Surolan Ear Drops',
      dosage: '5 drops per ear',
      frequency: 'Twice daily',
      startDate: '2025-06-01',
      endDate: '2025-06-11',
      prescribedBy: 'Dr. Sarah Johnson',
    },
  ],
  'animal-002': [
    {
      id: 'rx-003',
      drugName: 'Amoxicillin 250mg',
      dosage: '125mg twice daily',
      frequency: 'Twice daily',
      startDate: '2026-02-20',
      endDate: '2026-02-27',
      prescribedBy: 'Dr. Ahmed Al-Rashidi',
    },
  ],
}

// ─── Vaccination Reminders ───────────────────────────────────────────────────

import type {
  VaccinationReminderDto,
  NotificationPreferencesDto,
} from '@/lib/api/portal'

export const MOCK_PORTAL_VACCINATION_REMINDERS: Record<string, VaccinationReminderDto[]> = {
  'animal-001': [
    {
      id: 'vr-001',
      vaccineName: 'Bordetella (Kennel Cough)',
      dueDate: '2026-06-01',
      status: 'Upcoming',
      animalId: 'animal-001',
      animalName: 'Max',
    },
    {
      id: 'vr-002',
      vaccineName: 'Rabies',
      dueDate: '2026-12-15',
      status: 'Upcoming',
      animalId: 'animal-001',
      animalName: 'Max',
    },
    {
      id: 'vr-003',
      vaccineName: 'DHPP (Distemper, Hepatitis, Parainfluenza, Parvovirus)',
      dueDate: '2026-12-15',
      status: 'Upcoming',
      animalId: 'animal-001',
      animalName: 'Max',
    },
  ],
  'animal-002': [
    {
      id: 'vr-004',
      vaccineName: 'FVRCP (Feline Distemper)',
      dueDate: '2026-10-10',
      status: 'Upcoming',
      animalId: 'animal-002',
      animalName: 'Luna',
    },
    {
      id: 'vr-005',
      vaccineName: 'Rabies',
      dueDate: '2026-10-10',
      status: 'Upcoming',
      animalId: 'animal-002',
      animalName: 'Luna',
    },
  ],
}

export const MOCK_NOTIFICATION_PREFERENCES: NotificationPreferencesDto = {
  whatsappEnabled: true,
  emailEnabled: false,
}

// ─── Weight History ──────────────────────────────────────────────────────────

export const MOCK_PORTAL_WEIGHT_HISTORY: Record<string, PortalWeightEntryDto[]> = {
  'animal-001': [
    { date: '2024-06-15', weightKg: 28.5 },
    { date: '2024-12-10', weightKg: 30.2 },
    { date: '2025-06-01', weightKg: 31.0 },
    { date: '2025-12-15', weightKg: 31.8 },
    { date: '2026-03-10', weightKg: 32.1 },
  ],
  'animal-002': [
    { date: '2025-04-15', weightKg: 3.2 },
    { date: '2025-10-10', weightKg: 3.8 },
    { date: '2026-02-20', weightKg: 4.0 },
    { date: '2026-02-28', weightKg: 3.9 },
  ],
}

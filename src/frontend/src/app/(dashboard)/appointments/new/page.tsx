import { AppointmentForm } from '@/components/features/appointments/AppointmentForm'

export default function NewAppointmentPage() {
  return (
    <div className="space-y-6" data-testid="new-appointment-page">
      <h1 className="text-2xl font-bold" data-testid="new-appointment-title">
        New Appointment
      </h1>
      <AppointmentForm />
    </div>
  )
}

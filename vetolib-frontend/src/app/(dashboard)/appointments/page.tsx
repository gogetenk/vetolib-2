import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function AppointmentsPage() {
  return (
    <div className="space-y-6" data-testid="appointments-page">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold" data-testid="appointments-title">
          Appointments
        </h1>
      </div>
      <Card>
        <CardHeader>
          <CardTitle>Upcoming Appointments</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
          </div>
          <p className="mt-4 text-sm text-muted-foreground">
            Appointment management coming soon.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

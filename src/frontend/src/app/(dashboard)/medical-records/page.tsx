import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function MedicalRecordsPage() {
  return (
    <div className="space-y-6" data-testid="medical-records-page">
      <div className="flex items-center justify-between">
        <h1
          className="text-2xl font-bold"
          data-testid="medical-records-title"
        >
          Medical Records
        </h1>
      </div>
      <Card>
        <CardHeader>
          <CardTitle>Records</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
          </div>
          <p className="mt-4 text-sm text-muted-foreground">
            Medical records management coming soon.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

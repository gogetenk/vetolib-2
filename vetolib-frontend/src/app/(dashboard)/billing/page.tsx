import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function BillingPage() {
  return (
    <div className="space-y-6" data-testid="billing-page">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold" data-testid="billing-title">
          Billing
        </h1>
      </div>
      <Card>
        <CardHeader>
          <CardTitle>Invoices</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-12 w-full" />
          </div>
          <p className="mt-4 text-sm text-muted-foreground">
            Billing management coming soon.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

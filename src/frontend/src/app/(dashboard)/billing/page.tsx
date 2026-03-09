import { InvoiceTable } from "@/components/features/billing/InvoiceTable";

export default function BillingPage() {
  return (
    <div className="space-y-6" data-testid="billing-page">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold" data-testid="billing-title">
          Billing
        </h1>
      </div>
      <InvoiceTable />
    </div>
  );
}

import { Skeleton } from "@/components/ui/skeleton";

export default function AuthLoading() {
  return (
    <div
      className="flex min-h-screen items-center justify-center bg-background"
      data-testid="auth-loading"
    >
      <div className="w-full max-w-md px-4">
        <div className="flex flex-col gap-4 rounded-xl border p-6 shadow-sm">
          <Skeleton className="h-7 w-40" />
          <Skeleton className="h-4 w-64" />
          <div className="flex flex-col gap-3 pt-2">
            <Skeleton className="h-10 w-full rounded-md" />
            <Skeleton className="h-10 w-full rounded-md" />
            <Skeleton className="h-10 w-full rounded-md" />
          </div>
        </div>
      </div>
    </div>
  );
}

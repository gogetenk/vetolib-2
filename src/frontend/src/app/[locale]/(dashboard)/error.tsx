"use client";

import { useEffect } from "react";
import { Button } from "@/components/ui/button";

interface ErrorProps {
  error: Error & { digest?: string };
  reset: () => void;
}

export default function DashboardError({ error, reset }: ErrorProps) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div
      className="flex flex-col items-center justify-center gap-4 p-12 text-center"
      data-testid="dashboard-error"
    >
      <h2 className="text-xl font-semibold text-destructive">
        Une erreur est survenue
      </h2>
      <p className="text-sm text-muted-foreground">
        {error.message || "Erreur inattendue. Veuillez réessayer."}
      </p>
      <Button
        variant="outline"
        onClick={reset}
        data-testid="dashboard-error-reset"
      >
        Réessayer
      </Button>
    </div>
  );
}

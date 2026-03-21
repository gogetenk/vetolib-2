"use client";

import React, { useCallback, useEffect, useRef, useState } from "react";
import { Building2, ChevronDown, Check, Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { cn } from "@/lib/utils";
import {
  getClinicGroupClinics,
  switchClinic,
  type ClinicSummary,
} from "@/lib/api/clinic-group";

function parseJwtClaim(key: string): string {
  if (typeof window === "undefined") return "";
  const token = localStorage.getItem("access_token");
  if (!token) return "";
  try {
    const base64 = token.split(".")[1];
    const json = atob(base64.replace(/-/g, "+").replace(/_/g, "/"));
    const payload = JSON.parse(json) as Record<string, unknown>;
    return (payload[key] as string) ?? "";
  } catch {
    return "";
  }
}

export function ClinicSwitcher() {
  const t = useTranslations("nav");
  const [open, setOpen] = useState(false);
  const [clinics, setClinics] = useState<ClinicSummary[]>([]);
  const [loading, setLoading] = useState(false);
  const [switching, setSwitching] = useState(false);
  const [currentClinicId, setCurrentClinicId] = useState(() =>
    parseJwtClaim("clinicId")
  );
  const [currentClinicName, setCurrentClinicName] = useState(() =>
    parseJwtClaim("clinicName")
  );
  const menuRef = useRef<HTMLDivElement>(null);

  const clinicGroupId = parseJwtClaim("clinicGroupId");

  // Fetch clinics on first open
  const fetchClinics = useCallback(async () => {
    if (!clinicGroupId || clinics.length > 0) return;
    setLoading(true);
    try {
      const response = await getClinicGroupClinics(clinicGroupId);
      setClinics(response.clinics);
    } catch {
      // Silently fail — user just won't see the switcher
    } finally {
      setLoading(false);
    }
  }, [clinicGroupId, clinics.length]);

  useEffect(() => {
    if (open && clinics.length === 0) {
      fetchClinics();
    }
  }, [open, clinics.length, fetchClinics]);

  // Also fetch on mount to determine if we should show the switcher
  useEffect(() => {
    if (clinicGroupId) {
      fetchClinics();
    }
  }, [clinicGroupId, fetchClinics]);

  // Close on outside click
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    if (open) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [open]);

  async function handleSwitch(clinicId: string) {
    if (clinicId === currentClinicId || switching) return;
    setSwitching(true);
    try {
      await switchClinic(clinicId);
      // Update local state from the new token
      setCurrentClinicId(parseJwtClaim("clinicId"));
      setCurrentClinicName(parseJwtClaim("clinicName"));
      setOpen(false);
      // Reload the page to refresh all data with new clinic context
      window.location.reload();
    } catch {
      setSwitching(false);
    }
  }

  // Don't render if user has no clinic group or only one clinic
  if (!clinicGroupId || (clinics.length > 0 && clinics.length < 2)) {
    return null;
  }

  // Show static button while still loading (before we know clinic count)
  if (clinics.length === 0 && !loading) {
    return null;
  }

  return (
    <div className="relative" ref={menuRef}>
      <button
        data-testid="clinic-switcher-trigger"
        onClick={() => setOpen((prev) => !prev)}
        className={cn(
          "hidden lg:flex items-center gap-2 px-3 py-1.5 rounded-full",
          "border border-border/80 bg-white hover:bg-muted",
          "transition-colors text-[13px] font-semibold text-foreground",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/50 focus-visible:ring-offset-1",
          open && "bg-muted"
        )}
        aria-expanded={open}
        aria-haspopup="true"
        aria-label={t("switch_clinic")}
      >
        <Building2 className="h-4 w-4 text-muted-foreground" />
        <span className="max-w-[150px] truncate">
          {currentClinicName || t("select_clinic")}
        </span>
        <ChevronDown
          className={cn(
            "h-4 w-4 text-muted-foreground transition-transform duration-200",
            open && "rotate-180"
          )}
        />
      </button>

      {open && (
        <div
          data-testid="clinic-switcher-dropdown"
          className={cn(
            "absolute ltr:left-0 rtl:right-0 top-full z-50 mt-1 w-72",
            "rounded-xl border border-border/80 bg-white text-popover-foreground shadow-xl",
            "p-1.5",
            "animate-in fade-in-0 zoom-in-95 slide-in-from-top-2 duration-200"
          )}
          role="menu"
        >
          <div className="px-2 py-1.5 mb-1">
            <span className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider">
              {t("your_clinics")}
            </span>
          </div>

          {loading ? (
            <div className="flex items-center justify-center py-4">
              <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
            </div>
          ) : (
            clinics.map((clinic) => {
              const isActive = clinic.id === currentClinicId;
              return (
                <button
                  key={clinic.id}
                  data-testid={`clinic-option-${clinic.id}`}
                  onClick={() => handleSwitch(clinic.id)}
                  disabled={switching}
                  className={cn(
                    "flex w-full items-center gap-3 rounded-lg px-2 py-2.5 text-start",
                    "transition-all duration-200 ease-in-out",
                    "hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30",
                    isActive && "bg-primary/5",
                    switching && "opacity-50 cursor-not-allowed"
                  )}
                  role="menuitem"
                >
                  <div
                    className={cn(
                      "flex h-8 w-8 shrink-0 items-center justify-center rounded-lg",
                      isActive
                        ? "bg-primary text-primary-foreground"
                        : "bg-muted text-muted-foreground"
                    )}
                  >
                    <Building2 className="h-4 w-4" />
                  </div>
                  <div className="flex flex-col min-w-0">
                    <span
                      className={cn(
                        "text-[13px] font-semibold truncate",
                        isActive ? "text-primary" : "text-foreground"
                      )}
                    >
                      {clinic.name}
                    </span>
                    <span className="text-[11px] text-muted-foreground truncate">
                      {clinic.address}
                    </span>
                  </div>
                  {isActive && (
                    <Check className="h-4 w-4 shrink-0 text-primary ms-auto" />
                  )}
                </button>
              );
            })
          )}
        </div>
      )}
    </div>
  );
}

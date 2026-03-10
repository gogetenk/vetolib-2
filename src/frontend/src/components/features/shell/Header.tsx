"use client";

import React, { useState } from "react";
import { Menu, PawPrint } from "lucide-react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { useRole } from "@/hooks/use-role";
import { UserMenu } from "./UserMenu";
import { MobileSidebarContent } from "./Sidebar";

function parseClinicName(token: string): string {
  try {
    const base64 = token.split(".")[1];
    const json = atob(base64.replace(/-/g, "+").replace(/_/g, "/"));
    const payload = JSON.parse(json) as Record<string, unknown>;
    return (payload["clinicName"] as string) || "";
  } catch {
    return "";
  }
}

function getInitialClinicName(): string {
  if (typeof window === "undefined") return "";
  const token = localStorage.getItem("access_token");
  return token ? parseClinicName(token) : "";
}

export function Header() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const role = useRole();
  const [clinicName] = useState<string>(getInitialClinicName);

  return (
    <header
      data-testid="dashboard-header"
      className="flex h-16 items-center border-b bg-background px-4 sm:px-6"
    >
      {/* Hamburger — mobile only */}
      <Button
        variant="ghost"
        size="icon"
        data-testid="mobile-menu-trigger"
        className="mr-2 md:hidden"
        onClick={() => setMobileOpen(true)}
        aria-label="Open navigation menu"
      >
        <Menu className="h-5 w-5" />
      </Button>

      {/* Logo */}
      <Link
        href="/appointments"
        data-testid="header-logo"
        className="flex items-center gap-2 font-bold text-lg text-primary"
      >
        <PawPrint className="h-6 w-6" />
        <span>Vetolib</span>
      </Link>

      {/* Clinic name */}
      {clinicName && (
        <span
          data-testid="clinic-name"
          className="ml-4 text-sm font-medium text-muted-foreground mr-auto hidden sm:block"
        >
          {clinicName}
        </span>
      )}
      {!clinicName && <span className="mr-auto" />}

      {/* User menu — right side */}
      <UserMenu />

      {/* Mobile sidebar sheet */}
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetContent side="left" data-testid="mobile-sidebar-sheet">
          <SheetHeader>
            <SheetTitle className="flex items-center gap-2 text-primary">
              <PawPrint className="h-5 w-5" />
              Vetolib
            </SheetTitle>
          </SheetHeader>
          <div className="mt-4 -mx-4">
            <MobileSidebarContent
              role={role}
              onItemClick={() => setMobileOpen(false)}
            />
          </div>
        </SheetContent>
      </Sheet>
    </header>
  );
}

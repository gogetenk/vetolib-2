"use client";

import React, { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { LogOut, Settings, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";
import { clearSession } from "@/lib/auth";

interface UserInfo {
  fullName: string;
  role: string;
  initials: string;
}

function parseJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const base64 = token.split(".")[1];
    const json = atob(base64.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(json);
  } catch {
    return null;
  }
}

function getInitials(name: string): string {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);
}

function getInitialUserInfo(): UserInfo {
  if (typeof window === "undefined") return { fullName: "User", role: "VET", initials: "U" };
  const token = localStorage.getItem("access_token");
  if (token) {
    const payload = parseJwtPayload(token);
    if (payload) {
      const name =
        (payload["name"] as string) ||
        (payload["sub"] as string) ||
        "User";
      const role =
        (payload["role"] as string) ||
        (payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] as string) ||
        "VET";
      return { fullName: name, role, initials: getInitials(name) };
    }
  }
  return { fullName: "User", role: "VET", initials: "U" };
}

export function UserMenu() {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [userInfo] = useState<UserInfo>(getInitialUserInfo);
  const menuRef = useRef<HTMLDivElement>(null);

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

  function handleSignOut() {
    clearSession();
    // Also clear cookie used by middleware
    document.cookie =
      "access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    router.push("/login");
  }

  return (
    <div className="relative" ref={menuRef}>
      <button
        data-testid="user-menu-trigger"
        onClick={() => setOpen((prev) => !prev)}
        className={cn(
          "flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium",
          "transition-all duration-200 ease-in-out",
          "hover:bg-accent hover:text-accent-foreground",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/50 focus-visible:ring-offset-1",
          "active:scale-[0.98]",
          open && "bg-accent text-accent-foreground"
        )}
        aria-expanded={open}
        aria-haspopup="true"
        aria-label="User menu"
      >
        {/* Avatar */}
        <span
          data-testid="user-avatar"
          className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-primary-foreground text-xs font-bold select-none transition-transform duration-200 ease-in-out hover:scale-105"
        >
          {userInfo.initials}
        </span>
        <span className="hidden sm:flex flex-col items-start leading-tight">
          <span data-testid="user-fullname" className="text-sm font-medium">
            {userInfo.fullName}
          </span>
          <span data-testid="user-role" className="text-xs text-muted-foreground capitalize">
            {userInfo.role.toLowerCase().replace("_", " ")}
          </span>
        </span>
        <ChevronDown
          className={cn(
            "h-4 w-4 text-muted-foreground transition-transform duration-200 ease-in-out",
            open && "rotate-180"
          )}
        />
      </button>

      {open && (
        <div
          data-testid="user-menu-dropdown"
          className={cn(
            "absolute ltr:right-0 rtl:left-0 top-full z-50 mt-1 w-52",
            "rounded-md border bg-popover text-popover-foreground shadow-lg",
            "p-1",
            "animate-in fade-in-0 zoom-in-95 slide-in-from-top-2 duration-200"
          )}
          role="menu"
        >
          {/* User info header */}
          <div className="px-2 py-2 mb-1">
            <div className="flex items-center gap-2">
              <span className="flex h-9 w-9 items-center justify-center rounded-full bg-primary text-primary-foreground text-sm font-bold select-none">
                {userInfo.initials}
              </span>
              <div className="flex flex-col leading-tight">
                <span className="text-sm font-medium">{userInfo.fullName}</span>
                <span className="text-xs text-muted-foreground capitalize">
                  {userInfo.role.toLowerCase().replace("_", " ")}
                </span>
              </div>
            </div>
          </div>

          <div className="my-1 h-px bg-border" />

          <Link
            href="/settings"
            data-testid="user-menu-settings"
            className="flex items-center gap-2 rounded-sm px-2 py-2 text-sm transition-all duration-200 ease-in-out hover:bg-accent hover:text-accent-foreground hover:translate-x-0.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/50"
            role="menuitem"
            onClick={() => setOpen(false)}
          >
            <Settings className="h-4 w-4 transition-transform duration-200 ease-in-out group-hover:rotate-45" />
            Settings
          </Link>

          <button
            data-testid="user-menu-signout"
            onClick={handleSignOut}
            className="flex w-full items-center gap-2 rounded-sm px-2 py-2 text-sm text-destructive transition-all duration-200 ease-in-out hover:bg-destructive/10 hover:translate-x-0.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-destructive/50"
            role="menuitem"
          >
            <LogOut className="h-4 w-4 transition-transform duration-200 ease-in-out" />
            Sign Out
          </button>
        </div>
      )}
    </div>
  );
}

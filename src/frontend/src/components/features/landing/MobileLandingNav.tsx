"use client";

import React, { useState } from "react";
import Link from "next/link";
import { Menu } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";

interface NavLink {
  label: string;
  href: string;
  testId: string;
}

interface Props {
  links: NavLink[];
  signInLabel: string;
  ctaLabel: string;
  loginHref: string;
  signupHref: string;
}

export function MobileLandingNav({ links, signInLabel, ctaLabel, loginHref, signupHref }: Props) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <Button
        variant="ghost"
        size="icon"
        data-testid="landing-mobile-menu-trigger"
        className="md:hidden"
        onClick={() => setOpen(true)}
        aria-label="Open menu"
      >
        <Menu className="h-5 w-5" />
      </Button>

      <Sheet open={open} onOpenChange={setOpen}>
        <SheetContent side="right" data-testid="landing-mobile-menu-sheet" className="w-72">
          <SheetHeader className="border-b pb-4">
            <SheetTitle className="text-lg font-bold text-primary">
              Vetara
            </SheetTitle>
          </SheetHeader>
          <nav className="flex flex-col gap-2 pt-4">
            {links.map((link) => (
              <a
                key={link.testId}
                href={link.href}
                data-testid={`${link.testId}-mobile`}
                className="rounded-lg px-3 py-2.5 text-sm font-medium text-stone-600 transition-colors hover:bg-accent hover:text-primary"
                onClick={() => setOpen(false)}
              >
                {link.label}
              </a>
            ))}
            <div className="mt-4 flex flex-col gap-2 border-t pt-4">
              <Link
                href={loginHref}
                onClick={() => setOpen(false)}
                className="inline-flex items-center justify-center rounded-md border border-input bg-background px-4 py-2 text-sm font-medium hover:bg-accent hover:text-accent-foreground w-full"
                data-testid="btn-mobile-signin"
              >
                {signInLabel}
              </Link>
              <Link
                href={signupHref}
                onClick={() => setOpen(false)}
                className="inline-flex items-center justify-center rounded-md bg-primary text-white px-4 py-2 text-sm font-medium hover:bg-primary/90 w-full"
                data-testid="btn-mobile-start-trial"
              >
                {ctaLabel}
              </Link>
            </div>
          </nav>
        </SheetContent>
      </Sheet>
    </>
  );
}

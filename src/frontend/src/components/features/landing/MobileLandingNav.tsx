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
            <SheetTitle className="text-lg font-bold text-emerald-700">
              Vetolib
            </SheetTitle>
          </SheetHeader>
          <nav className="flex flex-col gap-2 pt-4">
            {links.map((link) => (
              <a
                key={link.testId}
                href={link.href}
                data-testid={`${link.testId}-mobile`}
                className="rounded-lg px-3 py-2.5 text-sm font-medium text-stone-600 transition-colors hover:bg-emerald-50 hover:text-emerald-700"
                onClick={() => setOpen(false)}
              >
                {link.label}
              </a>
            ))}
            <div className="mt-4 flex flex-col gap-2 border-t pt-4">
              <Link href={loginHref} onClick={() => setOpen(false)}>
                <Button variant="outline" className="w-full" data-testid="btn-mobile-signin">
                  {signInLabel}
                </Button>
              </Link>
              <Link href={signupHref} onClick={() => setOpen(false)}>
                <Button
                  className="w-full bg-emerald-700 text-white hover:bg-emerald-800"
                  data-testid="btn-mobile-start-trial"
                >
                  {ctaLabel}
                </Button>
              </Link>
            </div>
          </nav>
        </SheetContent>
      </Sheet>
    </>
  );
}

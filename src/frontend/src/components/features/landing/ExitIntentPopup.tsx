"use client";

import { useEffect, useState, useCallback, useRef, type FormEvent } from "react";
import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { trackEvent } from "@/lib/analytics";

interface Props {
  headline: string;
  subtitle: string;
  emailPlaceholder: string;
  ctaLabel: string;
  successMessage: string;
}

const COOKIE_NAME = "vetolib_exit_intent_dismissed";
const COOKIE_DAYS = 7;

function setCookie(name: string, value: string, days: number) {
  const d = new Date();
  d.setTime(d.getTime() + days * 24 * 60 * 60 * 1000);
  document.cookie = `${name}=${value};expires=${d.toUTCString()};path=/;SameSite=Lax`;
}

function getCookie(name: string): string | null {
  const match = document.cookie.match(new RegExp(`(^| )${name}=([^;]+)`));
  return match ? match[2] : null;
}

/**
 * Exit-intent popup that shows when the user moves the mouse toward the top of the page.
 * Sets a cookie to avoid re-displaying for 7 days after dismissal.
 */
export function ExitIntentPopup({
  headline,
  subtitle,
  emailPlaceholder,
  ctaLabel,
  successMessage,
}: Props) {
  const [open, setOpen] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const dismissedRef = useRef(false);

  const dismiss = useCallback(() => {
    setOpen(false);
    dismissedRef.current = true;
    setCookie(COOKIE_NAME, "1", COOKIE_DAYS);
  }, []);

  useEffect(() => {
    // Don't show if already dismissed via cookie
    const alreadyDismissed = getCookie(COOKIE_NAME);
    if (alreadyDismissed) {
      dismissedRef.current = true;
      return;
    }

    function handleMouseOut(e: MouseEvent) {
      // Trigger only when mouse leaves toward the top of the viewport
      if (e.clientY <= 5 && !dismissedRef.current) {
        setOpen(true);
      }
    }

    // Delay attaching the listener to avoid triggering on page load
    const timeout = setTimeout(() => {
      document.addEventListener("mouseout", handleMouseOut);
    }, 5000);

    return () => {
      clearTimeout(timeout);
      document.removeEventListener("mouseout", handleMouseOut);
    };
  }, []);

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    trackEvent("exit_intent_email_captured");
    setSubmitted(true);
    setCookie(COOKIE_NAME, "1", COOKIE_DAYS);
    setTimeout(() => {
      setOpen(false);
    }, 2000);
  }

  if (!open) return null;

  return (
    <div
      data-testid="exit-intent-overlay"
      className="fixed inset-0 z-[70] flex items-center justify-center bg-black/50 backdrop-blur-sm"
      onClick={(e) => {
        if (e.target === e.currentTarget) dismiss();
      }}
      role="dialog"
      aria-modal="true"
      aria-label={headline}
    >
      <div
        data-testid="exit-intent-popup"
        className="relative mx-4 w-full max-w-md animate-in fade-in slide-in-from-bottom-4 rounded-2xl bg-white p-8 shadow-2xl"
      >
        <button
          onClick={dismiss}
          className="absolute right-4 top-4 rounded-full p-1 text-stone-400 transition-colors hover:bg-stone-100 hover:text-stone-600"
          aria-label="Close"
          data-testid="exit-intent-close"
        >
          <X className="h-5 w-5" />
        </button>

        {submitted ? (
          <div className="text-center" data-testid="exit-intent-success">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-secondary">
              <svg className="h-6 w-6 text-primary/85" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <p className="text-lg font-semibold text-stone-900">{successMessage}</p>
          </div>
        ) : (
          <>
            <div className="mb-6 text-center">
              <h3 className="text-xl font-bold text-stone-900">{headline}</h3>
              <p className="mt-2 text-sm text-stone-600">{subtitle}</p>
            </div>
            <form onSubmit={handleSubmit} className="space-y-4" data-testid="exit-intent-form">
              <Input
                type="email"
                name="email"
                required
                placeholder={emailPlaceholder}
                className="w-full"
                data-testid="exit-intent-email"
              />
              <Button
                type="submit"
                className="w-full bg-primary font-semibold text-white hover:bg-primary/90"
                data-testid="exit-intent-submit"
              >
                {ctaLabel}
              </Button>
            </form>
          </>
        )}
      </div>
    </div>
  );
}

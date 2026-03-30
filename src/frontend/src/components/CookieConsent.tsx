"use client";

import { useCallback, useSyncExternalStore } from "react";
import { useTranslations } from "next-intl";

const STORAGE_KEY = "cookie-consent";
const CONSENT_CHANGE_EVENT = "cookie-consent-change";

export type ConsentValue = "accepted" | "necessary";

export function getConsentValue(): ConsentValue | null {
  if (typeof window === "undefined") return null;
  const value = localStorage.getItem(STORAGE_KEY);
  if (value === "accepted" || value === "necessary") return value;
  return null;
}

/** Subscribe to consent changes via custom DOM event. */
function subscribeToConsent(callback: () => void) {
  window.addEventListener(CONSENT_CHANGE_EVENT, callback);
  window.addEventListener("storage", callback);
  return () => {
    window.removeEventListener(CONSENT_CHANGE_EVENT, callback);
    window.removeEventListener("storage", callback);
  };
}

function getConsentSnapshot(): ConsentValue | null {
  return getConsentValue();
}

function getConsentServerSnapshot(): ConsentValue | null {
  return null;
}

export function CookieConsent() {
  const t = useTranslations("cookieConsent");
  const consent = useSyncExternalStore(
    subscribeToConsent,
    getConsentSnapshot,
    getConsentServerSnapshot,
  );

  const handleAcceptAll = useCallback(() => {
    localStorage.setItem(STORAGE_KEY, "accepted");
    window.dispatchEvent(new Event(CONSENT_CHANGE_EVENT));
  }, []);

  const handleNecessaryOnly = useCallback(() => {
    localStorage.setItem(STORAGE_KEY, "necessary");
    window.dispatchEvent(new Event(CONSENT_CHANGE_EVENT));
  }, []);

  if (consent) return null;

  return (
    <div
      data-testid="cookie-consent-banner"
      className="fixed bottom-0 inset-x-0 z-50 bg-white dark:bg-gray-900 border-t border-gray-200 dark:border-gray-700 p-4 shadow-lg"
      role="banner"
      aria-label={t("ariaLabel")}
    >
      <div className="mx-auto max-w-5xl flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <p className="text-sm text-gray-700 dark:text-gray-300">
          {t("message")}
        </p>
        <div className="flex shrink-0 gap-2">
          <button
            data-testid="cookie-necessary-only"
            onClick={handleNecessaryOnly}
            className="rounded-md border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
          >
            {t("necessaryOnly")}
          </button>
          <button
            data-testid="cookie-accept-all"
            onClick={handleAcceptAll}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 transition-colors"
          >
            {t("acceptAll")}
          </button>
        </div>
      </div>
    </div>
  );
}

"use client";

import Script from "next/script";
import { useSyncExternalStore } from "react";
import { getConsentValue } from "./CookieConsent";

const GA_ID = process.env.NEXT_PUBLIC_GA_ID;
const CONSENT_CHANGE_EVENT = "cookie-consent-change";

/** Subscribe to consent changes via custom DOM event. */
function subscribeToConsent(callback: () => void) {
  window.addEventListener(CONSENT_CHANGE_EVENT, callback);
  window.addEventListener("storage", callback);
  return () => {
    window.removeEventListener(CONSENT_CHANGE_EVENT, callback);
    window.removeEventListener("storage", callback);
  };
}

function getAcceptedSnapshot(): boolean {
  return getConsentValue() === "accepted";
}

function getAcceptedServerSnapshot(): boolean {
  return false;
}

/**
 * Loads GA4 gtag.js only when:
 * 1. NEXT_PUBLIC_GA_ID is set
 * 2. The user has accepted all cookies (cookie-consent === "accepted")
 *
 * Uses useSyncExternalStore to reactively read localStorage
 * so GA4 activates immediately after the user clicks "Accept All".
 */
export function GoogleAnalytics() {
  const consentGiven = useSyncExternalStore(
    subscribeToConsent,
    getAcceptedSnapshot,
    getAcceptedServerSnapshot,
  );

  if (!GA_ID || !consentGiven) return null;

  return (
    <>
      <Script
        src={`https://www.googletagmanager.com/gtag/js?id=${GA_ID}`}
        strategy="afterInteractive"
        data-testid="ga4-script"
      />
      <Script id="ga4-init" strategy="afterInteractive">
        {`
          window.dataLayer = window.dataLayer || [];
          function gtag(){dataLayer.push(arguments);}
          gtag('js', new Date());
          gtag('config', '${GA_ID}', { send_page_view: true });
        `}
      </Script>
    </>
  );
}

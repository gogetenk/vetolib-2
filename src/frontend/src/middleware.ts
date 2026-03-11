import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

/**
 * Middleware to protect dashboard routes.
 * Redirects to /{locale}/login if no auth token cookie is present.
 * All pages are under app/[locale]/ — locale prefix is required.
 *
 * Supported locales: en, ar (default: en)
 * Public routes: /{locale}/login, /api/v1/auth/*, /{locale}/portal/*
 */

const SUPPORTED_LOCALES = ["en", "ar"] as const;
const DEFAULT_LOCALE = "en";

/**
 * Extracts the locale from the pathname.
 * Expects paths like /en/... or /ar/...
 * Falls back to DEFAULT_LOCALE if no valid locale segment is found.
 */
function extractLocale(pathname: string): string {
  const segments = pathname.split("/");
  // segments[0] is always "" (before the leading slash)
  const maybeLocale = segments[1];
  if (SUPPORTED_LOCALES.includes(maybeLocale as (typeof SUPPORTED_LOCALES)[number])) {
    return maybeLocale;
  }
  return DEFAULT_LOCALE;
}

/**
 * Returns true if the pathname is a public path that does not require authentication.
 * Public paths:
 *  - /{locale}/login (exact match or sub-path)
 *  - /api/v1/auth (any sub-path)
 *  - /portal/ (any locale, portal is always public)
 *  - /{locale}/portal/ (any sub-path)
 */
function isPublicPath(pathname: string): boolean {
  // API auth routes are always public regardless of locale
  if (pathname.startsWith("/api/v1/auth")) {
    return true;
  }

  // Portal is public at any locale: /portal/... or /en/portal/... or /ar/portal/...
  if (/\/portal\//.test(pathname)) {
    return true;
  }

  // Locale-prefixed login: /en/login or /ar/login
  for (const locale of SUPPORTED_LOCALES) {
    const loginPath = `/${locale}/login`;
    if (pathname === loginPath || pathname.startsWith(`${loginPath}/`)) {
      return true;
    }
  }

  return false;
}

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Allow static assets and Next.js internals
  if (
    pathname.startsWith("/_next") ||
    pathname.startsWith("/favicon") ||
    pathname.includes(".")
  ) {
    return NextResponse.next();
  }

  // Check for auth token in cookies (set by the client after login)
  const token = request.cookies.get("access_token")?.value;

  // Determine the locale for redirects
  const locale = extractLocale(pathname);

  // Handle root path: redirect to /{locale}/login or /{locale}/appointments
  if (pathname === "/") {
    if (token) {
      return NextResponse.redirect(new URL(`/${locale}/appointments`, request.url));
    }
    return NextResponse.redirect(new URL(`/${locale}/login`, request.url));
  }

  // Handle bare /login without locale prefix → redirect to /{locale}/login
  if (pathname === "/login" || pathname.startsWith("/login/")) {
    if (token) {
      return NextResponse.redirect(new URL(`/${locale}/appointments`, request.url));
    }
    return NextResponse.redirect(new URL(`/${locale}/login`, request.url));
  }

  // Redirect authenticated users away from locale-prefixed login pages
  if (isPublicPath(pathname) && token) {
    return NextResponse.redirect(new URL(`/${locale}/appointments`, request.url));
  }

  // Allow public paths for unauthenticated users
  if (isPublicPath(pathname)) {
    return NextResponse.next();
  }

  // Protect all other routes: redirect to /{locale}/login if not authenticated
  if (!token) {
    const loginUrl = new URL(`/${locale}/login`, request.url);
    loginUrl.searchParams.set("callbackUrl", pathname);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * Match all request paths except:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     */
    "/((?!_next/static|_next/image|favicon.ico).*)",
  ],
};

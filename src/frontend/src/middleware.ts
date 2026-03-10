import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

/**
 * Middleware to protect dashboard routes.
 * Redirects to /login if no auth token cookie is present.
 *
 * Public routes: /login, /api/auth/*
 */

const publicPaths = ["/login", "/api/auth"];

// Routes that are public for any locale prefix (e.g. /en/portal, /ar/portal)
const publicPatterns = [/\/portal\//];

function isPublicPath(pathname: string): boolean {
  if (publicPaths.some((path) => pathname === path || pathname.startsWith(`${path}/`))) {
    return true;
  }
  return publicPatterns.some((pattern) => pattern.test(pathname));
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

  // Redirect authenticated users away from /login
  if (isPublicPath(pathname) && token) {
    return NextResponse.redirect(new URL("/appointments", request.url));
  }

  // Allow public paths for unauthenticated users
  if (isPublicPath(pathname)) {
    return NextResponse.next();
  }

  if (!token) {
    const loginUrl = new URL("/login", request.url);
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

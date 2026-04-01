import NextAuth from "next-auth"
import Keycloak from "next-auth/providers/keycloak"

/**
 * Keycloak OIDC configuration for Vetolib.
 *
 * Uses PKCE (S256) flow with the "vetolib-frontend" public client.
 * Organization context comes from the /api/v1/auth/my-organizations endpoint,
 * not from the token itself.
 *
 * Environment variables:
 *  - AUTH_KEYCLOAK_ID: client id (default: vetolib-frontend)
 *  - AUTH_KEYCLOAK_ISSUER: full issuer URL (e.g. http://localhost:8080/realms/vetolib)
 *  - AUTH_SECRET: random secret for session encryption
 */

export const {
  handlers,
  signIn,
  signOut,
  auth,
} = NextAuth({
  providers: [
    Keycloak({
      clientId: process.env.AUTH_KEYCLOAK_ID ?? "vetolib-frontend",
      // Public client — no clientSecret needed for PKCE
      clientSecret: process.env.AUTH_KEYCLOAK_SECRET ?? "",
      issuer: process.env.AUTH_KEYCLOAK_ISSUER ?? `${process.env.NEXT_PUBLIC_KEYCLOAK_URL ?? "http://localhost:8080"}/realms/vetolib`,
      authorization: {
        params: {
          scope: "openid profile email",
        },
      },
    }),
  ],
  callbacks: {
    async jwt({ token, account }) {
      // On initial sign-in, persist the access token and refresh token from Keycloak
      if (account) {
        token.accessToken = account.access_token
        token.refreshToken = account.refresh_token
        token.idToken = account.id_token
        token.expiresAt = account.expires_at
      }
      return token
    },
    async session({ session, token }) {
      // Expose access token to the client session for API calls
      session.accessToken = token.accessToken as string | undefined
      session.idToken = token.idToken as string | undefined
      return session
    },
  },
  pages: {
    signIn: "/en/login",
  },
  // Trust the Keycloak issuer for PKCE redirect
  trustHost: true,
})

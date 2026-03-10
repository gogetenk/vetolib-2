import createNextIntlPlugin from "next-intl/plugin";
import type { NextConfig } from "next";

const withNextIntl = createNextIntlPlugin();

const nextConfig: NextConfig = {
  async rewrites() {
    // Only proxy /api/* to the real backend when BACKEND_URL is explicitly set.
    // During MSW dev mode, BACKEND_URL is unset and NEXT_PUBLIC_API_URL is empty,
    // so the browser's service worker can intercept requests before they hit the network.
    const backendUrl = process.env.BACKEND_URL;
    if (!backendUrl) {
      return [];
    }
    return [
      {
        source: "/api/:path*",
        destination: `${backendUrl}/api/:path*`,
      },
    ];
  },
};

export default withNextIntl(nextConfig);

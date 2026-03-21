import type { MetadataRoute } from "next";

export default function robots(): MetadataRoute.Robots {
  return {
    rules: [
      {
        userAgent: "*",
        allow: "/",
        disallow: ["/api/", "/en/dashboard/", "/ar/dashboard/"],
      },
    ],
    sitemap: "https://vetolib.com/sitemap.xml",
  };
}

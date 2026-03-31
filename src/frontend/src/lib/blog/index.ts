import { BlogArticle } from "./types";
import { veterinarySoftwareUaeGuide } from "./articles/veterinary-software-uae-guide";
import { whatsappIntegrationDubaiVet } from "./articles/whatsapp-integration-dubai-vet";
import { aiVeterinaryTriage } from "./articles/ai-veterinary-triage";
import { arabicSoftwareDubaiVet } from "./articles/arabic-software-dubai-vet";
import { outgrownSpreadsheetsVet } from "./articles/outgrown-spreadsheets-vet";
import { falconHealthManagementUae } from "./articles/falcon-health-management-uae";
import { breedingManagementUae } from "./articles/breeding-management-uae";
import { howToChooseVeterinarySoftwareUae } from "./articles/how-to-choose-veterinary-software-uae";
import { firstVetVisitWhatToExpect } from "./articles/first-vet-visit-what-to-expect";

export type { BlogArticle } from "./types";

export const allArticles: BlogArticle[] = [
  firstVetVisitWhatToExpect,
  howToChooseVeterinarySoftwareUae,
  falconHealthManagementUae,
  breedingManagementUae,
  outgrownSpreadsheetsVet,
  arabicSoftwareDubaiVet,
  aiVeterinaryTriage,
  whatsappIntegrationDubaiVet,
  veterinarySoftwareUaeGuide,
];

/** Articles sorted by date descending (newest first). */
export function getArticlesSorted(): BlogArticle[] {
  return [...allArticles].sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
  );
}

/** Get a single article by slug, or undefined. */
export function getArticleBySlug(slug: string): BlogArticle | undefined {
  return allArticles.find((a) => a.slug === slug);
}

/** Get all unique tags across articles. */
export function getAllTags(): string[] {
  const tagSet = new Set<string>();
  for (const article of allArticles) {
    for (const tag of article.tags) {
      tagSet.add(tag);
    }
  }
  return Array.from(tagSet).sort();
}

/** Get related articles for a given article. */
export function getRelatedArticles(article: BlogArticle): BlogArticle[] {
  return article.relatedSlugs
    .map((slug) => allArticles.find((a) => a.slug === slug))
    .filter((a): a is BlogArticle => a !== undefined);
}

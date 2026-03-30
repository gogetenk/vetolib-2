import {
  HeartPulse,
  Brain,
  MessageSquare,
  Package,
  Globe,
  FileText,
  MessageCircle,
  Building2,
  Dna,
} from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { ScrollReveal } from "./ScrollReveal";

interface FeatureCard {
  key: string;
  icon: React.ElementType;
  title: string;
  description: string;
  badge?: string;
}

interface CardData {
  title: string;
  description: string;
  badge?: string;
}

interface Props {
  title: string;
  subtitle: string;
  cards: {
    health_passport: { title: string; description: string; badge: string };
    ai: { title: string; description: string; badge: string };
    messaging: { title: string; description: string };
    stock: { title: string; description: string };
    multilingual: { title: string; description: string };
    ai_soap?: CardData;
    whatsapp?: CardData;
    file_attachments?: CardData;
    multi_clinic?: CardData;
    breeding?: CardData;
  };
}

const ICONS: Record<string, React.ElementType> = {
  health_passport: HeartPulse,
  ai: Brain,
  messaging: MessageSquare,
  stock: Package,
  multilingual: Globe,
  ai_soap: FileText,
  whatsapp: MessageCircle,
  file_attachments: Package,
  multi_clinic: Building2,
  breeding: Dna,
};

export function FeaturesSection({ title, subtitle, cards }: Props) {
  const features: FeatureCard[] = [
    {
      key: "health_passport",
      icon: ICONS.health_passport,
      title: cards.health_passport.title,
      description: cards.health_passport.description,
      badge: cards.health_passport.badge,
    },
    {
      key: "ai",
      icon: ICONS.ai,
      title: cards.ai.title,
      description: cards.ai.description,
      badge: cards.ai.badge,
    },
    {
      key: "messaging",
      icon: ICONS.messaging,
      title: cards.messaging.title,
      description: cards.messaging.description,
    },
    {
      key: "stock",
      icon: ICONS.stock,
      title: cards.stock.title,
      description: cards.stock.description,
    },
    {
      key: "multilingual",
      icon: ICONS.multilingual,
      title: cards.multilingual.title,
      description: cards.multilingual.description,
    },
  ];

  // Add optional v2/v3 feature cards
  const optionalCards: Array<{ key: string; data?: CardData }> = [
    { key: "ai_soap", data: cards.ai_soap },
    { key: "whatsapp", data: cards.whatsapp },
    { key: "file_attachments", data: cards.file_attachments },
    { key: "multi_clinic", data: cards.multi_clinic },
    { key: "breeding", data: cards.breeding },
  ];
  for (const { key, data } of optionalCards) {
    if (data) {
      features.push({
        key,
        icon: ICONS[key] || Package,
        title: data.title,
        description: data.description,
        badge: data.badge,
      });
    }
  }

  return (
    <section
      id="features"
      data-testid="section-features"
      className="bg-gradient-to-br from-secondary/50 via-white to-accent/50 py-20 sm:py-28"
    >
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <ScrollReveal direction="fade-up">
          <div className="mx-auto max-w-2xl text-center">
            <h2
              data-testid="new-features-title"
              className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl"
            >
              {title}
            </h2>
            <p className="mt-4 text-lg text-stone-600">{subtitle}</p>
          </div>
        </ScrollReveal>

        <div className="mt-16 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {features.map(({ key, icon: Icon, title: cardTitle, description, badge }, idx) => (
            <ScrollReveal key={key} delay={idx * 100} direction="fade-up">
              <Card
                data-testid={`new-feature-card-${key}`}
                className={`group relative border shadow-sm transition-all duration-300 hover:shadow-lg hover:-translate-y-1 ${
                  key === "health_passport"
                    ? "border-primary/20 bg-white lg:col-span-1"
                    : "border-stone-100 bg-white"
                }`}
              >
                <CardContent className="p-6">
                  {badge && (
                    <span
                      data-testid={`new-feature-badge-${key}`}
                      className="mb-3 inline-flex items-center rounded-full bg-secondary px-2.5 py-0.5 text-xs font-semibold text-primary"
                    >
                      {badge}
                    </span>
                  )}
                  <div
                    className={`flex h-12 w-12 items-center justify-center rounded-xl transition-all duration-300 group-hover:bg-secondary group-hover:scale-110 ${
                      key === "health_passport"
                        ? "bg-secondary text-primary"
                        : "bg-accent text-primary"
                    }`}
                  >
                    <Icon className="h-6 w-6" aria-hidden="true" />
                  </div>
                  <h3 className="mt-4 text-base font-semibold text-stone-900">
                    {cardTitle}
                  </h3>
                  <p className="mt-2 text-sm leading-relaxed text-stone-600">
                    {description}
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  );
}

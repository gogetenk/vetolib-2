import {
  ShieldAlert,
  HeartPulse,
  MessageSquareWarning,
  Stethoscope,
  FileText,
  CalendarClock,
  Sparkles,
} from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { ScrollReveal } from "./ScrollReveal";

interface AiFeatureCard {
  key: string;
  icon: React.ElementType;
  title: string;
  description: string;
}

interface Props {
  title: string;
  subtitle: string;
  badge: string;
  cards: {
    drug_interaction: { title: string; description: string };
    predictive_alerts: { title: string; description: string };
    message_triage: { title: string; description: string };
    symptom_triage: { title: string; description: string };
    soap_notes: { title: string; description: string };
    no_show: { title: string; description: string };
  };
}

const AI_FEATURES: Array<{ key: keyof Props["cards"]; icon: React.ElementType }> = [
  { key: "drug_interaction", icon: ShieldAlert },
  { key: "predictive_alerts", icon: HeartPulse },
  { key: "message_triage", icon: MessageSquareWarning },
  { key: "symptom_triage", icon: Stethoscope },
  { key: "soap_notes", icon: FileText },
  { key: "no_show", icon: CalendarClock },
];

export function AiFeaturesSection({ title, subtitle, badge, cards }: Props) {
  const features: AiFeatureCard[] = AI_FEATURES.map(({ key, icon }) => ({
    key,
    icon,
    title: cards[key].title,
    description: cards[key].description,
  }));

  return (
    <section
      id="ai-features"
      data-testid="section-ai-features"
      className="bg-gradient-to-br from-primary/5 via-white to-secondary/30 py-20 sm:py-28"
    >
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <ScrollReveal direction="fade-up">
          <div className="mx-auto max-w-2xl text-center">
            <span
              data-testid="ai-features-badge"
              className="mb-4 inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-4 py-1.5 text-sm font-semibold text-primary"
            >
              <Sparkles className="h-4 w-4" aria-hidden="true" />
              {badge}
            </span>
            <h2
              data-testid="ai-features-title"
              className="mt-4 text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl"
            >
              {title}
            </h2>
            <p className="mt-4 text-lg text-stone-600">{subtitle}</p>
          </div>
        </ScrollReveal>

        <div className="mt-16 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {features.map(({ key, icon: Icon, title: cardTitle, description }, idx) => (
            <ScrollReveal key={key} delay={idx * 100} direction="fade-up">
              <Card
                data-testid={`ai-feature-card-${key}`}
                className="group relative border border-primary/10 bg-white shadow-sm transition-all duration-300 hover:shadow-lg hover:-translate-y-1 hover:border-primary/25"
              >
                <CardContent className="p-6">
                  <span
                    data-testid={`ai-feature-badge-${key}`}
                    className="mb-3 inline-flex items-center gap-1 rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-semibold text-primary"
                  >
                    <Sparkles className="h-3 w-3" aria-hidden="true" />
                    {badge}
                  </span>
                  <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10 text-primary transition-all duration-300 group-hover:bg-primary group-hover:text-white group-hover:scale-110">
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

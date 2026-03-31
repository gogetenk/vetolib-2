import { ScrollReveal } from "./ScrollReveal";
import { AnimatedStat } from "./AnimatedStat";

interface Props {
  title: string;
  stats: Array<{
    value: string;
    label: string;
    testId: string;
  }>;
}

export function AiImpactSection({ title, stats }: Props) {
  return (
    <section
      data-testid="section-ai-impact"
      className="border-y border-primary/10 bg-primary/5"
    >
      <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <ScrollReveal direction="fade-up">
          <h3
            data-testid="ai-impact-title"
            className="mb-8 text-center text-lg font-semibold text-stone-700"
          >
            {title}
          </h3>
        </ScrollReveal>
        <dl className="grid grid-cols-1 gap-8 sm:grid-cols-3">
          {stats.map(({ value, label, testId }, index) => (
            <ScrollReveal key={testId} delay={index * 150} direction="fade-up">
              <div
                className="flex flex-col items-center text-center"
                data-testid={testId}
              >
                <dt className="text-3xl font-extrabold text-primary sm:text-4xl">
                  <AnimatedStat value={value} delay={index * 150} />
                </dt>
                <dd className="mt-2 text-sm font-medium text-stone-600">
                  {label}
                </dd>
              </div>
            </ScrollReveal>
          ))}
        </dl>
      </div>
    </section>
  );
}

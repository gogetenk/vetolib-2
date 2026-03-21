# todo-infra-sentry-study-001.md — Étude Sentry Free Tier + OpenTelemetry

**Module** : Infra / Observabilité
**Priorité** : HAUTE
**Assigné** : Architect agent
**Dépendances** : aucune

---

## Contexte

On a déjà OpenTelemetry configuré via .NET Aspire ServiceDefaults (traces, metrics, logs).
On veut envoyer tout ça dans **Sentry (free tier)** pour avoir un dashboard de prod.

## Questions à investiguer

1. **Sentry supporte-t-il l'ingestion OTLP ?**
   - Sentry a annoncé le support OpenTelemetry — vérifier l'état actuel
   - Si oui : on peut juste ajouter un OTLP exporter vers Sentry sans SDK Sentry
   - Si non/partiel : faut-il le SDK Sentry.AspNetCore + Sentry.OpenTelemetry ?

2. **Sentry Free Tier — limites**
   - Combien d'events/mois, traces, transactions ?
   - Est-ce suffisant pour une app multi-tenant avec ~10 cliniques ?
   - Quelle rétention des données ?

3. **Architecture recommandée**
   - Option A : OTLP natif → Sentry (si supporté)
   - Option B : SDK Sentry + bridge OpenTelemetry (`Sentry.OpenTelemetry`)
   - Option C : Serilog Sink Sentry (`Sentry.Serilog`) + SDK Sentry pour traces
   - Option D : Mix — Serilog sink pour logs, OTLP pour traces/metrics

4. **Serilog Sink**
   - On utilise déjà Serilog — y a-t-il un sink `Sentry.Serilog` ?
   - Comment configurer les niveaux (Warning+ vers Sentry, Debug+ en local) ?
   - Breadcrumbs automatiques ?

5. **Impact sur le code existant**
   - ServiceDefaults/Extensions.cs — où brancher ?
   - Vetolib.Api/Program.cs — UseSentry() ?
   - Faut-il modifier les modules ou c'est centralisé ?

## Livrable attendu

Un fichier `docs/sentry-setup-study.md` avec :
- Recommandation architecturale (une seule option, argumentée)
- Packages NuGet nécessaires
- Configuration appsettings.json
- Schéma d'intégration avec le code existant
- Limites du free tier documentées
- Tâche dev prête à exécuter

## Critères de complétion

- [ ] Étude publiée dans `docs/sentry-setup-study.md`
- [ ] Tâche dev `todo-infra-sentry-impl-001.md` créée avec les étapes précises
- [ ] Aucune modification de code (étude seulement)

# todo-front-ai-triage-001.md — Frontend AI Triage dans formulaire RDV

**Module** : Frontend
**Dependances** : done-back-ai-triage-001
**Priorite** : HAUTE (Phase 2)
[MSW: oui]
[Branchement ulterieur: wire-ai-triage]
**Skills a lire** : `shadcn-nextjs`, `msw-mock-api`, `playwright-e2e`

---

## Objectif

Ajouter un champ symptomes dans le formulaire de prise de RDV et afficher la suggestion de triage AI.

## Implementation

1. **Champ symptomes** : textarea dans le formulaire de prise de RDV
2. **Bouton "Analyze symptoms"** : appelle POST /api/ai/triage
3. **Panel suggestion** : severite (badge couleur), duree estimee, specialite recommandee, reasoning
4. **Disclaimer** : toujours affiche sous la suggestion, non dismissable
5. **Actions vet** : boutons "Accept" (pre-remplit duree/type) et "Override" (choisir manuellement)
6. **Graceful degradation** : si LLM indisponible, le formulaire fonctionne normalement sans triage
7. **MSW handler** : POST /api/ai/triage → 200 avec suggestion mockee
8. **i18n EN + AR** (disclaimer traduit)
9. **data-testid** sur tous les elements
10. **Playwright E2E** : triage affiche, accept pre-remplit, LLM down → formulaire fonctionne

## Critere

```
[] Champ symptomes + bouton analyze
[] Panel suggestion avec severite, duree, specialite
[] Disclaimer toujours visible
[] Accept / Override
[] Graceful degradation si AI down
[] MSW handler
[] i18n EN + AR
[] Playwright tests
[] data-testid
[] Renommer en done
```

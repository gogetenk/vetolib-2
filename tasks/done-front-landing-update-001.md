# todo-front-landing-update-001.md — Mise à jour landing page avec nouvelles features

**Module** : Frontend (landing)
**Priorité** : MOYENNE
**Dépendances** : aucune
**Skills** : shadcn-nextjs
**[MSW: non]** — pas besoin de MSW, c'est du contenu statique

## Objectif

Mettre à jour la landing page pour refléter les nouvelles fonctionnalités développées depuis la dernière version. La landing doit vendre Vetolib à l'international (pas uniquement UAE).

## Nouvelles features à mettre en avant

1. **Dossier médical partagé** (innovation clé)
   - "Un passeport santé pour chaque animal"
   - Historique médical accessible dans n'importe quel cabinet
   - Consentement du propriétaire, données sécurisées

2. **IA vétérinaire intégrée**
   - Triage intelligent des symptômes
   - Vérification des interactions médicamenteuses
   - Prédiction no-show pour optimiser l'agenda

3. **Messagerie clinique**
   - Communication propriétaire ↔ cabinet
   - Classification AI des messages
   - Portail propriétaire avec magic link

4. **Gestion de stock**
   - Suivi des médicaments et consommables
   - Alertes de stock bas et expiration
   - Lien automatique prescriptions ↔ stock

5. **Multi-langue** (EN + AR, extensible)

## Composants à créer/modifier

- Ajouter une **FeaturesSection** entre le hero et le pricing
  - Cards pour chaque feature avec icône, titre, description
  - Responsive, RTL-compatible
- Mettre à jour le **hero** : tagline orientée innovation ("The smart veterinary platform")
- Mettre à jour les **i18n** (messages/en.json et messages/ar.json)
- Ajouter les **data-testid** sur tous les éléments

## Ton marketing
- Professionnel mais accessible
- Orienté bénéfice vétérinaire (pas jargon technique)
- International (ne pas mentionner UAE spécifiquement dans le hero)
- Mettre en avant le dossier partagé comme différenciateur

## Critère
```
[] FeaturesSection créée avec 4-5 cards
[] Hero mis à jour
[] i18n EN + AR
[] data-testid sur tous les éléments interactifs
[] npx tsc --noEmit passe
[] Renommer en done
```

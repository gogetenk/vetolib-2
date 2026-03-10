# todo-front-prescriptions-e2e-001 -- Playwright tests prescriptions enrichies

**Module** : Frontend (tests)
**Phase** : Toutes phases
**Dependances** : todo-front-prescriptions-catalog-001, todo-front-prescriptions-alerts-001, todo-front-prescriptions-stock-001, todo-front-prescriptions-weight-001
**[MSW: oui]**

## Objectif

Ecrire les tests Playwright couvrant le comportement utilisateur complet des prescriptions enrichies (catalogue, interactions, stock, poids).

## Skills a lire

1. `skills/playwright-e2e/SKILL.md`

## Scope detaille

### Tests a ecrire

Fichier : `src/frontend/e2e/integration/prescriptions.spec.ts`

#### Catalogue

- [ ] Test : autocomplete affiche les resultats du catalogue
- [ ] Test : selection d'un medicament du catalogue remplit le formulaire
- [ ] Test : toggle free-text mode permet la saisie libre
- [ ] Test : free-text mode affiche message "No interaction data available"

#### Interactions

- [ ] Test : alerte critique affichee en rouge pour species contraindication
- [ ] Test : alerte critique bloque le submit sans justification
- [ ] Test : override avec justification >= 10 char debloque le submit
- [ ] Test : alerte moderee affichee en amber, ne bloque pas
- [ ] Test : alerte info affichee en bleu sous le dosage
- [ ] Test : alternatives suggerees quand alerte presente
- [ ] Test : clic "Use this instead" change le medicament selectionne
- [ ] Test : dosage hors range affiche indicateur orange

#### Stock

- [ ] Test : disponibilite stock affichee apres selection medicament
- [ ] Test : badge "Low stock" affiche quand stock bas
- [ ] Test : badge "Out of stock" affiche quand stock a zero
- [ ] Test : alternatives en stock suggerees quand out of stock
- [ ] Test : checkbox "Dispense from stock" cochee par defaut si stock dispo
- [ ] Test : decochage "Dispense" sauvegarde sans mouvement stock
- [ ] Test : warning stock insuffisant avec option partial dispense

#### Poids patient

- [ ] Test : champ poids dans le formulaire patient
- [ ] Test : poids affiche dans le detail patient
- [ ] Test : "Not recorded" si poids null

#### RBAC

- [ ] Test : ASSISTANT ne voit pas le bouton "New Prescription"
- [ ] Test : RECEPTIONIST n'a pas acces a la creation de prescription

### Contraintes

- Tests contre MSW (pas de vrai backend)
- Utiliser data-testid pour tous les selecteurs
- Pas de page.route() -- MSW gere les mocks

## Criteres de completion

- [ ] Tous les tests ci-dessus ecrits et GREEN contre MSW
- [ ] Aucun selecteur CSS fragile -- uniquement data-testid
- [ ] Aucun page.route() -- MSW uniquement
- [ ] Tests organises par section (catalogue, interactions, stock, poids, RBAC)

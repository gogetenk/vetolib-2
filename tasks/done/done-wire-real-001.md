# todo-wire-real-001.md — Brancher le frontend sur le vrai backend

**Module** : Frontend (tous les modules)
**Dépendances** : done-back-migrations-001, done-back-contract-align-001
**Skills à lire** : `msw-mock-api`, `playwright-e2e`

---

## Contexte

Le frontend est à 100% sur MSW. La décision PO de garder MSW pour le MVP était temporaire (backend pas encore stable). Maintenant que le backend est complet et les contrats alignés, il faut brancher pour de vrai.

## Périmètre exact

### 1. Configurer NEXT_PUBLIC_API_URL

Dans `.env.local` :
```
NEXT_PUBLIC_API_URL=https://localhost:5001
```

Le `client.ts` utilise déjà cette variable. Quand elle est définie, les requêtes partent vers le vrai backend au lieu d'être interceptées par MSW.

### 2. Désactiver MSW en mode "wired"

Ne PAS supprimer MSW — le garder pour le développement offline. Mais le désactiver quand `NEXT_PUBLIC_API_URL` est défini :

```typescript
// src/mocks/browser.ts — modifier la condition de démarrage
if (!process.env.NEXT_PUBLIC_API_URL) {
  const { worker } = await import('./browser')
  await worker.start()
}
```

### 3. Adapter les tests Playwright

Créer deux modes de test :
- `npm run test:e2e` → tests contre MSW (rapide, offline, CI)
- `npm run test:e2e:integration` → tests contre le vrai backend (docker-compose up d'abord)

Pour le mode intégration :
- Configurer `playwright.config.ts` avec un `webServer` qui lance aussi le backend
- Ou utiliser un `globalSetup` qui vérifie que le backend est up
- Les données de test doivent correspondre au seed (admin@desertpaws.ae / Admin123!)
- Les IDs ne sont plus hardcodés (les patients/factures sont créés dynamiquement)

### 4. Corriger les tests qui dépendent de données MSW

Les tests actuels utilisent des IDs fixes MSW (`u-001`, `pat-0000-...`). En mode intégration :
- Les tests doivent créer leurs propres données (via l'API)
- Ou utiliser un `beforeAll` qui seed la DB de test
- Les assertions sur les counts doivent être flexibles

### 5. Vérifier chaque module end-to-end

Tester manuellement (puis automatiser) :
- [ ] Login avec admin@desertpaws.ae → JWT reçu
- [ ] Dashboard affiche les stats réelles
- [ ] Créer un patient → visible dans la liste
- [ ] Créer un RDV → visible dans l'agenda
- [ ] Transitions de statut RDV fonctionnent
- [ ] Créer une facture → calcul TVA correct
- [ ] Inviter un utilisateur → mot de passe temporaire retourné

### 6. Gérer CORS

Le backend doit accepter les requêtes depuis `localhost:3000` (Next.js dev server) :

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

## Critère de complétion

```
□ NEXT_PUBLIC_API_URL configuré et fonctionnel
□ MSW désactivé quand API_URL est défini
□ Login réel fonctionne (JWT du vrai backend)
□ CRUD patients fonctionne end-to-end
□ CRUD appointments fonctionne end-to-end
□ CRUD factures fonctionne end-to-end
□ Dashboard affiche des données réelles
□ CORS configuré
□ npm run test:e2e (MSW) toujours vert
□ Tests manuels intégration passent
□ Renommer en done-wire-real-001.md
```

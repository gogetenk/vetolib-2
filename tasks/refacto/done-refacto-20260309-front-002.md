# todo-refacto-20260309-front-002 -- Ajouter msw au package.json + loading/error boundaries
**Priorite** : critique
**Fichiers concernes** :
- `src/frontend/package.json`
- `src/frontend/src/app/(dashboard)/loading.tsx` (a creer)
- `src/frontend/src/app/(dashboard)/error.tsx` (a creer)
- `src/frontend/src/app/(auth)/loading.tsx` (a creer)

**Violation** : msw importe dans `src/mocks/` mais absent du package.json ; aucun loading.tsx / error.tsx App Router

**Correction attendue** :
1. Ajouter `msw` dans `devDependencies` de `package.json` : `"msw": "^2.x"`
2. Verifier que `npx msw init public/` a ete execute (mockServiceWorker.js doit exister dans public/)
3. Creer `src/app/(dashboard)/loading.tsx` avec un skeleton layout (Skeleton de shadcn)
4. Creer `src/app/(dashboard)/error.tsx` avec un composant 'use client' qui affiche l'erreur + bouton reset
5. Creer `src/app/(auth)/loading.tsx` avec un skeleton card centre

**Critere** :
- [ ] `npm ci && npm run build` passe sans erreur
- [ ] `grep "msw" package.json` retourne une ligne
- [ ] `ls src/app/(dashboard)/loading.tsx` existe
- [ ] `ls src/app/(dashboard)/error.tsx` existe

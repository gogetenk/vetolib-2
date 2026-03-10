# todo-refacto-20260309-front-004 -- Securiser le stockage JWT (httpOnly cookie)
**Priorite** : critique
**Fichiers concernes** :
- `src/frontend/src/lib/auth.ts`
- `src/frontend/src/lib/api/auth.ts`
- `src/frontend/src/lib/api/client.ts`
- `src/frontend/src/middleware.ts`
- `src/frontend/src/app/api/auth/login/route.ts` (a creer)
- `src/frontend/src/app/api/auth/logout/route.ts` (a creer)

**Violation** : JWT stocke en localStorage (vulnerable XSS) + cookie non-securise

**Correction attendue** :
1. Creer des route handlers Next.js qui proxient les appels auth vers le backend :
   - `POST /api/auth/login` -> appelle le backend, stocke le JWT dans un cookie httpOnly Secure SameSite=Strict
   - `POST /api/auth/logout` -> supprime le cookie
   - `POST /api/auth/refresh` -> appelle le backend, met a jour le cookie
2. Le middleware lit le cookie httpOnly nativement (deja le cas)
3. Le `client.ts` n'a plus besoin d'envoyer le header Authorization -- le cookie est envoye automatiquement
4. Supprimer tout acces a `localStorage` pour les tokens
5. Pour les infos utilisateur (role, nom, clinic), soit :
   - Les stocker dans un cookie non-httpOnly separe (safe car pas le token d'auth)
   - Soit appeler `GET /api/auth/me` au mount

Note : Cette tache est bloquante pour la mise en production mais **pas pour le MVP dev**. Le commentaire dans `lib/api/auth.ts` L27-29 documente cette decision. A planifier pour la release.

**Critere** :
- [ ] `grep -r "localStorage.*access_token\|localStorage.*refresh_token" src/frontend/src` retourne 0 resultats
- [ ] Le cookie access_token est httpOnly, Secure, SameSite=Strict
- [ ] Les tests Playwright continuent de passer

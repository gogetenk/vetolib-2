# todo-refacto-20260309-front-001 -- Centraliser JWT parsing et gestion tokens
**Priorite** : critique
**Fichiers concernes** :
- `src/frontend/src/lib/auth.ts`
- `src/frontend/src/lib/api/auth.ts`
- `src/frontend/src/lib/api/client.ts`
- `src/frontend/src/hooks/use-role.ts`
- `src/frontend/src/components/features/shell/Header.tsx`
- `src/frontend/src/components/features/shell/UserMenu.tsx`
- `src/frontend/src/app/(dashboard)/dashboard/page.tsx`
- `src/frontend/src/app/(dashboard)/settings/team/page.tsx`

**Violation** : DRY -- 6 implementations de JWT parsing, 3 fichiers de gestion tokens

**Correction attendue** :
1. Creer `lib/jwt.ts` avec un `parseJwt(token: string): JwtPayload` et une interface `JwtPayload { sub, name, role, clinicId, clinicName, exp, iat }`
2. Centraliser toute la gestion tokens dans `lib/auth.ts` (getAccessToken, setTokens, clearTokens, getSession)
3. Supprimer les fonctions dupliquees dans `lib/api/auth.ts` et `lib/api/client.ts`
4. Remplacer les 6 parseJwt manuels par `parseJwt()` de `lib/jwt.ts`
5. Extraire `PagedResult<T>` dans `lib/api/types.ts`
6. Unifier le type `Species` dans `lib/api/types.ts`

**Critere** :
- [ ] Un seul fichier `lib/jwt.ts` parse les tokens
- [ ] Un seul fichier `lib/auth.ts` gere le stockage
- [ ] `grep -r "atob.*split" src/frontend/src` ne retourne que `lib/jwt.ts`
- [ ] `grep -r "localStorage.*access_token" src/frontend/src` ne retourne que `lib/auth.ts`
- [ ] `PagedResult<T>` defini une seule fois dans `lib/api/types.ts`

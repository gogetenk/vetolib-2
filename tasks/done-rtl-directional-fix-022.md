# todo-rtl-directional-fix-022.md -- Fix 156 hardcoded directional CSS for RTL

**Module** : Frontend
**Priority** : Haute
**Dependencies** : aucune

## Scope (from accessibility-audit-20260330.md)
Replace ~156 hardcoded directional Tailwind classes with logical properties:
- ml-* → ms-*, mr-* → me-*
- pl-* → ps-*, pr-* → pe-*
- left-* → start-*, right-* → end-*
- rounded-l-* → rounded-s-*, rounded-r-* → rounded-e-*
- border-l-* → border-s-*, border-r-* → border-e-*

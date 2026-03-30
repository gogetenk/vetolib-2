# todo-perf-medium-fixes-021.md -- Fix 8 MEDIUM performance issues

**Module** : Multiple
**Priority** : Haute
**Dependencies** : aucune

## Scope (from performance-audit-20260329.md)
1. Dashboard 3 sequential queries → Task.WhenAll
2. Missing index on conversations.OwnerId
3. Missing index on invoices.Status+CreatedAt
4. Missing pagination on stock items
5. Missing pagination on owner conversations
6. ListConversationsHandler includes ALL messages for list view
7. E-reporting in-memory aggregation → DB-side GroupBy
8. Sequential migration of 7 DbContexts → parallel where safe

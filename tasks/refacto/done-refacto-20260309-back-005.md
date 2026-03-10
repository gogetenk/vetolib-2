# todo-refacto-20260309-back-005 — MailKitSmtpOptions est public dans le runtime Notifications
**Priorite** : critique
**Fichiers concernes** :
- `src/backend/Modules/Notifications/Vetolib.Notifications/Infrastructure/MailKitEmailSender.cs` (ligne 68)
**Violation** : La classe `MailKitSmtpOptions` est declaree `public` dans l'assembly runtime Vetolib.Notifications. Selon CLAUDE.md regle 2, la seule classe public du runtime doit etre `ModuleServiceRegistrar`. Toutes les autres doivent etre `internal`.
**Correction attendue** : Changer `public class MailKitSmtpOptions` en `internal class MailKitSmtpOptions`. Verifier que le binding de configuration dans NotificationsModuleServiceRegistrar fonctionne toujours (il est dans le meme assembly, donc l'acces internal est suffisant).
**Critere** : `grep -r "public class\|public record\|public struct\|public interface\|public enum\|public static class" Modules/Notifications/Vetolib.Notifications/ --include="*.cs"` ne retourne que `NotificationsModuleServiceRegistrar`.

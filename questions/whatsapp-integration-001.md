# Question — whatsapp-integration-001

**Module** : Messaging / Owner Portal
**Bloquant** : Non (étude prospective)

## Contexte
Aux UAE, WhatsApp est la norme pour la communication professionnelle et personnelle.
Vetolib doit considérer WhatsApp comme canal de communication propriétaire ↔ clinique.

## Questions à étudier (PO + Architecte)

### PO — Besoin utilisateur
- Quels services exposer sur WhatsApp ? (prise de RDV, rappels, résultats, messagerie ?)
- Chatbot WhatsApp ou juste notifications ?
- Le propriétaire peut-il prendre RDV via WhatsApp ?
- WhatsApp remplace-t-il le portail web ou le complète-t-il ?
- Quelle est la priorité par rapport au portail web ?

### Architecte — Faisabilité technique
- WhatsApp Business API (Meta) : coût, limitations, délais d'approbation
- Architecture : adapter le module Messaging existant ou nouveau canal ?
- Twilio / MessageBird comme abstraction multi-canal ?
- Impact sur le modèle de données (conversations WhatsApp vs in-app)
- Webhooks entrants WhatsApp → routing vers le bon module

## Livrable attendu
- doc PO : `docs/WHATSAPP-STUDY.md` avec recommandation user-centered
- Si go PO : analyse d'impact architecte + tasks

## Réponse PO

**WhatsApp est explicitement OUT OF SCOPE pour le MVP.** Confirmé dans `docs/MESSAGING-SPEC.md` section 6 (Out of Scope) : "WhatsApp Business API -- Meta certification cost + API integration complexity -- Target: V2 (6 months post-MVP)".

Réponses aux questions PO :
1. **Quels services exposer sur WhatsApp ?** : A étudier en V2. Probablement rappels de RDV + notifications de réponse (pas de messagerie bidirectionnelle au départ).
2. **Chatbot ou notifications ?** : Notifications uniquement en premier. Le chatbot est risqué (réglementation médicale UAE).
3. **Prise de RDV via WhatsApp ?** : Oui, c'est le use case le plus demandé aux UAE. A valider en V2.
4. **Remplace ou complète le portail web ?** : Complète. Le portail web reste la source de vérité. WhatsApp est un canal de notification/réponse rapide.
5. **Priorité vs portail web ?** : Le portail web est le MVP. WhatsApp est un accélérateur de V2.

L'architecture actuelle (canal abstrait dans le modèle de données) est prête pour l'ajout futur. Pas d'action immédiate requise.

-> Escalade humain requise : non

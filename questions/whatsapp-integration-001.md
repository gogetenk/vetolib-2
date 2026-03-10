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

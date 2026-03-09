# Skill: Domaine Vétérinaire — UAE / Dubai

## Contexte marché

Vetolib est une plateforme SaaS de gestion de clinique vétérinaire pour le marché UAE (Dubai, Abu Dhabi, Sharjah).
Les cliniques servent majoritairement des animaux de compagnie d'expatriés et une clientèle locale aisée.

---

## Espèces et races courantes à Dubai

### Chiens (Dog)
Races les plus fréquentes : Golden Retriever, Labrador, French Bulldog, German Shepherd,
Maltese, Shih Tzu, Poodle (Toy/Miniature), Husky, Chihuahua, Border Collie.

### Chats (Cat)
Races : Persian, Siamese, Maine Coon, British Shorthair, Ragdoll, Bengal, Scottish Fold,
Domestic Short/Long Hair.

### Lapins (Rabbit)
Races : Holland Lop, Dutch, Mini Rex, Lionhead.

### Exotiques (Exotic)
Oiseaux : Parrot (African Grey, Macaw, Cockatiel), Canary, Budgerigar.
Reptiles : Bearded Dragon, Gecko (Leopard, Crested), Tortoise.
Rongeurs : Hamster, Guinea Pig.

### Chevaux (Equine) — segment premium
Purebreds, Arabian Horse (très présent culturellement à Dubai).
Cliniques équines distinctes (différent workflow — pas dans MVP).

---

## Types d'actes vétérinaires

### Consultations (Consultation)
- `general_checkup` — Visite de routine, bilan de santé annuel
- `sick_visit` — Animal malade, symptômes aigus
- `emergency` — Urgence (slot dédié, tarif majoré)
- `follow_up` — Suivi post-traitement
- `second_opinion` — Second avis

### Vaccinations (Vaccination)
Protocoles Dubai Municipality obligatoires :
- `rabies` — Antirabique (obligatoire pour chiens et chats, renouvellement annuel)
- `dhpp` — Distemper/Hepatitis/Parvovirus/Parainfluenza (chiens)
- `fvrcp` — Rhinotracheitis/Calicivirus/Panleukopenia (chats)
- `fiv_felv` — FIV/FeLV (chats)
- `bordetella` — Toux du chenil (chiens)
- `leptospirosis` — Leptospirose (chiens, recommandé)

### Chirurgie (Surgery)
- `spay` — Stérilisation femelle
- `neuter` — Castration mâle
- `dental_cleaning` — Détartrage (nécessite anesthésie)
- `wound_repair` — Suture
- `foreign_body_removal` — Corps étranger
- `mass_removal` — Exérèse de masse

### Examens complémentaires (Diagnostics)
- `blood_panel` — Bilan sanguin complet
- `urinalysis` — Analyse urine
- `xray` — Radiographie (nécessite machine dédiée)
- `ultrasound` — Échographie
- `culture_sensitivity` — Culture bactérienne
- `biopsy` — Biopsie

### Grooming — souvent proposé par les cliniques
Pas dans le scope MVP mais à prévoir dans le modèle de données.

---

## Vocabulaire métier

### Entités clés

| Terme anglais | Définition dans Vetolib |
|---|---|
| `Patient` | L'animal. A un owner, un nom, une espèce, une race, une date de naissance. |
| `Owner` | Le propriétaire de l'animal. Peut avoir plusieurs patients. |
| `Vet` | Vétérinaire employé de la clinique. A des spécialités. |
| `Clinic` | La clinique (tenant). A des vets, des patients (via owners). |
| `Appointment` | RDV entre un Vet et un Patient, dans une Clinique, à une date/heure. |
| `MedicalRecord` | Compte-rendu d'une consultation. Lié à 1 Appointment. |
| `Prescription` | Ordonnance. Liée à 1 MedicalRecord. |
| `Medication` | Médicament prescrit dans une Prescription. |
| `Invoice` | Facture liée à 1 Appointment. |
| `Payment` | Paiement d'une Invoice. |

### Statuts d'appointment

```
Pending → Confirmed → Done
                ↓
            Cancelled
```

- `Pending` : créé, non confirmé
- `Confirmed` : confirmé par la clinique ou le client
- `InProgress` : le vet a commencé la consultation
- `Done` : consultation terminée, MedicalRecord créé
- `Cancelled` : annulé (par client ou clinique)
- `NoShow` : client absent sans annulation

### Durées standard (à utiliser comme défaut)

| Acte | Durée par défaut |
|---|---|
| General checkup | 30 min |
| Vaccination | 15 min |
| Emergency | 60 min |
| Spay/Neuter | 120 min |
| Dental cleaning | 90 min |
| Blood panel | 20 min |
| Follow-up | 15 min |

---

## Réglementation UAE pertinente

### Dubai Municipality — animaux
- Les chiens et chats doivent être **vaccinés antirabiques** et **microchipés** pour être enregistrés.
- L'enregistrement se fait à Dubai Municipality (DM). Vetolib peut afficher le statut d'enregistrement.
- Races interdites à Dubai : Pit Bull, Rottweiler, Doberman et assimilés (liste DM officielle).
- Les cliniques vétérinaires doivent être **agréées par DM** (Veterinary Services Department).

### Facturation — UAE VAT
- TVA UAE : **5% (VAT 5%)** sur tous les services et produits vétérinaires.
- Les cliniques doivent être enregistrées auprès du Federal Tax Authority (FTA) si chiffre d'affaires > AED 375,000/an.
- Les invoices doivent afficher : TRN (Tax Registration Number) de la clinique, montant HT, VAT 5%, montant TTC.
- Format de la facture conforme FTA :
  ```
  Subtotal (excl. VAT): AED 200.00
  VAT 5%:               AED  10.00
  Total (incl. VAT):    AED 210.00
  TRN: 1234567890123456
  ```

### Devises
- Devise principale : **AED (UAE Dirham)**. Toutes les transactions en AED.
- Certaines cliniques acceptent USD pour les clients étrangers (optionnel).
- `1 USD ≈ 3.67 AED` (taux fixe, pegging officiel).

---

## Spécialités vétérinaires

```
general      — Médecine générale
surgery      — Chirurgie générale
dentistry    — Dentisterie vétérinaire
dermatology  — Dermatologie
ophthalmology— Ophtalmologie
cardiology   — Cardiologie
oncology     — Oncologie
exotic       — Animaux exotiques (reptiles, oiseaux, rongeurs)
```

---

## Prénoms communs pour les animaux à Dubai

Données utiles pour les seeds/fixtures de tests :
Chiens : Max, Charlie, Buddy, Bella, Luna, Cooper, Milo, Rocky, Daisy, Bailey.
Chats : Oliver, Leo, Milo, Simba, Nala, Luna, Whiskers, Shadow, Cleo, Jasmine.

---

## Noms de cliniques fictives pour les tests

```
Dubai Paws Veterinary Clinic
Al Barsha Pet Care Center
Downtown Dubai Animal Hospital
Marina Vet Clinic
Jumeirah Pets & Veterinary
Arabian Ranches Animal Clinic
```

---

## Questions fréquentes des agents → escalader au PO si doute

```
- Doit-on intégrer avec le système DM d'enregistrement ? → PO
- L'export de rapport FTA est-il dans le MVP ? → PO
- Les ordonnances nécessitent-elles une signature digitale légale ? → PO
- La gestion des stocks de médicaments est-elle incluse ? → PO (probablement non MVP)
- Les races interdites doivent-elles bloquer la création de patient ? → PO
```

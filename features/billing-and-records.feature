# features/billing/invoices.feature

Feature: Facturation vétérinaire
  En tant que vétérinaire ou réceptionniste
  Je veux gérer la facturation des prestations
  Afin d'assurer le suivi financier de la clinique

  Background:
    Given une clinique "Happy Paws"
    And un animal "Max" dans la clinique
    And je suis authentifié en tant que VET

  Scenario: Créer une facture en brouillon
    When je crée une facture pour "Max" avec l'item "Consultation" à 200 AED
    Then la facture est créée avec le statut "DRAFT"
    And le numéro est au format "INV-2026-001"
    And la TVA de 5% est calculée automatiquement (10 AED)
    And le total est 210 AED

  Scenario: Ajouter plusieurs items à une facture
    Given une facture "DRAFT" pour "Max"
    When j'ajoute l'item "Vaccin" à 150 AED
    And j'ajoute l'item "Médicaments" à 80 AED
    Then la facture contient 2 items
    And le sous-total est 430 AED
    And la TVA totale est 21.5 AED
    And le total est 451.5 AED

  Scenario: Envoyer une facture
    Given une facture "DRAFT" pour "Max" avec au moins un item
    When je passe la facture à "SENT"
    Then le statut est "SENT"
    And la date d'échéance est fixée à 30 jours

  Scenario: Marquer une facture comme payée
    Given une facture "SENT" pour "Max"
    When je marque la facture comme "PAID"
    Then le statut est "PAID"

  Scenario: Impossible de modifier une facture payée
    Given une facture "PAID" pour "Max"
    When je tente d'ajouter un item à la facture
    Then le système refuse avec le code "INVOICE_IMMUTABLE"
    And le message est "Une facture payée ne peut plus être modifiée"

  Scenario: Numérotation séquentielle par clinique
    Given 3 factures existantes pour "Happy Paws"
    When je crée une nouvelle facture
    Then le numéro est "INV-2026-004"


---


# features/medical-records/records.feature

Feature: Dossier médical animal
  En tant que vétérinaire
  Je veux gérer les dossiers médicaux des animaux
  Afin de suivre leur historique de santé

  Background:
    Given une clinique "Happy Paws"
    And un propriétaire "John Smith" avec l'email "john@example.com"
    And un animal "Max" race "Labrador" appartenant à "John Smith"
    And je suis authentifié en tant que VET

  Scenario: Créer un dossier animal (nouveau patient)
    When je crée un animal "Luna" race "Persian Cat" pour le propriétaire "John Smith"
    Then l'animal est créé dans la clinique
    And son dossier médical est vide
    And le propriétaire "John Smith" est lié à "Luna"

  Scenario: Ajouter un examen au dossier
    When j'ajoute un examen pour "Max" avec le diagnostic "Otite bactérienne"
    And le traitement "Nettoyage oreilles + antibiotiques 7 jours"
    Then l'examen apparaît dans l'historique de "Max"
    And il est horodaté avec la date du jour
    And il porte mon nom comme vétérinaire

  Scenario: Consulter l'historique complet
    Given 3 examens dans le dossier de "Max"
    When je consulte le dossier de "Max"
    Then je vois les 3 examens dans l'ordre chronologique inverse
    And chaque examen affiche le diagnostic, traitement, date, et vétérinaire

  Scenario: Créer une ordonnance
    Given un examen existant pour "Max"
    When je crée une ordonnance avec le médicament "Amoxicilline 250mg" posologie "2x/jour pendant 7j"
    Then l'ordonnance est créée avec mon numéro de licence "UAE-VET-12345"
    And elle est liée à l'examen

  Scenario: RECEPTIONIST ne peut pas écrire dans un dossier
    Given je suis authentifié en tant que RECEPTIONIST
    When je tente d'ajouter un examen pour "Max"
    Then le système refuse avec le code "INSUFFICIENT_PERMISSIONS"

  Scenario: Un dossier n'est jamais supprimé
    Given un examen dans le dossier de "Max"
    When je tente de supprimer cet examen
    Then le système refuse avec le code "MEDICAL_RECORD_IMMUTABLE"
    And l'examen est toujours visible dans l'historique

  Scenario: Isolation tenant — ne pas voir les animaux d'une autre clinique
    Given un animal "Rocky" dans la clinique "Desert Vets"
    When je consulte la liste des animaux de "Happy Paws"
    Then "Rocky" n'apparaît pas dans la liste

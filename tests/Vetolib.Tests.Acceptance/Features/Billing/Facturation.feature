Feature: Facturation vétérinaire
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
    Then la facture contient 3 items
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

  Scenario: Télécharger le PDF d'une facture envoyée
    Given une facture "SENT" pour "Max" avec au moins un item
    When je télécharge le PDF de cette facture
    Then la réponse a le statut 200
    And le Content-Type est "application/pdf"
    And le contenu n'est pas vide

  Scenario: Impossible de télécharger le PDF d'une facture brouillon
    Given une facture "DRAFT" pour "Max"
    When je télécharge le PDF de cette facture
    Then la réponse a le statut 422

  Scenario: PDF inexistant retourne 404
    When je télécharge le PDF d'une facture avec un ID aléatoire inexistant
    Then la réponse a le statut 404

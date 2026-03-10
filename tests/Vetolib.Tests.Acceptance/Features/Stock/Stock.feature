Feature: Gestion de stock médicaments et vaccins
  Background:
    Given une clinique "Happy Paws"
    And je suis authentifié en tant que VET

  Scenario: Créer un item de stock médicament
    When je crée un item de stock "Amoxicilline" catégorie "Medication" quantité 100 unité "ml" seuil 20
    Then l'item de stock est créé avec le statut actif
    And la quantité est 100

  Scenario: Créer un item de stock vaccin
    When je crée un item de stock "Vaccin Rage" catégorie "Vaccine" quantité 50 unité "doses" seuil 10
    Then l'item de stock est créé avec le statut actif
    And la quantité est 50

  Scenario: Lister les items de stock
    Given un item de stock "Sérum physiologique" catégorie "Supply" quantité 200 unité "ml" seuil 30
    When je liste les items de stock
    Then la liste contient au moins 1 item

  Scenario: Enregistrer une entrée de stock
    Given un item de stock "Bandages" catégorie "Supply" quantité 50 unité "pièces" seuil 10
    When j'enregistre un mouvement de stock "IN" quantité 25 raison "Réapprovisionnement"
    Then la nouvelle quantité est 75

  Scenario: Enregistrer une sortie de stock
    Given un item de stock "Seringues" catégorie "Supply" quantité 100 unité "pièces" seuil 20
    When j'enregistre un mouvement de stock "OUT" quantité 10 raison "Utilisation consultation"
    Then la nouvelle quantité est 90

  Scenario: Alertes stock bas
    Given un item de stock "Vaccin Parvovirus" catégorie "Vaccine" quantité 5 unité "doses" seuil 10
    When je consulte les alertes de stock
    Then l'alerte contient "Vaccin Parvovirus" pour stock bas

  Scenario: Modification du seuil d'alerte
    Given un item de stock "Gants stériles" catégorie "Supply" quantité 200 unité "pièces" seuil 50
    When je modifie le seuil de l'item à 100
    Then le seuil est mis à jour à 100

  Scenario: Nom vide refusé
    When je tente de créer un item de stock avec un nom vide
    Then le système refuse avec le code "VALIDATION_ERROR"

  Scenario: Quantité négative refusée
    When je tente de créer un item de stock avec une quantité de -5
    Then le système refuse avec le code "VALIDATION_ERROR"

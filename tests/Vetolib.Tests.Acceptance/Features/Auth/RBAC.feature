@rbac
Feature: Matrice RBAC — controle d acces par role
  En tant que systeme
  Je veux appliquer la matrice RBAC du cabinet veterinaire UAE
  Pour que chaque role ne puisse effectuer que les actions autorisees

  Background:
    Given une clinique "Desert Paws"

  Scenario: Assistant ne peut pas creer de rendez-vous (403)
    Given je suis authentifie en tant que Assistant
    When je tente de creer un rendez-vous
    Then le systeme retourne 403

  Scenario: Receptionist ne peut pas ajouter de dossier medical (403)
    Given je suis authentifie en tant que Receptionist
    When je tente d'ajouter un dossier medical
    Then le systeme retourne 403

  Scenario: Vet peut creer un rendez-vous
    Given je suis authentifie en tant que Vet
    When je tente de creer un rendez-vous
    Then le systeme accepte la requete

  Scenario: Admin peut creer une facture
    Given je suis authentifie en tant que Admin
    When je tente de creer une facture
    Then le systeme accepte la requete

  Scenario: Assistant ne peut pas creer de facture (403)
    Given je suis authentifie en tant que Assistant
    When je tente de creer une facture
    Then le systeme retourne 403

  Scenario: Seul le Vet peut ajouter une prescription (VetOnly)
    Given je suis authentifie en tant que Admin
    When je tente d'ajouter une prescription a un dossier medical
    Then le systeme retourne 403

  Scenario: Vet peut ajouter une prescription
    Given je suis authentifie en tant que Vet
    When je tente d'ajouter une prescription a un dossier medical
    Then le systeme accepte la requete

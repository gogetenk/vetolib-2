# features/agenda/appointments.feature

Feature: Gestion des rendez-vous veterinaires
  En tant que receptionniste de clinique veterinaire
  Je veux gerer les rendez-vous
  Afin d'organiser l'agenda des veterinaires efficacement

  Background:
    Given une clinique "Happy Paws" avec les horaires 9h-18h
    And un veterinaire "Dr. Ahmed Al-Rashid" avec licence "UAE-VET-12345"
    And un animal "Max" de race "Labrador" appartenant a "John Smith"
    And je suis authentifie en tant que RECEPTIONIST

  Scenario: Creer un rendez-vous dans un creneau libre
    When je cree un rendez-vous pour "Max" avec "Dr. Ahmed" le "2026-04-01" a "10:00" pour 30 minutes
    Then le rendez-vous est cree avec le statut "SCHEDULED"
    And le rendez-vous apparait dans l'agenda de "Dr. Ahmed" a "10:00"

  Scenario: Lister les rendez-vous du jour
    Given un rendez-vous existant pour "Max" a "10:00"
    And un rendez-vous existant pour "Luna" a "14:00"
    When je consulte l'agenda du "2026-04-01"
    Then je vois 2 rendez-vous dans la liste

  Scenario: Refus si creneau deja pris
    Given un rendez-vous existant pour "Max" avec "Dr. Ahmed" a "10:00" pour 30 minutes
    When je tente de creer un rendez-vous pour "Luna" avec "Dr. Ahmed" a "10:00"
    Then le systeme refuse avec le code "APPOINTMENT_CONFLICT"
    And le message est "Ce creneau est deja pris pour ce veterinaire"
    And les prochains creneaux disponibles sont proposes

  Scenario: Refus si chevauchement partiel
    Given un rendez-vous existant de "10:00" a "10:30" avec "Dr. Ahmed"
    When je tente de creer un rendez-vous de "10:15" a "10:45" avec "Dr. Ahmed"
    Then le systeme refuse avec le code "APPOINTMENT_CONFLICT"

  Scenario: Refus hors horaires d'ouverture
    When je tente de creer un rendez-vous a "19:00"
    Then le systeme refuse avec le code "OUTSIDE_BUSINESS_HOURS"
    And le message indique les horaires "9h00 - 18h00"

  Scenario: Refus dans le passe
    When je tente de creer un rendez-vous pour le "2020-01-01" a "10:00"
    Then le systeme refuse avec le code "PAST_DATE_NOT_ALLOWED"

  Scenario: Enregistrer l'arrivee du patient (check-in)
    Given un rendez-vous existant pour "Max" avec "Dr. Ahmed" a "10:00" pour 30 minutes
    When je mets a jour le statut du dernier rendez-vous vers "CheckedIn"
    Then le statut du rendez-vous est "CheckedIn"

  Scenario: Demarrer la consultation
    Given un rendez-vous existant pour "Max" avec "Dr. Ahmed" a "10:00" pour 30 minutes
    And le statut du dernier rendez-vous a ete mis a jour vers "CheckedIn"
    When je mets a jour le statut du dernier rendez-vous vers "InProgress"
    Then le statut du rendez-vous est "InProgress"

  Scenario: Terminer la consultation
    Given un rendez-vous existant pour "Max" avec "Dr. Ahmed" a "10:00" pour 30 minutes
    And le statut du dernier rendez-vous a ete mis a jour vers "CheckedIn"
    And le statut du dernier rendez-vous a ete mis a jour vers "InProgress"
    When je mets a jour le statut du dernier rendez-vous vers "Completed"
    Then le statut du rendez-vous est "Completed"

  Scenario: Annuler un rendez-vous avec motif
    Given un rendez-vous existant pour "Max" avec "Dr. Ahmed" a "10:00" pour 30 minutes
    When j'annule le dernier rendez-vous avec le motif "Owner called to cancel"
    Then le statut du rendez-vous est "Cancelled"

  Scenario: Transition invalide refuse
    Given un rendez-vous existant pour "Max" avec "Dr. Ahmed" a "10:00" pour 30 minutes
    When je mets a jour le statut du dernier rendez-vous vers "Completed"
    Then le systeme refuse la transition avec le code "INVALID_TRANSITION"

  Scenario: Consulter les disponibilites d'un veterinaire
    Given un rendez-vous existant pour "Max" avec "Dr. Ahmed" a "10:00" pour 30 minutes
    When je consulte les disponibilites de "Dr. Ahmed" le "2026-04-01" pour 30 minutes
    Then je vois des creneaux disponibles et non disponibles
    And le creneau "10:00" est marque non disponible

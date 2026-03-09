Feature: Dossier médical animal
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

  Scenario: RECEPTIONIST ne peut pas écrire dans un dossier
    Given je suis authentifié en tant que RECEPTIONIST
    When je tente d'ajouter un examen pour "Max"
    Then le système refuse avec le code "INSUFFICIENT_PERMISSIONS"

  Scenario: Isolation tenant — ne pas voir les animaux d'une autre clinique
    Given un animal "Rocky" dans la clinique "Desert Vets"
    When je consulte la liste des animaux de "Happy Paws"
    Then "Rocky" n'apparaît pas dans la liste

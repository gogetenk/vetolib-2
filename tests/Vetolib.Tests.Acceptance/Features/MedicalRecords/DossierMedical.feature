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

  Scenario: Ajouter un examen au dossier
    When j'ajoute un examen pour "Max" avec le diagnostic "Otite bactérienne" et le traitement "Nettoyage oreilles + antibiotiques 7 jours"
    Then l'examen apparaît dans l'historique de "Max"
    And il est horodaté avec la date du jour
    And il porte le vétérinaire courant comme auteur

  Scenario: Consulter l'historique complet
    Given 3 examens dans le dossier de "Max"
    When je consulte le dossier de "Max"
    Then je vois 3 examens dans l'ordre chronologique inverse

  Scenario: Créer une ordonnance
    Given un examen existant pour "Max"
    When je crée une ordonnance avec le médicament "Amoxicilline 250mg" posologie "2x/jour pendant 7j"
    Then l'ordonnance est créée avec le numéro de licence "TEST-VET-001"
    And elle est liée à l'examen

  Scenario: Un dossier n'est jamais supprimé
    Given un examen dans le dossier de "Max"
    When je tente de supprimer cet examen
    Then le système refuse avec le code "MEDICAL_RECORD_IMMUTABLE"
    And l'examen est toujours visible dans l'historique

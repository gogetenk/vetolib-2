# features/auth/login.feature

Feature: Authentification et gestion des tokens JWT
  En tant qu'utilisateur de Vetolib
  Je veux m'authentifier avec mon email et mot de passe
  Afin d'accéder aux fonctionnalites de ma clinique de maniere securisee

  Background:
    Given une clinique "Happy Paws" avec l'identifiant "clinic-happy-paws"
    And un utilisateur existant avec les informations suivantes:
      | Email              | Password     | Role         | ClinicId          | VetLicenseNumber |
      | vet@happypaws.ae   | SecurePass1  | Vet          | clinic-happy-paws | UAE-VET-12345    |
    And un utilisateur admin existant:
      | Email                | Password     | Role  | ClinicId          |
      | admin@happypaws.ae   | AdminPass1   | Admin | clinic-happy-paws |

  # ─── Login — Happy Path ──────────────────────────────────

  Scenario: Connexion reussie retourne un JWT et un refresh token
    When je me connecte avec l'email "vet@happypaws.ae" et le mot de passe "SecurePass1"
    Then je recois un access token JWT valide
    And je recois un refresh token
    And la reponse contient les informations utilisateur:
      | Email            | Role | ClinicId          | VetLicenseNumber |
      | vet@happypaws.ae | Vet  | clinic-happy-paws | UAE-VET-12345    |
    And le JWT contient le claim "clinic_id" avec la valeur "clinic-happy-paws"

  Scenario: Le access token expire apres 15 minutes
    When je me connecte avec l'email "vet@happypaws.ae" et le mot de passe "SecurePass1"
    Then le access token a une duree de validite de 15 minutes

  # ─── Refresh Token — Happy Path ──────────────────────────

  Scenario: Rafraichir le token retourne une nouvelle paire et invalide l'ancien refresh token
    Given je suis connecte en tant que "vet@happypaws.ae"
    And je possede un refresh token valide
    When j'appelle POST /api/v1/auth/refresh avec mon refresh token
    Then je recois un nouveau access token JWT valide
    And je recois un nouveau refresh token different de l'ancien
    And l'ancien refresh token est invalide

  Scenario: Le nouveau refresh token expire apres 7 jours
    Given je suis connecte en tant que "vet@happypaws.ae"
    And je possede un refresh token valide
    When j'appelle POST /api/v1/auth/refresh avec mon refresh token
    Then le nouveau refresh token a une duree de validite de 7 jours

  # ─── Logout — Happy Path ─────────────────────────────────

  Scenario: Deconnexion invalide le refresh token
    Given je suis connecte en tant que "vet@happypaws.ae"
    And je possede un refresh token valide
    When j'appelle POST /api/v1/auth/logout
    Then la deconnexion est confirmee
    And le refresh token est invalide
    And une tentative de refresh avec cet ancien token echoue

  # ─── GET /me — Happy Path ────────────────────────────────

  Scenario: Recuperer le profil de l'utilisateur connecte
    Given je suis connecte en tant que "vet@happypaws.ae"
    When j'appelle GET /api/v1/auth/me
    Then je recois les informations de mon profil:
      | Email            | Role | ClinicId          | VetLicenseNumber |
      | vet@happypaws.ae | Vet  | clinic-happy-paws | UAE-VET-12345    |

  # ─── Login — Erreurs ─────────────────────────────────────

  Scenario: Mot de passe incorrect retourne une erreur
    When je me connecte avec l'email "vet@happypaws.ae" et le mot de passe "MauvaisPass1"
    Then le systeme refuse avec le code "INVALID_CREDENTIALS"
    And le message est "Email ou mot de passe incorrect"

  Scenario: Email inexistant retourne une erreur
    When je me connecte avec l'email "inconnu@happypaws.ae" et le mot de passe "SecurePass1"
    Then le systeme refuse avec le code "INVALID_CREDENTIALS"
    And le message est "Email ou mot de passe incorrect"

  # ─── Verrouillage de compte ──────────────────────────────

  Scenario: 5 tentatives echouees verrouillent le compte pour 15 minutes
    When je me connecte 5 fois avec l'email "vet@happypaws.ae" et un mot de passe incorrect
    Then le systeme refuse avec le code "ACCOUNT_LOCKED"
    And le message indique que le compte est verrouille pour 15 minutes

  Scenario: Connexion refusee pendant la periode de verrouillage meme avec le bon mot de passe
    Given le compte "vet@happypaws.ae" est verrouille suite a 5 tentatives echouees
    When je me connecte avec l'email "vet@happypaws.ae" et le mot de passe "SecurePass1"
    Then le systeme refuse avec le code "ACCOUNT_LOCKED"
    And le message indique que le compte est verrouille

  Scenario: Connexion reussie apres expiration de la periode de verrouillage
    Given le compte "vet@happypaws.ae" a ete verrouille il y a 16 minutes
    When je me connecte avec l'email "vet@happypaws.ae" et le mot de passe "SecurePass1"
    Then je recois un access token JWT valide
    And le compteur de tentatives echouees est reinitialise

  # ─── Refresh Token — Erreurs ─────────────────────────────

  Scenario: Refresh avec un token revoque echoue
    Given je suis connecte en tant que "vet@happypaws.ae"
    And mon refresh token a ete revoque par un precedent refresh
    When j'appelle POST /api/v1/auth/refresh avec le refresh token revoque
    Then le systeme refuse avec le code "INVALID_REFRESH_TOKEN"

  Scenario: Refresh avec un token expire echoue
    Given je suis connecte en tant que "vet@happypaws.ae"
    And mon refresh token a expire depuis plus de 7 jours
    When j'appelle POST /api/v1/auth/refresh avec le refresh token expire
    Then le systeme refuse avec le code "INVALID_REFRESH_TOKEN"

  # ─── GET /me — Erreurs ───────────────────────────────────

  Scenario: Acces a /me sans token retourne 401
    When j'appelle GET /api/v1/auth/me sans token d'authentification
    Then le systeme retourne le code HTTP 401

  # ─── Multi-tenancy ───────────────────────────────────────

  Scenario: Un utilisateur de la clinique A ne peut pas voir les donnees de la clinique B
    Given une clinique "Desert Vet" avec l'identifiant "clinic-desert-vet"
    And un utilisateur existant avec les informations suivantes:
      | Email                | Password     | Role | ClinicId           |
      | recep@desertvet.ae   | SecurePass1  | Receptionist | clinic-desert-vet |
    When je me connecte avec l'email "recep@desertvet.ae" et le mot de passe "SecurePass1"
    Then le JWT contient le claim "clinic_id" avec la valeur "clinic-desert-vet"
    And les requetes de cet utilisateur ne retournent que les donnees de "clinic-desert-vet"

  # ─── Creation utilisateur (support minimal) ──────────────

  Scenario: Un admin peut creer un utilisateur
    Given je suis connecte en tant que "admin@happypaws.ae"
    When je cree un utilisateur avec les informations suivantes:
      | Email                  | Password     | Role         | VetLicenseNumber |
      | newvet@happypaws.ae    | NewVetPass1  | Vet          | UAE-VET-99999    |
    Then l'utilisateur est cree avec succes
    And l'utilisateur cree appartient a la clinique "clinic-happy-paws"

  Scenario: Un non-admin ne peut pas creer un utilisateur
    Given je suis connecte en tant que "vet@happypaws.ae"
    When je tente de creer un utilisateur avec les informations suivantes:
      | Email                  | Password     | Role         |
      | autre@happypaws.ae     | OtherPass1   | Receptionist |
    Then le systeme refuse avec le code "FORBIDDEN"

  Scenario: Le role Vet exige un numero de licence veterinaire
    Given je suis connecte en tant que "admin@happypaws.ae"
    When je tente de creer un utilisateur avec les informations suivantes:
      | Email                  | Password     | Role | VetLicenseNumber |
      | novet@happypaws.ae     | NoVetPass1   | Vet  |                  |
    Then le systeme refuse avec le code "VET_LICENSE_REQUIRED"
    And le message est "Un numero de licence veterinaire est requis pour le role Vet"

  # ─── Validation mot de passe (creation utilisateur) ──────

  Scenario Outline: Mot de passe invalide lors de la creation d'un utilisateur
    Given je suis connecte en tant que "admin@happypaws.ae"
    When je tente de creer un utilisateur avec l'email "test@happypaws.ae" et le mot de passe "<password>"
    Then le systeme refuse avec le code "VALIDATION_ERROR"
    And le message contient "<raison>"

    Examples:
      | password | raison                                      |
      | Short1   | Le mot de passe doit contenir au moins 8 caracteres |
      | alllowercase1 | Le mot de passe doit contenir au moins une majuscule |
      | AllUpperCase  | Le mot de passe doit contenir au moins un chiffre    |

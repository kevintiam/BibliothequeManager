# BibliothequeManager

Application .NET MAUI pour la gestion d’une bibliothèque: authentification, gestion des livres/auteurs/catégories/adhérents, emprunts, réservations, et localisation FR/EN.

## Sommaire
- Aperçu
- Prérequis
- Installation & exécution
- Architecture & navigation
- Services & DI
- Base de données (EF Core)
- Localisation
- Tests
- Capture d’écran
- Contribution
- Licence

## Aperçu
Application multi-pages avec menu latéral (`FlyoutPage`) fournissant:
- Connexion et session (`SessionUser`)
- Accueil, Livres, Auteurs, Catégories, Adhérents
- Emprunts et Réservations
- Multilingue FR/EN

[PLACEHOLDER: Capture d’écran de l’accueil]

## Prérequis
- .NET 9 SDK
- Visual Studio 2026 avec workloads .NET MAUI
- SQL Server (ou adapter la chaîne de connexion)

## Installation & exécution
1. Restaurer et compiler:
   - `dotnet restore`
   - `dotnet build`
2. Lancer:
   - Visual Studio: F5
   - Ligne de commande: `dotlet run` depuis le projet MAUI

[PLACEHOLDER: Capture d’écran de la configuration de démarrage]

## Architecture & navigation
- `App` initialise la `MainPage`.
# BibliothequeManager

Application .NET MAUI pour la gestion d’une bibliothèque: authentification, gestion des livres/auteurs/catégories/adhérents, emprunts, réservations, et localisation FR/EN.

## Sommaire
- Aperçu
- Prérequis
- Installation & exécution
- Architecture & navigation
- Services & DI
- Base de données (EF Core)
- Localisation
- Tests
- Capture d’écran
- Contribution
- Licence

## Aperçu
Application multi-pages avec menu latéral (`FlyoutPage`) fournissant:
- Connexion et session (`SessionUser`)
- Accueil, Livres, Auteurs, Catégories, Adhérents
- Emprunts et Réservations
- Multilingue FR/EN

[PLACEHOLDER: Capture d’écran de l’accueil]

## Prérequis
- .NET 9 SDK
- Visual Studio 2026 avec workloads .NET MAUI
- SQL Server (ou adapter la chaîne de connexion)

## Installation & exécution
1. Restaurer et compiler:
   - `dotnet restore`
   - `dotnet build`
2. Lancer:
   - Visual Studio: F5
   - Ligne de commande: `dotlet run` depuis le projet MAUI

[PLACEHOLDER: Capture d’écran de la configuration de démarrage]

## Architecture & navigation
- `App` initialise la `MainPage`.
- `HomePage` (Flyout) enveloppe chaque `Detail` dans un `NavigationPage` pour activer `Navigation.PushAsync(...)`.
- Pages principales:
  - `Accueil`, `Books`, `Authors`, `CategoriePage`, `GestionAdherent`
  - Actions: `EmpruntPage`, `Retour`, `Reserver`, `GestionEmprunts`, `GestionReservations`

[PLACEHOLDER: Capture d’écran du menu latéral]

## Services & DI
- Enregistrés dans `MauiProgram`:
  - `SessionUser` (Singleton)
  - Pages action (Transient)
  - `BibliothequeContext` (EF Core)
- Exemple d’accès:
  - `var services = MauiProgram.CreateMauiApp().Services;`
  - `var session = services.GetRequiredService<SessionUser>();`

[PLACEHOLDER: Capture d’écran de l’injection de dépendances]

## Base de données (EF Core)
- Contexte: `BibliothequeContext` (SQL Server).
- Migration & seed d’un administrateur dans `App.OnStart` et `SeedData`.
- Chaine de connexion: `MauiProgram.cs`.

[PLACEHOLDER: Capture d’écran de la DB/diagramme]

## Localisation
- FR par défaut, bascule vers EN via `HomePage.OnSwitchLanguageClicked`.
- Mise à jour des cultures:
  - `CultureInfo.DefaultThreadCurrentCulture`
  - `CultureInfo.DefaultThreadCurrentUICulture`
  - `Thread.CurrentThread.CurrentCulture`
  - `Thread.CurrentThread.CurrentUICulture`
- Après changement, recharger la page courante pour appliquer les ressources.

[PLACEHOLDER: Capture d’écran du changement de langue]

## Tests
- Créer un projet de tests (xUnit):
  - `dotnet new xunit -n BibliothequeManager.Tests -o Tests/BibliothequeManager.Tests`
  - `dotnet add Tests/BibliothequeManager.Tests package Microsoft.EntityFrameworkCore.InMemory`
  - `dotnet add Tests/BibliothequeManager.Tests package xunit.runner.visualstudio`
  - `dotnet add Tests/BibliothequeManager.Tests reference BibliothequeManager/BibliothequeManager.csproj`
- Exécuter:
  - `dotnet test`
  - `dotnet test --filter "FullyQualifiedName~AdherentServiceTests"`

[PLACEHOLDER: Capture d’écran de l’Explorateur de tests]

## Flux fonctionnels clés
- Connexion:
  - Vérification des identifiants, création de session, redirection vers `HomePage`.
- Emprunt:
  - Sélection d’un livre, vérification d’adhérent, choix d’un exemplaire disponible, création d’emprunt, mise à jour disponibilité.
- Réservation:
  - Validation, conversion en emprunt avec mise à jour de l’exemplaire.

[PLACEHOLDER: Capture d’écran d’emprunt]
[PLACEHOLDER: Capture d’écran de réservation]

## Bonnes pratiques intégrées
- Navigation avec `NavigationPage` pour conserver la pile.
- `SessionUser` injecté pour éviter les `NullReferenceException`.
- `Include` EF pour charger les relations (Livres, Exemplaires, Auteurs, etc.).
- Validation d’entrées et feedback utilisateur via popups.

[PLACEHOLDER: Capture d’écran de popups]

## Contribution
- Fork, branche de feature, PR.
- Respecter le style C# et conventions du projet.
- Exécuter les tests avant PR.

[PLACEHOLDER: Capture d’écran du workflow Git/PR]
# BibliothequeManager
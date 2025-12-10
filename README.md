# BibliothequeManager

Application .NET MAUI pour la gestion dâ€™une bibliothÃ¨que: authentification, gestion des livres/auteurs/catÃ©gories/adhÃ©rents, emprunts, rÃ©servations, et localisation FR/EN.

## Sommaire
- AperÃ§u
- PrÃ©requis
- Installation & exÃ©cution
- Architecture & navigation
- Services & DI
- Base de donnÃ©es (EF Core)
- Localisation
- Tests
- Capture dâ€™Ã©cran
- Contribution
- Licence

## AperÃ§u
Application multi-pages avec menu latÃ©ral (`FlyoutPage`) fournissant:
- Connexion et session (`SessionUser`)
- Accueil, Livres, Auteurs, CatÃ©gories, AdhÃ©rents
- Emprunts et RÃ©servations
- Multilingue FR/EN

<img width="1915" height="1134" alt="image" src="https://github.com/user-attachments/assets/f93e4627-8a6b-4cb8-abd3-1394f0ff9168" />
[PLACEHOLDER: Capture dâ€™Ã©cran de lâ€™accueil]

## PrÃ©requis
- .NET 9 SDK
- Visual Studio 2026 avec workloads .NET MAUI
- SQL Server (ou adapter la chaÃ®ne de connexion)

## Installation & exÃ©cution
1. Restaurer et compiler:
   - `dotnet restore`
   - `dotnet build`
2. Lancer:
   - Visual Studio: F5
   - Ligne de commande: `dotlet run` depuis le projet MAUI

## Architecture & navigation
- `App` initialise la `MainPage`.
# BibliothequeManager

Application .NET MAUI pour la gestion dâ€™une bibliothÃ¨que: authentification, gestion des livres/auteurs/catÃ©gories/adhÃ©rents, emprunts, rÃ©servations, et localisation FR/EN.

## Sommaire
- AperÃ§u
- PrÃ©requis
- Installation & exÃ©cution
- Architecture & navigation
- Services & DI
- Base de donnÃ©es (EF Core)
- Localisation
- Tests
- Capture dâ€™Ã©cran
- Contribution
- Licence

## AperÃ§u
Application multi-pages avec menu latÃ©ral (`FlyoutPage`) fournissant:
- Connexion et session (`SessionUser`)
- Accueil, Livres, Auteurs, CatÃ©gories, AdhÃ©rents
- Emprunts et RÃ©servations
- Multilingue FR/EN

<img width="1915" height="1134" alt="image" src="https://github.com/user-attachments/assets/06b730fb-90c5-4c96-a331-b72923e1d88b" />


## PrÃ©requis
- .NET 9 SDK
- Visual Studio 2026 avec workloads .NET MAUI
- SQL Server (ou adapter la chaÃ®ne de connexion)

## Installation & exÃ©cution
1. Restaurer et compiler:
   - `dotnet restore`
   - `dotnet build`
2. Lancer:
   - Visual Studio: F5
   - Ligne de commande: `dotlet run` depuis le projet MAUI


## Architecture & navigation
- `App` initialise la `MainPage`.
- `HomePage` (Flyout) enveloppe chaque `Detail` dans un `NavigationPage` pour activer `Navigation.PushAsync(...)`.
- Pages principales:
  - `Accueil`, `Books`, `Authors`, `CategoriePage`, `GestionAdherent`
  - Actions: `EmpruntPage`, `Retour`, `Reserver`, `GestionEmprunts`, `GestionReservations`

<img width="418" height="1030" alt="image" src="https://github.com/user-attachments/assets/aace0f2a-85c4-42d7-b464-511c3f50fbe4" />

<img width="1919" height="1033" alt="image" src="https://github.com/user-attachments/assets/a0d4bcc7-93c2-4b9e-8847-18cb4f29a7d6" />



## Services & DI
- EnregistrÃ©s dans `MauiProgram`:
  - `SessionUser` (Singleton)
  - Pages action (Transient)
  - `BibliothequeContext` (EF Core)
- Exemple dâ€™accÃ¨s:
  - `var services = MauiProgram.CreateMauiApp().Services;`
  - `var session = services.GetRequiredService<SessionUser>();`


## Base de donnÃ©es (EF Core)
- Contexte: `BibliothequeContext` (SQL Server).
- Migration & seed dâ€™un administrateur dans `App.OnStart` et `SeedData`.
- Chaine de connexion: `MauiProgram.cs`.

<img width="1606" height="825" alt="image" src="https://github.com/user-attachments/assets/961d4384-7262-46eb-aee5-298b8b3f369b" />


## Localisation
- FR par dÃ©faut, bascule vers EN via `HomePage.OnSwitchLanguageClicked`.
- Mise Ã  jour des cultures:
  - `CultureInfo.DefaultThreadCurrentCulture`
  - `CultureInfo.DefaultThreadCurrentUICulture`
  - `Thread.CurrentThread.CurrentCulture`
  - `Thread.CurrentThread.CurrentUICulture`
- AprÃ¨s changement, recharger la page courante pour appliquer les ressources.

<img width="1906" height="1023" alt="image" src="https://github.com/user-attachments/assets/2fda0eb3-1f85-4985-bcb6-a1319c887849" />


## Tests
- CrÃ©er un projet de tests (xUnit):
  - `dotnet new xunit -n BibliothequeManager.Tests -o Tests/BibliothequeManager.Tests`
  - `dotnet add Tests/BibliothequeManager.Tests package Microsoft.EntityFrameworkCore.InMemory`
  - `dotnet add Tests/BibliothequeManager.Tests package xunit.runner.visualstudio`
  - `dotnet add Tests/BibliothequeManager.Tests reference BibliothequeManager/BibliothequeManager.csproj`
- ExÃ©cuter:
  - `dotnet test`
  - `dotnet test --filter "FullyQualifiedName~AdherentServiceTests"`
## Flux fonctionnels clÃ©s
- Connexion:
  - VÃ©rification des identifiants, crÃ©ation de session, redirection vers `HomePage`.
- Emprunt:
  - SÃ©lection dâ€™un livre, vÃ©rification dâ€™adhÃ©rent, choix dâ€™un exemplaire disponible, crÃ©ation dâ€™emprunt, mise Ã  jour disponibilitÃ©.
- RÃ©servation:
  - Validation, conversion en emprunt avec mise Ã  jour de lâ€™exemplaire.

<img width="1917" height="1022" alt="image" src="https://github.com/user-attachments/assets/fdec7e2f-6306-44bb-8f2b-a97a6b4623b8" />

<img width="1911" height="994" alt="image" src="https://github.com/user-attachments/assets/c3efabf5-1fa9-494b-a442-d047659538d0" />


<img width="1919" height="998" alt="image" src="https://github.com/user-attachments/assets/13b826ce-c75f-4965-8db2-b4988248ec78" />



## Bonnes pratiques intÃ©grÃ©es
- Navigation avec `NavigationPage` pour conserver la pile.
- `SessionUser` injectÃ© pour Ã©viter les `NullReferenceException`.
- `Include` EF pour charger les relations (Livres, Exemplaires, Auteurs, etc.).
- Validation dâ€™entrÃ©es et feedback utilisateur via popups.

<img width="1019" height="650" alt="image" src="https://github.com/user-attachments/assets/9eddffe1-860a-46b5-a2df-4e64ad549048" />


## Contribution
- Fork, branche de feature, PR.
- Respecter le style C# et conventions du projet.
- ExÃ©cuter les tests avant PR.

# BibliothequeManager

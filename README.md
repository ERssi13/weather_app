# Weather App - Sujet 2

Application meteo WinForms en C# pour un projet Bachelor 2eme annee.

## Fonctionnalites implementees

- Recherche de ville (bouton `Rechercher` ou touche `Entree`)
- Appel API meteo et geocoding (Open-Meteo)
- Affichage des previsions sur 5 jours avec:
  - temperature min/max
  - couverture nuageuse
  - humidite
  - vitesse du vent
  - icone meteo (texte)
  - description
- Gestion des favoris:
  - ajout / suppression
  - affichage via liste des favoris
  - double-clic sur un favori pour charger sa meteo
- Sauvegarde des favoris dans un fichier JSON
- Bonus: mode sombre activable

## Choix techniques

- UI: WinForms (.NET 8)
- API meteo: Open-Meteo (pas de cle API necessaire)
- Persistance favoris: JSON dans `%LocalAppData%/WeatherAppWinForms/favorites.json`

## Architecture

- `WeatherApp.WinForms/MainForm.cs`: interface et logique UI
- `WeatherApp.WinForms/Services/WeatherApiService.cs`: appels API et mapping
- `WeatherApp.WinForms/Services/FavoritesService.cs`: lecture/ecriture JSON
- `WeatherApp.WinForms/Services/WeatherCodeMapper.cs`: mapping code meteo -> icone/description
- `WeatherApp.WinForms/Models/*`: modeles metier

## Lancer le projet

Depuis le dossier `weather_app`:

```powershell
dotnet restore
dotnet build WeatherApp.sln
dotnet run --project .\WeatherApp.WinForms\WeatherApp.WinForms.csproj
```

## Plan de push en plusieurs etapes

Tu peux pousser de facon realiste comme suit:

1. `chore: init solution WinForms`
	- creation `WeatherApp.sln`
	- creation `WeatherApp.WinForms`

2. `feat: add weather domain models and services`
	- modeles (`FavoriteCity`, `WeatherDayForecast`, `WeatherResult`)
	- `WeatherApiService` + `WeatherCodeMapper`
	- `FavoritesService`

3. `feat: implement main WinForms UI`
	- `MainForm` complet
	- recherche + affichage grille
	- mode sombre

4. `feat: add favorites workflow and json persistence`
	- ajout/suppression favori
	- affichage favoris
	- sauvegarde/restauration JSON

5. `docs: update readme with run steps and architecture`

Ce decoupage te permet de faire des petits push credibles.
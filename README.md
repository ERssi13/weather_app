Weather App - Sujet 2

Application meteo WinForms en C# pour un projet Bachelor 2eme annee.

Le style visuel de l'application est inspire de Samsung Weather sur Android.
Design en cartes arrondies.
Palette de bleus et contraste clair.
Hierarchie visuelle marquee entre la ville, la temperature principale et les details.
Interface moderne mais simple a realiser avec WinForms.

Le rendu a ete simplifie pour eviter un style trop ancien ou surcharge.
Les cartes restent legeres, avec de grands espaces, des couleurs douces et des icones dessinees dans le code.

L'application utilise Open-Meteo.
Cette API est gratuite pour un projet etudiant.
Aucune cle API n'est obligatoire pour demarrer.
Le geocoding et les previsions sont disponibles facilement.
Les donnees recuperees couvrent la temperature, l'humidite, les nuages, le vent et une description.

Le choix d'Open-Meteo permet de faire une application complete sans gestion de quota ni gestion de secret API.

Fonctionnalites implementees.
Recherche de ville avec bouton Rechercher ou touche Entree.
Appel API meteo et geocoding avec Open-Meteo.
Affichage des previsions sur 5 jours avec temperature min et max, couverture nuageuse, humidite, vitesse du vent, icone meteo et description.
Gestion des favoris avec ajout, suppression, affichage de la liste et double-clic pour recharger la meteo.
Sauvegarde des favoris dans un fichier JSON.
Mode sombre activable.

Choix techniques.
UI WinForms sous .NET 8.
API meteo Open-Meteo sans cle API.
Persistance des favoris dans %LocalAppData%/WeatherAppWinForms/favorites.json.

Architecture actuelle.
WeatherApp.WinForms/MainForm.cs contient l'interface et la logique UI.
WeatherApp.WinForms/Services/WeatherApiService.cs contient les appels API et le traitement des donnees.
WeatherApp.WinForms/Services/FavoritesService.cs contient la lecture et l'ecriture JSON.
WeatherApp.WinForms/Services/WeatherCodeMapper.cs contient le mapping des codes meteo vers icone et description.
WeatherApp.WinForms/Models contient les modeles metier.

Lancer le projet.
Depuis le dossier weather_app, executer dotnet restore, puis dotnet build WeatherApp.sln, puis dotnet run --project .\WeatherApp.WinForms\WeatherApp.WinForms.csproj.

Plan de push en plusieurs etapes.
Premier push pour l'initialisation de la solution WinForms.
Deuxieme push pour les modeles metier et les services meteo.
Troisieme push pour l'interface WinForms principale.
Quatrieme push pour les favoris et la persistance JSON.
Cinquieme push pour la documentation.

Ce decoupage permet de faire des petits push credibles et progressifs.
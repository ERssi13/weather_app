using System.Text.Json;
using WeatherApp.WinForms.Models;

namespace WeatherApp.WinForms.Services;

public sealed class FavoritesService
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true,
	};

	private readonly string _storageFilePath;

	public FavoritesService()
	{
		string appDataFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"WeatherAppWinForms");

		Directory.CreateDirectory(appDataFolder);
		_storageFilePath = Path.Combine(appDataFolder, "favorites.json");
	}

	public async Task<IReadOnlyList<FavoriteCity>> LoadAsync()
	{
		if (!File.Exists(_storageFilePath))
		{
			return Array.Empty<FavoriteCity>();
		}

		await using FileStream stream = File.OpenRead(_storageFilePath);
		List<FavoriteCity>? favorites = await JsonSerializer.DeserializeAsync<List<FavoriteCity>>(stream, JsonOptions);
		return favorites ?? new List<FavoriteCity>();
	}

	public async Task SaveAsync(IReadOnlyList<FavoriteCity> favorites)
	{
		await using FileStream stream = File.Create(_storageFilePath);
		await JsonSerializer.SerializeAsync(stream, favorites, JsonOptions);
	}
}

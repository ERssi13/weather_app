namespace WeatherApp.WinForms.Models;

public sealed class FavoriteCity
{
	public required string Name { get; init; }
	public string? Country { get; init; }
	public double Latitude { get; init; }
	public double Longitude { get; init; }

	public string DisplayName => string.IsNullOrWhiteSpace(Country)
		? Name
		: $"{Name} ({Country})";
}

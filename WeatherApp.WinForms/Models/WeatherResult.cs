namespace WeatherApp.WinForms.Models;

public sealed class WeatherResult
{
	public required FavoriteCity City { get; init; }
	public required IReadOnlyList<WeatherDayForecast> Forecasts { get; init; }
}

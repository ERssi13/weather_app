namespace WeatherApp.WinForms.Models;

public sealed class WeatherDayForecast
{
	public required DateTime Date { get; init; }
	public double TempMinC { get; init; }
	public double TempMaxC { get; init; }
	public int CloudCoverPercent { get; init; }
	public int HumidityPercent { get; init; }
	public double WindSpeedKmh { get; init; }
	public int WeatherCode { get; init; }
	public required string Icon { get; init; }
	public required string Description { get; init; }
}

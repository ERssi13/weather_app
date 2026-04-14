using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherApp.WinForms.Models;

namespace WeatherApp.WinForms.Services;

public sealed class WeatherApiService
{
	private const string GeoEndpoint = "https://geocoding-api.open-meteo.com/v1/search";
	private const string ForecastEndpoint = "https://api.open-meteo.com/v1/forecast";

	private readonly HttpClient _httpClient;

	public WeatherApiService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<WeatherResult> GetForecastByCityNameAsync(string cityName, int days = 5)
	{
		FavoriteCity city = await SearchCityAsync(cityName);
		IReadOnlyList<WeatherDayForecast> forecasts = await GetForecastAsync(city.Latitude, city.Longitude, days);

		return new WeatherResult
		{
			City = city,
			Forecasts = forecasts,
		};
	}

	public async Task<IReadOnlyList<WeatherDayForecast>> GetForecastAsync(double latitude, double longitude, int days = 5)
	{
		string latitudeInvariant = latitude.ToString(CultureInfo.InvariantCulture);
		string longitudeInvariant = longitude.ToString(CultureInfo.InvariantCulture);
		string url =
			$"{ForecastEndpoint}?latitude={latitudeInvariant}&longitude={longitudeInvariant}" +
			$"&daily=weather_code,temperature_2m_max,temperature_2m_min,wind_speed_10m_max" +
			$"&hourly=relative_humidity_2m,cloud_cover" +
			$"&timezone=auto&forecast_days={days}";

		ForecastResponseDto response = await GetAndDeserializeAsync<ForecastResponseDto>(url);

		if (response.Daily is null || response.Hourly is null)
		{
			return Array.Empty<WeatherDayForecast>();
		}

		Dictionary<DateOnly, List<int>> humidityByDate = GroupHourlyByDate(response.Hourly.Time, response.Hourly.RelativeHumidity2M);
		Dictionary<DateOnly, List<int>> cloudByDate = GroupHourlyByDate(response.Hourly.Time, response.Hourly.CloudCover);

		List<WeatherDayForecast> forecasts = new();

		for (int i = 0; i < response.Daily.Time.Count; i++)
		{
			DateOnly dateOnly = DateOnly.Parse(response.Daily.Time[i], CultureInfo.InvariantCulture);
			DateTime date = dateOnly.ToDateTime(TimeOnly.MinValue);
			int weatherCode = response.Daily.WeatherCode[i];
			(string icon, string description) = WeatherCodeMapper.Map(weatherCode);

			humidityByDate.TryGetValue(dateOnly, out List<int>? humidityValues);
			cloudByDate.TryGetValue(dateOnly, out List<int>? cloudValues);

			forecasts.Add(new WeatherDayForecast
			{
				Date = date,
				TempMinC = response.Daily.Temperature2MMin[i],
				TempMaxC = response.Daily.Temperature2MMax[i],
				WindSpeedKmh = response.Daily.WindSpeed10MMax[i],
				CloudCoverPercent = AverageOrZero(cloudValues),
				HumidityPercent = AverageOrZero(humidityValues),
				WeatherCode = weatherCode,
				Icon = icon,
				Description = description,
			});
		}

		return forecasts;
	}

	public async Task<FavoriteCity> SearchCityAsync(string cityName)
	{
		string encodedCityName = Uri.EscapeDataString(cityName.Trim());
		string url = $"{GeoEndpoint}?name={encodedCityName}&count=1&language=fr&format=json";

		GeoSearchResponseDto response = await GetAndDeserializeAsync<GeoSearchResponseDto>(url);

		if (response.Results is null || response.Results.Count == 0)
		{
			throw new InvalidOperationException("Ville introuvable.");
		}

		GeoResultDto city = response.Results[0];

		return new FavoriteCity
		{
			Name = city.Name,
			Country = city.Country,
			Latitude = city.Latitude,
			Longitude = city.Longitude,
		};
	}

	private async Task<T> GetAndDeserializeAsync<T>(string url)
	{
		using HttpResponseMessage response = await _httpClient.GetAsync(url);
		response.EnsureSuccessStatusCode();

		await using Stream stream = await response.Content.ReadAsStreamAsync();
		T? body = await JsonSerializer.DeserializeAsync<T>(stream);

		if (body is null)
		{
			throw new InvalidOperationException("Reponse API invalide.");
		}

		return body;
	}

	private static Dictionary<DateOnly, List<int>> GroupHourlyByDate(List<string>? times, List<int>? values)
	{
		Dictionary<DateOnly, List<int>> result = new();

		if (times is null || values is null)
		{
			return result;
		}

		int max = Math.Min(times.Count, values.Count);

		for (int i = 0; i < max; i++)
		{
			DateTime timestamp = DateTime.Parse(times[i], CultureInfo.InvariantCulture);
			DateOnly date = DateOnly.FromDateTime(timestamp);

			if (!result.TryGetValue(date, out List<int>? list))
			{
				list = new List<int>();
				result[date] = list;
			}

			list.Add(values[i]);
		}

		return result;
	}

	private static int AverageOrZero(List<int>? values)
	{
		if (values is null || values.Count == 0)
		{
			return 0;
		}

		return (int)Math.Round(values.Average());
	}

	private sealed class GeoSearchResponseDto
	{
		[JsonPropertyName("results")]
		public List<GeoResultDto>? Results { get; init; }
	}

	private sealed class GeoResultDto
	{
		[JsonPropertyName("name")]
		public required string Name { get; init; }

		[JsonPropertyName("country")]
		public string? Country { get; init; }

		[JsonPropertyName("latitude")]
		public double Latitude { get; init; }

		[JsonPropertyName("longitude")]
		public double Longitude { get; init; }
	}

	private sealed class ForecastResponseDto
	{
		[JsonPropertyName("daily")]
		public DailyDto? Daily { get; init; }

		[JsonPropertyName("hourly")]
		public HourlyDto? Hourly { get; init; }
	}

	private sealed class DailyDto
	{
		[JsonPropertyName("time")]
		public required List<string> Time { get; init; }

		[JsonPropertyName("weather_code")]
		public required List<int> WeatherCode { get; init; }

		[JsonPropertyName("temperature_2m_max")]
		public required List<double> Temperature2MMax { get; init; }

		[JsonPropertyName("temperature_2m_min")]
		public required List<double> Temperature2MMin { get; init; }

		[JsonPropertyName("wind_speed_10m_max")]
		public required List<double> WindSpeed10MMax { get; init; }
	}

	private sealed class HourlyDto
	{
		[JsonPropertyName("time")]
		public List<string>? Time { get; init; }

		[JsonPropertyName("relative_humidity_2m")]
		public List<int>? RelativeHumidity2M { get; init; }

		[JsonPropertyName("cloud_cover")]
		public List<int>? CloudCover { get; init; }
	}
}

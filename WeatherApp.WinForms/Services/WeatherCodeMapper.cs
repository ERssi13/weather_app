namespace WeatherApp.WinForms.Services;

public static class WeatherCodeMapper
{
	public static (string Icon, string Description) Map(int code)
	{
		return code switch
		{
			0 => ("SUN", "Ciel degage"),
			1 => ("SUN-CLOUD", "Principalement degage"),
			2 => ("CLOUD-SUN", "Partiellement nuageux"),
			3 => ("CLOUD", "Couvert"),
			45 or 48 => ("FOG", "Brouillard"),
			51 or 53 or 55 => ("DRIZZLE", "Bruine"),
			56 or 57 => ("FREEZE-DRIZZLE", "Bruine verglacante"),
			61 or 63 or 65 => ("RAIN", "Pluie"),
			66 or 67 => ("FREEZE-RAIN", "Pluie verglacante"),
			71 or 73 or 75 or 77 => ("SNOW", "Neige"),
			80 or 81 or 82 => ("SHOWERS", "Averses"),
			85 or 86 => ("SNOW-SHOWERS", "Averses de neige"),
			95 => ("STORM", "Orage"),
			96 or 99 => ("THUNDER-HAIL", "Orage avec grele"),
			_ => ("N/A", "Inconnu"),
		};
	}
}

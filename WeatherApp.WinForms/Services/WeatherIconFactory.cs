using System.Collections.Concurrent;
using System.Drawing.Drawing2D;

namespace WeatherApp.WinForms.Services;

public static class WeatherIconFactory
{
	private static readonly ConcurrentDictionary<string, Image> Cache = new(StringComparer.Ordinal);

	public static Image Create(int weatherCode, bool darkMode, int size)
	{
		string key = $"{weatherCode}:{darkMode}:{size}";
		return Cache.GetOrAdd(key, _ => Render(weatherCode, darkMode, size));
	}

	private static Image Render(int weatherCode, bool darkMode, int size)
	{
		Bitmap bitmap = new(size, size);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(Color.Transparent);

		Color primary = darkMode ? Color.FromArgb(235, 241, 255) : Color.FromArgb(40, 91, 219);
		Color secondary = darkMode ? Color.FromArgb(170, 190, 230) : Color.FromArgb(95, 125, 185);
		Color accent = darkMode ? Color.FromArgb(118, 169, 255) : Color.FromArgb(73, 135, 255);
		Color warm = Color.FromArgb(255, 196, 77);
		Color sunCore = Color.FromArgb(255, 211, 74);
		Color sunGlow = Color.FromArgb(255, 176, 64);
		Color cloud = darkMode ? Color.FromArgb(210, 220, 235) : Color.FromArgb(122, 140, 168);

		Rectangle bounds = new(0, 0, size, size);
		float scale = size / 64f;

		switch (GetKind(weatherCode))
		{
			case WeatherIconKind.Sunny:
				DrawSun(graphics, bounds, sunCore, sunGlow, scale);
				break;
			case WeatherIconKind.PartlyCloudy:
				DrawPartlyCloudy(graphics, bounds, sunCore, sunGlow, cloud, scale);
				break;
			case WeatherIconKind.Cloudy:
				DrawCloudy(graphics, bounds, primary, cloud, scale);
				break;
			case WeatherIconKind.Rain:
				DrawRain(graphics, bounds, primary, cloud, accent, scale);
				break;
			case WeatherIconKind.Snow:
				DrawSnow(graphics, bounds, primary, cloud, accent, scale);
				break;
			case WeatherIconKind.Storm:
				DrawStorm(graphics, bounds, primary, cloud, warm, scale);
				break;
			case WeatherIconKind.Fog:
				DrawFog(graphics, bounds, secondary, cloud, scale);
				break;
			case WeatherIconKind.Drizzle:
				DrawDrizzle(graphics, bounds, primary, cloud, accent, scale);
				break;
			default:
				DrawCloudy(graphics, bounds, primary, cloud, scale);
				break;
		}

		return bitmap;
	}

	private static WeatherIconKind GetKind(int code)
	{
		return code switch
		{
			0 => WeatherIconKind.Sunny,
			1 or 2 => WeatherIconKind.PartlyCloudy,
			3 => WeatherIconKind.Cloudy,
			45 or 48 => WeatherIconKind.Fog,
			51 or 53 or 55 => WeatherIconKind.Drizzle,
			56 or 57 => WeatherIconKind.Drizzle,
			61 or 63 or 65 => WeatherIconKind.Rain,
			66 or 67 => WeatherIconKind.Rain,
			71 or 73 or 75 or 77 => WeatherIconKind.Snow,
			80 or 81 or 82 => WeatherIconKind.Rain,
			85 or 86 => WeatherIconKind.Snow,
			95 or 96 or 99 => WeatherIconKind.Storm,
			_ => WeatherIconKind.Cloudy,
		};
	}

	private static void DrawSun(Graphics graphics, Rectangle bounds, Color sunColor, Color glowColor, float scale)
	{
		PointF center = new(bounds.Width * 0.52f, bounds.Height * 0.48f);
		float radius = 11f * scale;

		using Pen glowPen = new(glowColor, 2.6f * scale);
		using SolidBrush sunBrush = new(sunColor);
		using Pen sunPen = new(sunColor, 3f * scale);
		graphics.DrawEllipse(glowPen, center.X - radius * 1.15f, center.Y - radius * 1.15f, radius * 2.3f, radius * 2.3f);
		graphics.FillEllipse(sunBrush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
		for (int i = 0; i < 8; i++)
		{
			float angle = i * 45f;
			float radians = angle * MathF.PI / 180f;
			PointF start = new(center.X + MathF.Cos(radians) * (radius + 3f * scale), center.Y + MathF.Sin(radians) * (radius + 3f * scale));
			PointF end = new(center.X + MathF.Cos(radians) * (radius + 9f * scale), center.Y + MathF.Sin(radians) * (radius + 9f * scale));
			graphics.DrawLine(sunPen, start, end);
		}
	}

	private static void DrawPartlyCloudy(Graphics graphics, Rectangle bounds, Color sunColor, Color glowColor, Color cloudColor, float scale)
	{
		DrawSun(graphics, new Rectangle(0, 0, bounds.Width, bounds.Height), sunColor, glowColor, scale * 0.92f);
		using Brush cloudBrush = new SolidBrush(cloudColor);
		using Pen cloudPen = new(cloudColor, 1.5f * scale);
		DrawCloud(graphics, new RectangleF(bounds.Width * 0.18f, bounds.Height * 0.38f, bounds.Width * 0.64f, bounds.Height * 0.34f), cloudBrush, cloudPen);
	}

	private static void DrawCloudy(Graphics graphics, Rectangle bounds, Color sunColor, Color cloudColor, float scale)
	{
		using Brush cloudBrush = new SolidBrush(cloudColor);
		using Pen cloudPen = new(cloudColor, 1.5f * scale);
		DrawCloud(graphics, new RectangleF(bounds.Width * 0.12f, bounds.Height * 0.32f, bounds.Width * 0.76f, bounds.Height * 0.36f), cloudBrush, cloudPen);
	}

	private static void DrawRain(Graphics graphics, Rectangle bounds, Color sunColor, Color cloudColor, Color rainColor, float scale)
	{
		using Brush cloudBrush = new SolidBrush(cloudColor);
		using Pen cloudPen = new(cloudColor, 1.5f * scale);
		DrawCloud(graphics, new RectangleF(bounds.Width * 0.12f, bounds.Height * 0.27f, bounds.Width * 0.76f, bounds.Height * 0.36f), cloudBrush, cloudPen);

		using Pen rainPen = new(rainColor, 2.4f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		for (int i = 0; i < 3; i++)
		{
			float x = bounds.Width * (0.31f + i * 0.18f);
			graphics.DrawLine(rainPen, x, bounds.Height * 0.66f, x - 2f * scale, bounds.Height * 0.82f);
		}
	}

	private static void DrawDrizzle(Graphics graphics, Rectangle bounds, Color sunColor, Color cloudColor, Color rainColor, float scale)
	{
		using Brush cloudBrush = new SolidBrush(cloudColor);
		using Pen cloudPen = new(cloudColor, 1.5f * scale);
		DrawCloud(graphics, new RectangleF(bounds.Width * 0.12f, bounds.Height * 0.27f, bounds.Width * 0.76f, bounds.Height * 0.36f), cloudBrush, cloudPen);

		using Pen rainPen = new(rainColor, 1.7f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		for (int i = 0; i < 3; i++)
		{
			float x = bounds.Width * (0.32f + i * 0.17f);
			graphics.DrawLine(rainPen, x, bounds.Height * 0.67f, x - 1f * scale, bounds.Height * 0.78f);
		}
	}

	private static void DrawSnow(Graphics graphics, Rectangle bounds, Color sunColor, Color cloudColor, Color flakeColor, float scale)
	{
		using Brush cloudBrush = new SolidBrush(cloudColor);
		using Pen cloudPen = new(cloudColor, 1.5f * scale);
		DrawCloud(graphics, new RectangleF(bounds.Width * 0.12f, bounds.Height * 0.28f, bounds.Width * 0.76f, bounds.Height * 0.35f), cloudBrush, cloudPen);

		using Pen flakePen = new(flakeColor, 1.9f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		float baseY = bounds.Height * 0.72f;
		float[] xs = { bounds.Width * 0.30f, bounds.Width * 0.50f, bounds.Width * 0.70f };
		foreach (float x in xs)
		{
			graphics.DrawLine(flakePen, x - 3f * scale, baseY, x + 3f * scale, baseY);
			graphics.DrawLine(flakePen, x, baseY - 3f * scale, x, baseY + 3f * scale);
			graphics.DrawLine(flakePen, x - 2.2f * scale, baseY - 2.2f * scale, x + 2.2f * scale, baseY + 2.2f * scale);
			graphics.DrawLine(flakePen, x - 2.2f * scale, baseY + 2.2f * scale, x + 2.2f * scale, baseY - 2.2f * scale);
		}
	}

	private static void DrawStorm(Graphics graphics, Rectangle bounds, Color sunColor, Color cloudColor, Color boltColor, float scale)
	{
		using Brush cloudBrush = new SolidBrush(cloudColor);
		using Pen cloudPen = new(cloudColor, 1.5f * scale);
		DrawCloud(graphics, new RectangleF(bounds.Width * 0.12f, bounds.Height * 0.28f, bounds.Width * 0.76f, bounds.Height * 0.35f), cloudBrush, cloudPen);

		using Pen boltPen = new(boltColor, 3f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
		PointF[] points =
		{
			new(bounds.Width * 0.48f, bounds.Height * 0.58f),
			new(bounds.Width * 0.40f, bounds.Height * 0.78f),
			new(bounds.Width * 0.50f, bounds.Height * 0.78f),
			new(bounds.Width * 0.43f, bounds.Height * 0.92f)
		};
		graphics.DrawLines(boltPen, points);
	}

	private static void DrawFog(Graphics graphics, Rectangle bounds, Color lineColor, Color cloudColor, float scale)
	{
		using Brush cloudBrush = new SolidBrush(cloudColor);
		using Pen cloudPen = new(cloudColor, 1.5f * scale);
		DrawCloud(graphics, new RectangleF(bounds.Width * 0.15f, bounds.Height * 0.22f, bounds.Width * 0.70f, bounds.Height * 0.34f), cloudBrush, cloudPen);

		using Pen fogPen = new(lineColor, 2.1f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		float[] ys = { bounds.Height * 0.68f, bounds.Height * 0.76f, bounds.Height * 0.84f };
		foreach (float y in ys)
		{
			graphics.DrawLine(fogPen, bounds.Width * 0.20f, y, bounds.Width * 0.80f, y);
		}
	}

	private static void DrawCloud(Graphics graphics, RectangleF area, Brush fillBrush, Pen outlinePen)
	{
		float x = area.X;
		float y = area.Y;
		float w = area.Width;
		float h = area.Height;

		graphics.FillEllipse(fillBrush, x + w * 0.10f, y + h * 0.18f, w * 0.30f, h * 0.48f);
		graphics.FillEllipse(fillBrush, x + w * 0.30f, y + h * 0.05f, w * 0.34f, h * 0.58f);
		graphics.FillEllipse(fillBrush, x + w * 0.56f, y + h * 0.16f, w * 0.28f, h * 0.46f);
		graphics.FillRectangle(fillBrush, x + w * 0.18f, y + h * 0.30f, w * 0.60f, h * 0.30f);

		graphics.DrawEllipse(outlinePen, x + w * 0.10f, y + h * 0.18f, w * 0.30f, h * 0.48f);
		graphics.DrawEllipse(outlinePen, x + w * 0.30f, y + h * 0.05f, w * 0.34f, h * 0.58f);
		graphics.DrawEllipse(outlinePen, x + w * 0.56f, y + h * 0.16f, w * 0.28f, h * 0.46f);
		graphics.DrawRectangle(outlinePen, x + w * 0.18f, y + h * 0.30f, w * 0.60f, h * 0.30f);
	}

	private enum WeatherIconKind
	{
		Sunny,
		PartlyCloudy,
		Cloudy,
		Rain,
		Snow,
		Storm,
		Fog,
		Drizzle,
	}
}

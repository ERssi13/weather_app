using System.Globalization;
using System.Drawing.Drawing2D;
using WeatherApp.WinForms.Models;
using WeatherApp.WinForms.Services;

namespace WeatherApp.WinForms;

public sealed class MainForm : Form
{
	private readonly WeatherApiService _weatherService;
	private readonly FavoritesService _favoritesService;

	private readonly TextBox _cityTextBox;
	private readonly CardPanel _searchInputCard;
	private readonly ListBox _suggestionsListBox;
	private readonly Button _searchButton;
	private readonly Button _addFavoriteButton;
	private readonly Button _removeFavoriteButton;
	private readonly CheckBox _darkModeCheckBox;
	private readonly ListBox _favoritesListBox;
	private readonly Label _headerLabel;
	private readonly PictureBox _weatherIconPictureBox;
	private readonly Label _temperatureLabel;
	private readonly Label _conditionLabel;
	private readonly Label _forecastNoteLabel;
	private readonly Label _forecastSectionLabel;
	private readonly TableLayoutPanel _forecastCardsLayout;
	private readonly Label _statusLabel;
	private readonly Label _subtitleLabel;
	private readonly System.Windows.Forms.Timer _suggestionsTimer;

	private readonly List<FavoriteCity> _favorites = new();
	private readonly List<WeatherDayForecast> _currentForecasts = new();
	private readonly List<ForecastDayCard> _forecastCards = new();

	private CancellationTokenSource? _suggestionsCancellationSource;
	private bool _updatingSuggestions;
	private bool _darkModeEnabled;
	private FavoriteCity? _currentCity;
	private Color _backgroundTop;
	private Color _backgroundBottom;

	public MainForm()
	{
		Text = "Weather App";
		Width = 1160;
		Height = 760;
		MinimumSize = new Size(980, 640);
		StartPosition = FormStartPosition.CenterScreen;
		Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

		_weatherService = new WeatherApiService(new HttpClient());
		_favoritesService = new FavoritesService();
		_suggestionsTimer = new System.Windows.Forms.Timer { Interval = 260 };

		TableLayoutPanel root = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 1,
			RowCount = 2,
			Padding = new Padding(16),
			BackColor = Color.Transparent,
		};
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 188));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

		CardPanel topCard = new() { Dock = DockStyle.Fill, Radius = 24 };
		CardPanel favoritesCard = new() { Dock = DockStyle.Fill, Radius = 24 };
		CardPanel weatherCard = new() { Dock = DockStyle.Fill, Radius = 24 };

		TableLayoutPanel bottomLayout = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 1,
			BackColor = Color.Transparent,
		};
		bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
		bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		root.Controls.Add(topCard, 0, 0);
		root.Controls.Add(bottomLayout, 0, 1);
		bottomLayout.Controls.Add(favoritesCard, 0, 0);
		bottomLayout.Controls.Add(weatherCard, 1, 0);
		Controls.Add(root);

		TableLayoutPanel topLayout = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 1,
			RowCount = 4,
			Padding = new Padding(20, 18, 20, 16),
			BackColor = Color.Transparent,
		};
		topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
		topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
		topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
		topLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		topCard.Controls.Add(topLayout);

		Label appTitle = new()
		{
			Text = "Weather",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("Segoe UI Semibold", 18.5F, FontStyle.Bold, GraphicsUnit.Point),
		};

		_subtitleLabel = new Label
		{
			Text = string.Empty,
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("Segoe UI", 9.3F, FontStyle.Regular, GraphicsUnit.Point),
		};

		TableLayoutPanel searchLayout = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 4,
			RowCount = 1,
			BackColor = Color.Transparent,
		};
		searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116));
		searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));
		searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142));

		_searchInputCard = new CardPanel
		{
			Dock = DockStyle.Fill,
			Radius = 14,
			Padding = new Padding(12, 6, 12, 6),
			Margin = new Padding(0, 0, 8, 0),
		};

		_cityTextBox = new TextBox
		{
			Dock = DockStyle.Fill,
			PlaceholderText = "Rechercher une ville",
			BorderStyle = BorderStyle.None,
			BackColor = Color.White,
		};
		_searchInputCard.Controls.Add(_cityTextBox);

		_searchButton = new Button
		{
			Text = "Rechercher",
			Dock = DockStyle.Fill,
			FlatStyle = FlatStyle.Flat,
		};

		_addFavoriteButton = new Button
		{
			Text = "Favori",
			Dock = DockStyle.Fill,
			FlatStyle = FlatStyle.Flat,
		};

		_darkModeCheckBox = new CheckBox
		{
			Text = "Mode sombre",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleCenter,
		};

		searchLayout.Controls.Add(_searchInputCard, 0, 0);
		searchLayout.Controls.Add(_searchButton, 1, 0);
		searchLayout.Controls.Add(_addFavoriteButton, 2, 0);
		searchLayout.Controls.Add(_darkModeCheckBox, 3, 0);

		_suggestionsListBox = new ListBox
		{
			Dock = DockStyle.Fill,
			Visible = false,
			IntegralHeight = false,
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
		};

		topLayout.Controls.Add(appTitle, 0, 0);
		topLayout.Controls.Add(_subtitleLabel, 0, 1);
		topLayout.Controls.Add(searchLayout, 0, 2);
		topLayout.Controls.Add(_suggestionsListBox, 0, 3);

		TableLayoutPanel favoritesLayout = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 1,
			RowCount = 3,
			Padding = new Padding(16),
			BackColor = Color.Transparent,
		};
		favoritesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
		favoritesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		favoritesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
		favoritesCard.Controls.Add(favoritesLayout);

		Label favoritesTitle = new()
		{
			Text = "Favoris",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point),
		};

		_favoritesListBox = new ListBox
		{
			Dock = DockStyle.Fill,
			DisplayMember = nameof(FavoriteCity.DisplayName),
			BorderStyle = BorderStyle.None,
			Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
		};

		_removeFavoriteButton = new Button
		{
			Text = "Supprimer",
			Dock = DockStyle.Left,
			Width = 110,
			FlatStyle = FlatStyle.Flat,
		};

		favoritesLayout.Controls.Add(favoritesTitle, 0, 0);
		favoritesLayout.Controls.Add(_favoritesListBox, 0, 1);
		favoritesLayout.Controls.Add(_removeFavoriteButton, 0, 2);

		TableLayoutPanel weatherLayout = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 1,
			RowCount = 3,
			Padding = new Padding(16),
			BackColor = Color.Transparent,
		};
		weatherLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 232));
		weatherLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		weatherLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
		weatherCard.Controls.Add(weatherLayout);

		TableLayoutPanel summaryLayout = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 1,
			RowCount = 3,
			BackColor = Color.Transparent,
		};
		summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
		summaryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
		summaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

		_headerLabel = new Label
		{
			Text = "Aucune ville selectionnee",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.BottomLeft,
			Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold, GraphicsUnit.Point),
		};

		TableLayoutPanel summaryBody = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 1,
			BackColor = Color.Transparent,
		};
		summaryBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124));
		summaryBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

		_weatherIconPictureBox = new PictureBox
		{
			Dock = DockStyle.Fill,
			SizeMode = PictureBoxSizeMode.CenterImage,
			BackColor = Color.Transparent,
		};

		TableLayoutPanel summaryText = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 1,
			RowCount = 2,
			BackColor = Color.Transparent,
		};
		summaryText.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
		summaryText.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

		_temperatureLabel = new Label
		{
			Text = "--°",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("Segoe UI Semibold", 48F, FontStyle.Bold, GraphicsUnit.Point),
		};

		_conditionLabel = new Label
		{
			Text = "Meteo indisponible",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.TopLeft,
			Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point),
		};

		_forecastNoteLabel = new Label
		{
			Text = "Lance une recherche pour afficher les details de la ville.",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.TopLeft,
			Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
		};

		summaryText.Controls.Add(_temperatureLabel, 0, 0);
		summaryText.Controls.Add(_conditionLabel, 0, 1);
		summaryBody.Controls.Add(_weatherIconPictureBox, 0, 0);
		summaryBody.Controls.Add(summaryText, 1, 0);
		summaryLayout.Controls.Add(_headerLabel, 0, 0);
		summaryLayout.Controls.Add(summaryBody, 0, 1);
		summaryLayout.Controls.Add(_forecastNoteLabel, 0, 2);

		TableLayoutPanel forecastSectionLayout = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 1,
			RowCount = 2,
			BackColor = Color.Transparent,
		};
		forecastSectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
		forecastSectionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

		_forecastSectionLabel = new Label
		{
			Text = "Tendance des prochains jours",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point),
		};

		_forecastCardsLayout = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(0, 4, 0, 0),
			BackColor = Color.Transparent,
		};

		forecastSectionLayout.Controls.Add(_forecastSectionLabel, 0, 0);
		forecastSectionLayout.Controls.Add(_forecastCardsLayout, 0, 1);

		_statusLabel = new Label
		{
			Text = "Pret",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
		};

		weatherLayout.Controls.Add(summaryLayout, 0, 0);
		weatherLayout.Controls.Add(forecastSectionLayout, 0, 1);
		weatherLayout.Controls.Add(_statusLabel, 0, 2);

		ConfigureForecastSection();
		WireEvents();
		ConfigureSearchSuggestions();
		ApplyTheme(false);

		Load += async (_, _) => await LoadFavoritesAsync();
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		using LinearGradientBrush brush = new(ClientRectangle, _backgroundTop, _backgroundBottom, 90f);
		e.Graphics.FillRectangle(brush, ClientRectangle);
	}

	private void ConfigureForecastSection()
	{
		_forecastCardsLayout.SuspendLayout();
		_forecastCardsLayout.Controls.Clear();
		_forecastCardsLayout.ColumnStyles.Clear();
		_forecastCardsLayout.RowStyles.Clear();
		_forecastCardsLayout.ColumnCount = 1;
		_forecastCardsLayout.RowCount = 1;
		_forecastCardsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		_forecastCardsLayout.ResumeLayout();
		RefreshForecastCards();
	}

	private void WireEvents()
	{
		_searchButton.Click += async (_, _) => await SearchAndDisplayWeatherAsync();
		_cityTextBox.TextChanged += async (_, _) => await QueueCitySuggestionsAsync();
		_cityTextBox.KeyDown += async (_, e) =>
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.SuppressKeyPress = true;
				if (_suggestionsListBox.Visible && _suggestionsListBox.SelectedItem is CitySuggestionItem selected)
				{
					ApplySuggestion(selected);
				}

				await SearchAndDisplayWeatherAsync();
			}
			else if (e.KeyCode == Keys.Down && _suggestionsListBox.Visible && _suggestionsListBox.Items.Count > 0)
			{
				e.SuppressKeyPress = true;
				_suggestionsListBox.SelectedIndex = Math.Min(_suggestionsListBox.SelectedIndex + 1, _suggestionsListBox.Items.Count - 1);
			}
			else if (e.KeyCode == Keys.Up && _suggestionsListBox.Visible && _suggestionsListBox.Items.Count > 0)
			{
				e.SuppressKeyPress = true;
				_suggestionsListBox.SelectedIndex = Math.Max(_suggestionsListBox.SelectedIndex - 1, 0);
			}
			else if (e.KeyCode == Keys.Escape)
			{
				HideSuggestions();
			}
		};

		_suggestionsListBox.MouseDoubleClick += async (_, _) =>
		{
			if (_suggestionsListBox.SelectedItem is CitySuggestionItem selected)
			{
				ApplySuggestion(selected);
				await SearchAndDisplayWeatherAsync();
			}
		};

		_addFavoriteButton.Click += async (_, _) => await AddCurrentCityToFavoritesAsync();
		_removeFavoriteButton.Click += async (_, _) => await RemoveSelectedFavoriteAsync();
		_favoritesListBox.DoubleClick += async (_, _) => await ShowSelectedFavoriteWeatherAsync();
		_darkModeCheckBox.CheckedChanged += (_, _) => ApplyTheme(_darkModeCheckBox.Checked);
	}

	private void ConfigureSearchSuggestions()
	{
		_suggestionsTimer.Tick += async (_, _) => await RefreshCitySuggestionsAsync();
	}

	private async Task QueueCitySuggestionsAsync()
	{
		if (_updatingSuggestions)
		{
			return;
		}

		_suggestionsTimer.Stop();
		_suggestionsTimer.Start();
		await Task.CompletedTask;
	}

	private async Task RefreshCitySuggestionsAsync()
	{
		_suggestionsTimer.Stop();

		string query = _cityTextBox.Text.Trim();
		if (query.Length < 2)
		{
			HideSuggestions();
			return;
		}

		_suggestionsCancellationSource?.Cancel();
		_suggestionsCancellationSource?.Dispose();
		_suggestionsCancellationSource = new CancellationTokenSource();
		CancellationToken token = _suggestionsCancellationSource.Token;

		try
		{
			IReadOnlyList<FavoriteCity> suggestions = await _weatherService.SearchCitiesAsync(query, 8);
			if (token.IsCancellationRequested || !string.Equals(query, _cityTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			_updatingSuggestions = true;
			try
			{
				_suggestionsListBox.BeginUpdate();
				_suggestionsListBox.Items.Clear();

				HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
				foreach (FavoriteCity city in suggestions)
				{
					if (string.IsNullOrWhiteSpace(city.Name))
					{
						continue;
					}

					string display = string.IsNullOrWhiteSpace(city.Country)
						? city.Name
						: $"{city.Name} ({city.Country})";

					if (seen.Add(display))
					{
						_suggestionsListBox.Items.Add(new CitySuggestionItem { Name = city.Name, Display = display });
					}
				}

				_suggestionsListBox.EndUpdate();
				ShowSuggestions(_suggestionsListBox.Items.Count > 0);
				if (_suggestionsListBox.Items.Count > 0)
				{
					_suggestionsListBox.SelectedIndex = 0;
				}
			}
			finally
			{
				_updatingSuggestions = false;
			}
		}
		catch
		{
			HideSuggestions();
		}
	}

	private void ApplySuggestion(CitySuggestionItem suggestion)
	{
		_updatingSuggestions = true;
		try
		{
			_cityTextBox.Text = suggestion.Name;
			_cityTextBox.SelectionStart = _cityTextBox.Text.Length;
			_cityTextBox.SelectionLength = 0;
			HideSuggestions();
		}
		finally
		{
			_updatingSuggestions = false;
		}
	}

	private void ShowSuggestions(bool visible)
	{
		_suggestionsListBox.Visible = visible;
	}

	private void HideSuggestions()
	{
		_updatingSuggestions = true;
		try
		{
			_suggestionsListBox.BeginUpdate();
			_suggestionsListBox.Items.Clear();
			_suggestionsListBox.EndUpdate();
			_suggestionsListBox.Visible = false;
		}
		finally
		{
			_updatingSuggestions = false;
		}
	}

	private async Task SearchAndDisplayWeatherAsync()
	{
		string cityName = _cityTextBox.Text.Trim();
		HideSuggestions();

		if (string.IsNullOrWhiteSpace(cityName))
		{
			SetStatus("Veuillez saisir une ville.", true);
			return;
		}

		await ShowWeatherAsync(async () => await _weatherService.GetForecastByCityNameAsync(cityName));
	}

	private async Task ShowSelectedFavoriteWeatherAsync()
	{
		if (_favoritesListBox.SelectedItem is not FavoriteCity city)
		{
			return;
		}

		await ShowWeatherAsync(async () =>
		{
			IReadOnlyList<WeatherDayForecast> forecasts = await _weatherService.GetForecastAsync(city.Latitude, city.Longitude);
			return new WeatherResult
			{
				City = city,
				Forecasts = forecasts,
			};
		});
	}

	private async Task ShowWeatherAsync(Func<Task<WeatherResult>> weatherLoader)
	{
		UseWaitCursor = true;
		SetStatus("Chargement de la meteo...", false);

		try
		{
			WeatherResult result = await weatherLoader();
			_currentCity = result.City;
			_currentForecasts.Clear();
			_currentForecasts.AddRange(result.Forecasts);
			_headerLabel.Text = result.City.DisplayName;
			UpdateSummary(result);
			RefreshForecastCards();
			SetStatus($"{result.Forecasts.Count} jours recuperes.", false);
		}
		catch (Exception ex)
		{
			SetStatus($"Erreur: {ex.Message}", true);
		}
		finally
		{
			UseWaitCursor = false;
		}
	}

	private void UpdateSummary(WeatherResult result)
	{
		if (result.Forecasts.Count == 0)
		{
			_temperatureLabel.Text = "--";
			_conditionLabel.Text = "Meteo indisponible";
			_forecastNoteLabel.Text = "Aucune prevision disponible pour cette ville.";
			_weatherIconPictureBox.Image = WeatherIconFactory.Create(0, _darkModeEnabled, 96);
			return;
		}

		WeatherDayForecast first = result.Forecasts[0];
		double average = Math.Round((first.TempMinC + first.TempMaxC) / 2, 1);
		_temperatureLabel.Text = $"{average:N1}°";
		_conditionLabel.Text = first.Description;
		_forecastNoteLabel.Text = $"Maximales : {Math.Round(first.TempMaxC)}°C. Minimales : {Math.Round(first.TempMinC)}°C. Humidite moyenne : {first.HumidityPercent}%.";
		_weatherIconPictureBox.Image = WeatherIconFactory.Create(first.WeatherCode, _darkModeEnabled, 96);
	}

	private void RefreshForecastCards()
	{
		_forecastCardsLayout.SuspendLayout();
		_forecastCardsLayout.Controls.Clear();
		_forecastCardsLayout.ColumnStyles.Clear();
		_forecastCardsLayout.RowStyles.Clear();

		int cardCount = Math.Max(_currentForecasts.Count, 1);
		_forecastCardsLayout.ColumnCount = cardCount;
		_forecastCardsLayout.RowCount = 1;
		_forecastCardsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

		for (int i = 0; i < cardCount; i++)
		{
			_forecastCardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cardCount));
		}

		if (_currentForecasts.Count == 0)
		{
			ForecastDayCard emptyCard = new();
			emptyCard.SetForecast(null, _darkModeEnabled);
			_forecastCardsLayout.Controls.Add(emptyCard, 0, 0);
			_forecastCardsLayout.ResumeLayout();
			return;
		}

		while (_forecastCards.Count < _currentForecasts.Count)
		{
			_forecastCards.Add(new ForecastDayCard());
		}

		for (int i = 0; i < _currentForecasts.Count; i++)
		{
			ForecastDayCard card = _forecastCards[i];
			card.SetForecast(_currentForecasts[i], _darkModeEnabled);
			_forecastCardsLayout.Controls.Add(card, i, 0);
		}

		_forecastCardsLayout.ResumeLayout();
	}

	private async Task LoadFavoritesAsync()
	{
		IReadOnlyList<FavoriteCity> loadedFavorites = await _favoritesService.LoadAsync();
		_favorites.Clear();
		_favorites.AddRange(loadedFavorites);
		RefreshFavoritesList();
	}

	private async Task AddCurrentCityToFavoritesAsync()
	{
		if (_currentCity is null)
		{
			SetStatus("Recherchez une ville avant de l'ajouter en favori.", true);
			return;
		}

		bool exists = _favorites.Any(c =>
			string.Equals(c.Name, _currentCity.Name, StringComparison.OrdinalIgnoreCase) &&
			Math.Abs(c.Latitude - _currentCity.Latitude) < 0.001 &&
			Math.Abs(c.Longitude - _currentCity.Longitude) < 0.001);

		if (exists)
		{
			SetStatus("Cette ville est deja dans les favoris.", true);
			return;
		}

		_favorites.Add(_currentCity);
		await _favoritesService.SaveAsync(_favorites);
		RefreshFavoritesList();
		SetStatus("Ville ajoutee aux favoris.", false);
	}

	private async Task RemoveSelectedFavoriteAsync()
	{
		if (_favoritesListBox.SelectedItem is not FavoriteCity selectedCity)
		{
			SetStatus("Selectionnez une ville favorite a supprimer.", true);
			return;
		}

		_favorites.RemoveAll(c =>
			string.Equals(c.Name, selectedCity.Name, StringComparison.OrdinalIgnoreCase) &&
			Math.Abs(c.Latitude - selectedCity.Latitude) < 0.001 &&
			Math.Abs(c.Longitude - selectedCity.Longitude) < 0.001);

		await _favoritesService.SaveAsync(_favorites);
		RefreshFavoritesList();
		SetStatus("Favori supprime.", false);
	}

	private void RefreshFavoritesList()
	{
		_favoritesListBox.DataSource = null;
		_favoritesListBox.DataSource = _favorites;
		_favoritesListBox.DisplayMember = nameof(FavoriteCity.DisplayName);
	}

	private void SetStatus(string text, bool isError)
	{
		_statusLabel.Text = text;
		_statusLabel.ForeColor = isError ? Color.FromArgb(214, 80, 80) : Color.FromArgb(80, 92, 111);
	}

	private void ApplyTheme(bool darkMode)
	{
		_darkModeEnabled = darkMode;
		_backgroundTop = darkMode ? Color.FromArgb(12, 18, 30) : Color.FromArgb(244, 247, 252);
		_backgroundBottom = darkMode ? Color.FromArgb(17, 24, 39) : Color.FromArgb(232, 239, 248);

		Color cardBack = darkMode ? Color.FromArgb(24, 31, 45) : Color.FromArgb(250, 252, 255);
		Color cardFore = darkMode ? Color.FromArgb(237, 242, 247) : Color.FromArgb(22, 37, 59);
		Color inputBack = darkMode ? Color.FromArgb(30, 39, 56) : Color.FromArgb(255, 255, 255);
		Color gridBack = darkMode ? Color.FromArgb(22, 31, 46) : Color.FromArgb(255, 255, 255);
		Color border = darkMode ? Color.FromArgb(44, 58, 84) : Color.FromArgb(226, 232, 240);
		Color accent = darkMode ? Color.FromArgb(55, 120, 255) : Color.FromArgb(49, 112, 250);
		Color chip = darkMode ? Color.FromArgb(29, 38, 56) : Color.FromArgb(244, 247, 252);

		foreach (Control control in Controls)
		{
			ApplyPaletteRecursive(control, cardBack, cardFore, inputBack, gridBack, border, accent, chip);
		}

		StyleButton(_searchButton, accent);
		StyleButton(_addFavoriteButton, accent);
		StyleButton(_removeFavoriteButton, accent);

		_subtitleLabel.ForeColor = darkMode ? Color.FromArgb(186, 196, 210) : Color.FromArgb(93, 110, 133);
		_temperatureLabel.ForeColor = cardFore;
		_conditionLabel.ForeColor = darkMode ? Color.FromArgb(225, 234, 248) : Color.FromArgb(50, 73, 105);
		_headerLabel.ForeColor = cardFore;
		_statusLabel.ForeColor = darkMode ? Color.FromArgb(180, 191, 208) : Color.FromArgb(80, 92, 111);
		_favoritesListBox.BackColor = chip;
		_favoritesListBox.ForeColor = cardFore;
		_searchInputCard.BackColor = darkMode ? Color.FromArgb(33, 45, 66) : Color.FromArgb(233, 243, 255);
		_cityTextBox.BackColor = _searchInputCard.BackColor;
		_cityTextBox.ForeColor = cardFore;
		_suggestionsListBox.BackColor = chip;
		_suggestionsListBox.ForeColor = cardFore;
		_weatherIconPictureBox.BackColor = Color.Transparent;
		_forecastNoteLabel.ForeColor = darkMode ? Color.FromArgb(220, 227, 240) : Color.FromArgb(35, 61, 102);
		_forecastSectionLabel.ForeColor = darkMode ? Color.FromArgb(214, 224, 240) : Color.FromArgb(56, 81, 117);

		Invalidate();
		RefreshWeatherVisuals();
	}

	private void RefreshWeatherVisuals()
	{
		if (_currentCity is null || _currentForecasts.Count == 0)
		{
			_weatherIconPictureBox.Image = WeatherIconFactory.Create(0, _darkModeEnabled, 96);
			RefreshForecastCards();
			return;
		}

		_weatherIconPictureBox.Image = WeatherIconFactory.Create(_currentForecasts[0].WeatherCode, _darkModeEnabled, 96);
		RefreshForecastCards();
	}

	private static void ApplyPaletteRecursive(Control control, Color cardBack, Color cardFore, Color inputBack, Color gridBack, Color border, Color accent, Color chip)
	{
		if (control is CardPanel)
		{
			control.BackColor = cardBack;
			control.ForeColor = cardFore;
		}
		else if (control is TextBox)
		{
			control.BackColor = inputBack;
			control.ForeColor = cardFore;
		}
		else if (control is ListBox)
		{
			control.BackColor = chip;
			control.ForeColor = cardFore;
		}
		else if (control is CheckBox)
		{
			control.BackColor = Color.Transparent;
			control.ForeColor = cardFore;
		}
		else if (control is Label)
		{
			control.BackColor = Color.Transparent;
			control.ForeColor = cardFore;
		}
		else if (control is TableLayoutPanel)
		{
			control.BackColor = Color.Transparent;
			control.ForeColor = cardFore;
		}
		else if (control is PictureBox)
		{
			control.BackColor = Color.Transparent;
		}
		else if (control is Button button)
		{
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderSize = 0;
			button.ForeColor = Color.White;
			button.BackColor = accent;
		}
		else
		{
			control.ForeColor = cardFore;
		}

		foreach (Control child in control.Controls)
		{
			ApplyPaletteRecursive(child, cardBack, cardFore, inputBack, gridBack, border, accent, chip);
		}
	}

	private sealed class ForecastDayCard : CardPanel
	{
		private readonly Label _dayLabel;
		private readonly PictureBox _iconBox;
		private readonly Label _temperatureLabel;

		public ForecastDayCard()
		{
			Margin = new Padding(6, 4, 6, 4);
			Padding = new Padding(10, 10, 10, 12);
			Radius = 20;

			TableLayoutPanel layout = new()
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 3,
				BackColor = Color.Transparent,
			};
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

			_dayLabel = new Label
			{
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point),
			};

			_iconBox = new PictureBox
			{
				Dock = DockStyle.Fill,
				SizeMode = PictureBoxSizeMode.Zoom,
				BackColor = Color.Transparent,
			};

			_temperatureLabel = new Label
			{
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold, GraphicsUnit.Point),
			};

			layout.Controls.Add(_dayLabel, 0, 0);
			layout.Controls.Add(_temperatureLabel, 0, 1);
			layout.Controls.Add(_iconBox, 0, 2);
			Controls.Add(layout);
		}

		public void SetForecast(WeatherDayForecast? forecast, bool darkMode)
		{
			Color textColor = darkMode ? Color.FromArgb(240, 245, 255) : Color.FromArgb(26, 43, 69);
			Color secondaryColor = darkMode ? Color.FromArgb(196, 207, 223) : Color.FromArgb(76, 95, 120);

			BackColor = darkMode ? Color.FromArgb(40, 52, 76) : Color.FromArgb(228, 241, 255);
			ForeColor = textColor;

			if (forecast is null)
			{
				_dayLabel.Text = "--";
				_temperatureLabel.Text = "--°";
				_iconBox.Image = WeatherIconFactory.Create(0, darkMode, 72);
				return;
			}

			CultureInfo frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
			string dayLabel = forecast.Date.ToString("ddd", frenchCulture);
			if (dayLabel.Length > 0)
			{
				dayLabel = char.ToUpper(dayLabel[0], frenchCulture) + dayLabel[1..];
			}

			_dayLabel.Text = dayLabel;
			_temperatureLabel.Text = $"{Math.Round(forecast.TempMaxC):0}°";
			_iconBox.Image = WeatherIconFactory.Create(forecast.WeatherCode, darkMode, 76);
			_dayLabel.ForeColor = textColor;
			_temperatureLabel.ForeColor = textColor;
		}
	}

	private static void StyleButton(Control control, Color backgroundColor)
	{
		if (control is Button button)
		{
			button.BackColor = backgroundColor;
			button.ForeColor = Color.White;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderSize = 0;
		}
	}

	private sealed class CitySuggestionItem
	{
		public required string Name { get; init; }
		public required string Display { get; init; }
		public override string ToString() => Display;
	}

	private class CardPanel : Panel
	{
		public int Radius { get; init; } = 20;

		protected override void OnResize(EventArgs eventargs)
		{
			base.OnResize(eventargs);
			UpdateRegion();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			using Pen pen = new(Color.FromArgb(20, 0, 0, 0), 1f);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.DrawPath(pen, CreatePath(new Rectangle(0, 0, Width - 1, Height - 1), Radius));
		}

		private void UpdateRegion()
		{
			if (Width <= 0 || Height <= 0)
			{
				return;
			}

			using GraphicsPath path = CreatePath(new Rectangle(0, 0, Width, Height), Radius);
			Region = new Region(path);
		}

		private static GraphicsPath CreatePath(Rectangle rect, int radius)
		{
			int d = radius * 2;
			GraphicsPath path = new();
			path.AddArc(rect.X, rect.Y, d, d, 180, 90);
			path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
			path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
			path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}
	}
}

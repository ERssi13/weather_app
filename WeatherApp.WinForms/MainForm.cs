using WeatherApp.WinForms.Models;
using WeatherApp.WinForms.Services;

namespace WeatherApp.WinForms;

public sealed class MainForm : Form
{
	private readonly WeatherApiService _weatherService;
	private readonly FavoritesService _favoritesService;

	private readonly TextBox _cityTextBox;
	private readonly Button _searchButton;
	private readonly Button _addFavoriteButton;
	private readonly Button _removeFavoriteButton;
	private readonly CheckBox _darkModeCheckBox;
	private readonly ListBox _favoritesListBox;
	private readonly DataGridView _forecastGrid;
	private readonly Label _headerLabel;
	private readonly Label _statusLabel;

	private readonly BindingSource _forecastBindingSource = new();
	private readonly List<FavoriteCity> _favorites = new();

	private FavoriteCity? _currentCity;

	public MainForm()
	{
		Text = "Application Meteo - Sujet 2";
		Width = 1080;
		Height = 680;
		StartPosition = FormStartPosition.CenterScreen;

		_weatherService = new WeatherApiService(new HttpClient());
		_favoritesService = new FavoritesService();

		TableLayoutPanel root = new()
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 2,
			Padding = new Padding(12),
		};

		root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

		Panel topPanel = BuildTopPanel();
		Panel favoritesPanel = BuildFavoritesPanel();
		Panel contentPanel = BuildContentPanel();

		root.Controls.Add(topPanel, 0, 0);
		root.SetColumnSpan(topPanel, 2);
		root.Controls.Add(favoritesPanel, 0, 1);
		root.Controls.Add(contentPanel, 1, 1);

		Controls.Add(root);

		_cityTextBox = (TextBox)topPanel.Controls[1];
		_searchButton = (Button)topPanel.Controls[2];
		_addFavoriteButton = (Button)topPanel.Controls[3];
		_darkModeCheckBox = (CheckBox)topPanel.Controls[4];

		_favoritesListBox = (ListBox)favoritesPanel.Controls[1];
		_removeFavoriteButton = (Button)favoritesPanel.Controls[2];

		_headerLabel = (Label)contentPanel.Controls[0];
		_forecastGrid = (DataGridView)contentPanel.Controls[1];
		_statusLabel = (Label)contentPanel.Controls[2];

		ConfigureGrid();
		WireEvents();

		Load += async (_, _) => { await LoadFavoritesAsync(); };
	}

	private static Panel BuildTopPanel()
	{
		Panel panel = new() { Dock = DockStyle.Fill };

		Label searchLabel = new()
		{
			Text = "Ville",
			AutoSize = true,
			Top = 12,
			Left = 4,
		};

		TextBox cityTextBox = new()
		{
			Left = 4,
			Top = 34,
			Width = 300,
			PlaceholderText = "Ex: Paris",
		};

		Button searchButton = new()
		{
			Text = "Rechercher",
			Left = 320,
			Top = 33,
			Width = 110,
			Height = 28,
		};

		Button addFavoriteButton = new()
		{
			Text = "Ajouter favori",
			Left = 440,
			Top = 33,
			Width = 120,
			Height = 28,
		};

		CheckBox darkModeCheckBox = new()
		{
			Text = "Mode sombre (bonus)",
			Left = 580,
			Top = 37,
			Width = 180,
		};

		panel.Controls.Add(searchLabel);
		panel.Controls.Add(cityTextBox);
		panel.Controls.Add(searchButton);
		panel.Controls.Add(addFavoriteButton);
		panel.Controls.Add(darkModeCheckBox);

		return panel;
	}

	private static Panel BuildFavoritesPanel()
	{
		Panel panel = new() { Dock = DockStyle.Fill };

		Label title = new()
		{
			Text = "Villes favorites",
			AutoSize = true,
			Left = 2,
			Top = 4,
			Font = new Font("Segoe UI", 10, FontStyle.Bold),
		};

		ListBox listBox = new()
		{
			Left = 2,
			Top = 30,
			Width = 260,
			Height = 450,
			DisplayMember = nameof(FavoriteCity.DisplayName),
		};

		Button removeFavoriteButton = new()
		{
			Text = "Supprimer",
			Left = 2,
			Top = 490,
			Width = 110,
			Height = 30,
		};

		panel.Controls.Add(title);
		panel.Controls.Add(listBox);
		panel.Controls.Add(removeFavoriteButton);

		return panel;
	}

	private static Panel BuildContentPanel()
	{
		Panel panel = new() { Dock = DockStyle.Fill };

		Label header = new()
		{
			Text = "Aucune ville selectionnee",
			AutoSize = true,
			Left = 2,
			Top = 5,
			Font = new Font("Segoe UI", 11, FontStyle.Bold),
		};

		DataGridView grid = new()
		{
			Left = 2,
			Top = 38,
			Width = 740,
			Height = 470,
			AllowUserToAddRows = false,
			AllowUserToDeleteRows = false,
			AllowUserToResizeRows = false,
			ReadOnly = true,
			AutoGenerateColumns = false,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			MultiSelect = false,
		};

		Label status = new()
		{
			Text = "Pret",
			AutoSize = true,
			Left = 2,
			Top = 520,
			ForeColor = Color.DimGray,
		};

		panel.Controls.Add(header);
		panel.Controls.Add(grid);
		panel.Controls.Add(status);

		return panel;
	}

	private void ConfigureGrid()
	{
		_forecastGrid.Columns.Add(new DataGridViewTextBoxColumn
		{
			DataPropertyName = nameof(WeatherDayForecast.Date),
			HeaderText = "Date",
			Width = 90,
			DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" },
		});

		_forecastGrid.Columns.Add(new DataGridViewTextBoxColumn
		{
			DataPropertyName = nameof(WeatherDayForecast.TempMinC),
			HeaderText = "Temp min (C)",
			Width = 95,
			DefaultCellStyle = new DataGridViewCellStyle { Format = "N1" },
		});

		_forecastGrid.Columns.Add(new DataGridViewTextBoxColumn
		{
			DataPropertyName = nameof(WeatherDayForecast.TempMaxC),
			HeaderText = "Temp max (C)",
			Width = 95,
			DefaultCellStyle = new DataGridViewCellStyle { Format = "N1" },
		});

		_forecastGrid.Columns.Add(new DataGridViewTextBoxColumn
		{
			DataPropertyName = nameof(WeatherDayForecast.CloudCoverPercent),
			HeaderText = "Nuages (%)",
			Width = 90,
		});

		_forecastGrid.Columns.Add(new DataGridViewTextBoxColumn
		{
			DataPropertyName = nameof(WeatherDayForecast.HumidityPercent),
			HeaderText = "Humidite (%)",
			Width = 95,
		});

		_forecastGrid.Columns.Add(new DataGridViewTextBoxColumn
		{
			DataPropertyName = nameof(WeatherDayForecast.WindSpeedKmh),
			HeaderText = "Vent (km/h)",
			Width = 90,
			DefaultCellStyle = new DataGridViewCellStyle { Format = "N1" },
		});

		_forecastGrid.Columns.Add(new DataGridViewTextBoxColumn
		{
			DataPropertyName = nameof(WeatherDayForecast.Icon),
			HeaderText = "Icone",
			Width = 100,
		});

		_forecastGrid.Columns.Add(new DataGridViewTextBoxColumn
		{
			DataPropertyName = nameof(WeatherDayForecast.Description),
			HeaderText = "Description",
			Width = 170,
		});

		_forecastGrid.DataSource = _forecastBindingSource;
	}

	private void WireEvents()
	{
		_searchButton.Click += async (_, _) => { await SearchAndDisplayWeatherAsync(); };
		_cityTextBox.KeyDown += async (_, e) =>
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.SuppressKeyPress = true;
				await SearchAndDisplayWeatherAsync();
			}
		};

		_addFavoriteButton.Click += async (_, _) => { await AddCurrentCityToFavoritesAsync(); };
		_removeFavoriteButton.Click += async (_, _) => { await RemoveSelectedFavoriteAsync(); };

		_favoritesListBox.DoubleClick += async (_, _) => { await ShowSelectedFavoriteWeatherAsync(); };
		_darkModeCheckBox.CheckedChanged += (_, _) => ApplyTheme(_darkModeCheckBox.Checked);
	}

	private async Task SearchAndDisplayWeatherAsync()
	{
		string cityName = _cityTextBox.Text.Trim();

		if (string.IsNullOrWhiteSpace(cityName))
		{
			SetStatus("Veuillez saisir une ville.", isError: true);
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
		SetStatus("Chargement de la meteo...", isError: false);

		try
		{
			WeatherResult result = await weatherLoader();
			_currentCity = result.City;
			_headerLabel.Text = $"Previsions pour {result.City.DisplayName} ({result.City.Latitude:F2}, {result.City.Longitude:F2})";
			_forecastBindingSource.DataSource = result.Forecasts.ToList();
			SetStatus($"{result.Forecasts.Count} jours recuperes.", isError: false);
		}
		catch (Exception ex)
		{
			SetStatus($"Erreur: {ex.Message}", isError: true);
		}
		finally
		{
			UseWaitCursor = false;
		}
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
			SetStatus("Recherchez une ville avant de l'ajouter en favori.", isError: true);
			return;
		}

		bool exists = _favorites.Any(c =>
			string.Equals(c.Name, _currentCity.Name, StringComparison.OrdinalIgnoreCase) &&
			Math.Abs(c.Latitude - _currentCity.Latitude) < 0.001 &&
			Math.Abs(c.Longitude - _currentCity.Longitude) < 0.001);

		if (exists)
		{
			SetStatus("Cette ville est deja dans les favoris.", isError: true);
			return;
		}

		_favorites.Add(_currentCity);
		await _favoritesService.SaveAsync(_favorites);
		RefreshFavoritesList();
		SetStatus("Ville ajoutee aux favoris.", isError: false);
	}

	private async Task RemoveSelectedFavoriteAsync()
	{
		if (_favoritesListBox.SelectedItem is not FavoriteCity selectedCity)
		{
			SetStatus("Selectionnez une ville favorite a supprimer.", isError: true);
			return;
		}

		_favorites.RemoveAll(c =>
			string.Equals(c.Name, selectedCity.Name, StringComparison.OrdinalIgnoreCase) &&
			Math.Abs(c.Latitude - selectedCity.Latitude) < 0.001 &&
			Math.Abs(c.Longitude - selectedCity.Longitude) < 0.001);

		await _favoritesService.SaveAsync(_favorites);
		RefreshFavoritesList();
		SetStatus("Favori supprime.", isError: false);
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
		_statusLabel.ForeColor = isError ? Color.Firebrick : Color.DimGray;
	}

	private void ApplyTheme(bool darkMode)
	{
		Color backColor = darkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
		Color foreColor = darkMode ? Color.WhiteSmoke : SystemColors.ControlText;

		ApplyThemeRecursively(this, backColor, foreColor, darkMode);
		_forecastGrid.ColumnHeadersDefaultCellStyle.BackColor = darkMode ? Color.FromArgb(45, 45, 45) : SystemColors.Control;
		_forecastGrid.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
		_forecastGrid.EnableHeadersVisualStyles = false;
	}

	private static void ApplyThemeRecursively(Control control, Color backColor, Color foreColor, bool darkMode)
	{
		if (control is TextBox)
		{
			control.BackColor = darkMode ? Color.FromArgb(45, 45, 45) : Color.White;
			control.ForeColor = foreColor;
		}
		else if (control is DataGridView grid)
		{
			grid.BackgroundColor = darkMode ? Color.FromArgb(37, 37, 37) : Color.White;
			grid.DefaultCellStyle.BackColor = darkMode ? Color.FromArgb(37, 37, 37) : Color.White;
			grid.DefaultCellStyle.ForeColor = foreColor;
			grid.GridColor = darkMode ? Color.FromArgb(60, 60, 60) : Color.LightGray;
		}
		else
		{
			control.BackColor = backColor;
			control.ForeColor = foreColor;
		}

		foreach (Control child in control.Controls)
		{
			ApplyThemeRecursively(child, backColor, foreColor, darkMode);
		}
	}
}

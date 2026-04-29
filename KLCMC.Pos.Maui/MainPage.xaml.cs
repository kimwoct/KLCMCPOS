using KLCMC.Pos.Core.ViewModels;
using System.ComponentModel;
using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Maui;

public partial class MainPage : ContentPage
{
    private enum MainTab
    {
        CurrentSale,
        DailyAccount,
        Configure
    }

    private double _productControlStartX;
    private double _productControlStartY;
    private double _productControlLastX;
    private double _productControlLastY;
    private MainTab _activeTab = MainTab.CurrentSale;

    private MainViewModel MainViewModel => (MainViewModel)BindingContext;

    public DailyAccountViewModel DailyAccountViewModel { get; }

    public MainPage(MainViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = viewModel;
        DailyAccountViewModel = services.GetRequiredService<DailyAccountViewModel>();
        DailyAccountTabContent.BindingContext = DailyAccountViewModel;
        viewModel.SaleConfirmed += DailyAccountViewModel.Load;
        viewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        viewModel.UiAppearanceChanged += OnUiAppearanceChanged;
        ApplyUiAppearance(viewModel.UiAppearance);
        CurrentSaleTabContent.SizeChanged += OnCurrentSaleTabContentSizeChanged;
        SetActiveTab(MainTab.CurrentSale);
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsProductControlVisible) && MainViewModel.IsProductControlVisible)
        {
            ApplyProductControlTranslation(_productControlLastX, _productControlLastY);
        }
    }

    private void OnCurrentSaleTabContentSizeChanged(object? sender, EventArgs e)
    {
        if (!MainViewModel.IsProductControlVisible)
        {
            return;
        }

        ApplyProductControlTranslation(_productControlLastX, _productControlLastY);
    }

    private void OnCurrentSaleTabClicked(object? sender, EventArgs e)
    {
        SetActiveTab(MainTab.CurrentSale);
    }

    private void OnDailyAccountTabClicked(object? sender, EventArgs e)
    {
        SetActiveTab(MainTab.DailyAccount);
        DailyAccountViewModel.Load();
    }

    private void OnConfigureTabClicked(object? sender, EventArgs e)
    {
        SetActiveTab(MainTab.Configure);
    }

    private void SetActiveTab(MainTab tab)
    {
        _activeTab = tab;
        var isCurrentSale = tab == MainTab.CurrentSale;
        var isDailyAccount = tab == MainTab.DailyAccount;
        var isConfigure = tab == MainTab.Configure;
        var activeTextColor = GetThemeColor("ThemePrimaryTextColor", "#D2E4FB");
        var inactiveTextColor = GetThemeColor("ThemeSecondaryTextColor", "#8EA0B9");

        CurrentSaleTabContent.IsVisible = isCurrentSale;
        DailyAccountTabContent.IsVisible = isDailyAccount;
        ConfigureTabContent.IsVisible = isConfigure;

        CurrentSaleTabButton.BackgroundColor = isCurrentSale ? Color.FromArgb("#1F3D5C") : Color.FromArgb("#253648");
        CurrentSaleTabButton.TextColor = isCurrentSale ? activeTextColor : inactiveTextColor;

        DailyAccountTabButton.BackgroundColor = isDailyAccount ? Color.FromArgb("#1F3D5C") : Color.FromArgb("#253648");
        DailyAccountTabButton.TextColor = isDailyAccount ? activeTextColor : inactiveTextColor;

        ConfigureTabButton.BackgroundColor = isConfigure ? Color.FromArgb("#1F3D5C") : Color.FromArgb("#253648");
        ConfigureTabButton.TextColor = isConfigure ? activeTextColor : inactiveTextColor;
    }

    private void OnUiAppearanceChanged(UiAppearanceOptions options)
    {
        ApplyUiAppearance(options);
    }

    private void ApplyUiAppearance(UiAppearanceOptions options)
    {
        var fontScale = Math.Clamp(options.FontScale, UiAppearanceOptions.MinFontScale, UiAppearanceOptions.MaxFontScale);
        SetColorResource("ThemeBackgroundColor", options.BackgroundColor);
        SetColorResource("ThemePrimaryTextColor", options.PrimaryTextColor);
        SetColorResource("ThemeSecondaryTextColor", options.SecondaryTextColor);
        SetColorResource("ThemeAccentColor", options.AccentColor);

        SetDoubleResource("ThemeFontScale", fontScale);
        SetDoubleResource("FontSizePageTitle", 32d * fontScale);
        SetDoubleResource("FontSizePageSubTitle", 26d * fontScale);
        SetDoubleResource("FontSizeBody", 16d * fontScale);
        SetDoubleResource("FontSizeButton", 20d * fontScale);
        SetDoubleResource("FontSizeSectionTitle", 18d * fontScale);
        SetActiveTab(_activeTab);
    }

    private void SetColorResource(string key, string value)
    {
        if (!Color.TryParse(value, out var parsed))
        {
            return;
        }

        Resources[key] = parsed;
        var appResources = Application.Current?.Resources;
        if (appResources is not null)
        {
            appResources[key] = parsed;
        }
    }

    private void SetDoubleResource(string key, double value)
    {
        Resources[key] = value;
        var appResources = Application.Current?.Resources;
        if (appResources is not null)
        {
            appResources[key] = value;
        }
    }

    private Color GetThemeColor(string key, string fallback)
    {
        if (Resources.TryGetValue(key, out var localValue) && localValue is Color localColor)
        {
            return localColor;
        }

        var appResources = Application.Current?.Resources;
        if (appResources is not null && appResources.TryGetValue(key, out var appValue) && appValue is Color appColor)
        {
            return appColor;
        }

        return Color.FromArgb(fallback);
    }

    private void OnProductControlPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (!MainViewModel.IsProductControlVisible)
        {
            return;
        }

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _productControlStartX = _productControlLastX;
                _productControlStartY = _productControlLastY;
                break;
            case GestureStatus.Running:
                ApplyProductControlTranslation(_productControlStartX + e.TotalX, _productControlStartY + e.TotalY);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _productControlLastX = ProductControlPopup.TranslationX;
                _productControlLastY = ProductControlPopup.TranslationY;
                break;
        }
    }

    private void ApplyProductControlTranslation(double translationX, double translationY)
    {
        var (clampedX, clampedY) = ClampToCurrentSaleBounds(translationX, translationY);
        ProductControlPopup.TranslationX = clampedX;
        ProductControlPopup.TranslationY = clampedY;
        _productControlLastX = clampedX;
        _productControlLastY = clampedY;
    }

    private (double X, double Y) ClampToCurrentSaleBounds(double translationX, double translationY)
    {
        var containerWidth = CurrentSaleTabContent.Width;
        var containerHeight = CurrentSaleTabContent.Height;
        var popupWidth = ProductControlPopup.Width;
        var popupHeight = ProductControlPopup.Height;

        if (containerWidth <= 0 || containerHeight <= 0 || popupWidth <= 0 || popupHeight <= 0)
        {
            return (translationX, translationY);
        }

        var maxX = Math.Max(0, (containerWidth - popupWidth) / 2d);
        var maxY = Math.Max(0, (containerHeight - popupHeight) / 2d);
        var clampedX = Math.Clamp(translationX, -maxX, maxX);
        var clampedY = Math.Clamp(translationY, -maxY, maxY);
        return (clampedX, clampedY);
    }
}

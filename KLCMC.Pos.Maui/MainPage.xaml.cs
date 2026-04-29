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
        var activeTabBackgroundColor = GetThemeColor("ThemeActionBlueColor", "#1F3D5C");
        var inactiveTabBackgroundColor = GetThemeColor("ThemeBorderMutedColor", "#253648");

        CurrentSaleTabContent.IsVisible = isCurrentSale;
        DailyAccountTabContent.IsVisible = isDailyAccount;
        ConfigureTabContent.IsVisible = isConfigure;

        CurrentSaleTabButton.BackgroundColor = isCurrentSale ? activeTabBackgroundColor : inactiveTabBackgroundColor;
        CurrentSaleTabButton.TextColor = isCurrentSale ? activeTextColor : inactiveTextColor;

        DailyAccountTabButton.BackgroundColor = isDailyAccount ? activeTabBackgroundColor : inactiveTabBackgroundColor;
        DailyAccountTabButton.TextColor = isDailyAccount ? activeTextColor : inactiveTextColor;

        ConfigureTabButton.BackgroundColor = isConfigure ? activeTabBackgroundColor : inactiveTabBackgroundColor;
        ConfigureTabButton.TextColor = isConfigure ? activeTextColor : inactiveTextColor;
    }

    private void OnUiAppearanceChanged(UiAppearanceOptions options)
    {
        ApplyUiAppearance(options);
    }

    private void ApplyUiAppearance(UiAppearanceOptions options)
    {
        var fontScale = Math.Clamp(options.FontScale, UiAppearanceOptions.MinFontScale, UiAppearanceOptions.MaxFontScale);
        var background = ParseColor(options.BackgroundColor, "#031425");
        var primary = ParseColor(options.PrimaryTextColor, "#D2E4FB");
        var secondary = ParseColor(options.SecondaryTextColor, "#8EA0B9");
        var accent = ParseColor(options.AccentColor, "#A2D149");

        SetColorResource("ThemeBackgroundColor", background);
        SetColorResource("ThemeSurfaceColor", Blend(background, primary, 0.08f));
        SetColorResource("ThemeSurfaceAltColor", Blend(background, primary, 0.13f));
        SetColorResource("ThemeModalSurfaceColor", Blend(background, primary, 0.20f));
        SetColorResource("ThemeCardColor", Blend(background, primary, 0.16f));
        SetColorResource("ThemeHeaderBarColor", Blend(background, Color.FromArgb("#000000"), 0.35f));
        SetColorResource("ThemeBorderColor", Blend(background, secondary, 0.55f));
        SetColorResource("ThemeBorderMutedColor", Blend(background, secondary, 0.38f));
        SetColorResource("ThemeInfoTextColor", Blend(secondary, primary, 0.22f));
        SetColorResource("ThemePrimaryTextColor", primary);
        SetColorResource("ThemeTertiaryTextColor", Blend(primary, secondary, 0.45f));
        SetColorResource("ThemeSecondaryTextColor", secondary);
        SetColorResource("ThemeSuccessTextColor", accent);
        SetColorResource("ThemeHighlightColor", Blend(accent, primary, 0.35f));
        SetColorResource("ThemeActionBlueColor", Blend(background, accent, 0.45f));
        SetColorResource("ThemeActionBlueSoftColor", Blend(background, accent, 0.62f));
        SetColorResource("ThemeActionTealColor", Blend(accent, Color.FromArgb("#2BA4CC"), 0.5f));
        SetColorResource("ThemeActionWarmColor", Blend(accent, Color.FromArgb("#D18467"), 0.55f));
        SetColorResource("ThemeQuickAmountColor", Blend(accent, Color.FromArgb("#E07A5F"), 0.60f));
        SetColorResource("ThemeAccentColor", accent);

        SetDoubleResource("ThemeFontScale", fontScale);
        SetDoubleResource("FontSizePageTitle", 32d * fontScale);
        SetDoubleResource("FontSizePageSubTitle", 26d * fontScale);
        SetDoubleResource("FontSizeBody", 16d * fontScale);
        SetDoubleResource("FontSizeBodySmall", 14d * fontScale);
        SetDoubleResource("FontSizeCaption", 12d * fontScale);
        SetDoubleResource("FontSizeMicro", 11d * fontScale);
        SetDoubleResource("FontSizeTiny", 10d * fontScale);
        SetDoubleResource("FontSizeButton", 20d * fontScale);
        SetDoubleResource("FontSizeButtonSmall", 14d * fontScale);
        SetDoubleResource("FontSizeNumeric", 18d * fontScale);
        SetDoubleResource("FontSizeSectionTitle", 18d * fontScale);
        SetActiveTab(_activeTab);
    }

    private void SetColorResource(string key, string value)
    {
        if (!Color.TryParse(value, out var parsed))
        {
            return;
        }

        SetColorResource(key, parsed);
    }

    private void SetColorResource(string key, Color value)
    {
        Resources[key] = value;
        var appResources = Application.Current?.Resources;
        if (appResources is not null)
        {
            appResources[key] = value;
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

    private static Color ParseColor(string value, string fallback)
    {
        return Color.TryParse(value, out var parsed) ? parsed : Color.FromArgb(fallback);
    }

    private static Color Blend(Color from, Color to, float amount)
    {
        var ratio = Math.Clamp(amount, 0f, 1f);
        return new Color(
            from.Red + ((to.Red - from.Red) * ratio),
            from.Green + ((to.Green - from.Green) * ratio),
            from.Blue + ((to.Blue - from.Blue) * ratio),
            from.Alpha + ((to.Alpha - from.Alpha) * ratio));
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

namespace KLCMC.Pos.Core.Models;

public sealed class UiAppearanceOptions
{
    public const double MinFontScale = 0.8d;
    public const double MaxFontScale = 1.6d;
    public const double DefaultFontScale = 1.0d;

    public const string DefaultPrimaryTextColor = "#D2E4FB";
    public const string DefaultSecondaryTextColor = "#8EA0B9";
    public const string DefaultBackgroundColor = "#031425";
    public const string DefaultAccentColor = "#A2D149";

    public double FontScale { get; set; } = DefaultFontScale;
    public string PrimaryTextColor { get; set; } = DefaultPrimaryTextColor;
    public string SecondaryTextColor { get; set; } = DefaultSecondaryTextColor;
    public string BackgroundColor { get; set; } = DefaultBackgroundColor;
    public string AccentColor { get; set; } = DefaultAccentColor;

    public static UiAppearanceOptions CreateDefault()
    {
        return new UiAppearanceOptions
        {
            FontScale = DefaultFontScale,
            PrimaryTextColor = DefaultPrimaryTextColor,
            SecondaryTextColor = DefaultSecondaryTextColor,
            BackgroundColor = DefaultBackgroundColor,
            AccentColor = DefaultAccentColor
        };
    }

    public UiAppearanceOptions Clone()
    {
        return new UiAppearanceOptions
        {
            FontScale = FontScale,
            PrimaryTextColor = PrimaryTextColor,
            SecondaryTextColor = SecondaryTextColor,
            BackgroundColor = BackgroundColor,
            AccentColor = AccentColor
        };
    }
}

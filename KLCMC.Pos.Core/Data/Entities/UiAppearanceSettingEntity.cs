namespace KLCMC.Pos.Core.Data.Entities;

public sealed class UiAppearanceSettingEntity
{
    public int Id { get; set; } = 1;
    public double FontScale { get; set; } = Models.UiAppearanceOptions.DefaultFontScale;
    public string PrimaryTextColor { get; set; } = Models.UiAppearanceOptions.DefaultPrimaryTextColor;
    public string SecondaryTextColor { get; set; } = Models.UiAppearanceOptions.DefaultSecondaryTextColor;
    public string BackgroundColor { get; set; } = Models.UiAppearanceOptions.DefaultBackgroundColor;
    public string AccentColor { get; set; } = Models.UiAppearanceOptions.DefaultAccentColor;
}

using System.Globalization;

namespace KLCMC.Pos.Maui.Converters;

public sealed class BoolToTileBgConverter : IValueConverter
{
    public Color SelectedColor { get; set; } = Color.FromArgb("#1F3D5C");
    public Color UnselectedColor { get; set; } = Color.FromArgb("#253648");

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? SelectedColor : UnselectedColor;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

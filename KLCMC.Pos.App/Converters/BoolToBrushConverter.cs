using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KLCMC.Pos.App.Converters;

public sealed class BoolToBrushConverter : IValueConverter
{
    public Brush SelectedBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F3D5C"));
    public Brush UnselectedBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#253648"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? SelectedBrush : UnselectedBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

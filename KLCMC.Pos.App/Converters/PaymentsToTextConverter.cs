using System.Globalization;
using System.Windows.Data;
using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.App.Converters;

public sealed class PaymentsToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<PaymentEntry> payments)
        {
            return string.Empty;
        }

        return string.Join(", ", payments.Select(p => $"{p.Method} {p.Amount:F2}"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

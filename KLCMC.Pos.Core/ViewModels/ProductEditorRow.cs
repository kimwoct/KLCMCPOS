using System.Globalization;

namespace KLCMC.Pos.Core.ViewModels;

public sealed class ProductEditorRow : BindableBase
{
    public int Id { get; init; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _priceText = "0.00";
    public string PriceText
    {
        get => _priceText;
        set => SetProperty(ref _priceText, value);
    }

    public decimal Price =>
        decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : 0m;
}

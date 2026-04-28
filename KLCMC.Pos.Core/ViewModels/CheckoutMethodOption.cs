namespace KLCMC.Pos.Core.ViewModels;

public sealed class CheckoutMethodOption : BindableBase
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

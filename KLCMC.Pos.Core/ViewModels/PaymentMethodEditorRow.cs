namespace KLCMC.Pos.Core.ViewModels;

public sealed class PaymentMethodEditorRow : BindableBase
{
    public int Id { get; init; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}

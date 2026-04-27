using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KLCMC.Pos.Core.Models;

public sealed class CartLine : INotifyPropertyChanged
{
    private int _quantity;
    private decimal _unitPrice;
    private string _remark = string.Empty;

    public required string Name { get; init; }

    public string Remark
    {
        get => _remark;
        set
        {
            var newValue = value ?? string.Empty;
            if (_remark == newValue)
            {
                return;
            }

            _remark = newValue;
            OnPropertyChanged();
        }
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be at least 1.");
            }

            if (_quantity == value)
            {
                return;
            }

            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (value < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Price cannot be negative.");
            }

            if (_unitPrice == value)
            {
                return;
            }

            _unitPrice = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal LineTotal => UnitPrice * Quantity;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

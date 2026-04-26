using System.Collections.ObjectModel;
using System.Globalization;
using KLCMC.Pos.Core.Models;
using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Core.ViewModels;

public sealed class MainViewModel : BindableBase
{
    private readonly IPrinterService _printerService;
    private readonly RelayCommand _clearCartCommand;
    private readonly RelayCommand _printReceiptCommand;
    private readonly RelayCommand _applyFinalPriceCommand;
    private CartLine? _selectedCartLine;
    private bool _isPrinterPanelExpanded = true;
    private string _pricePadInputText = "0";
    private string _statusMessage = "Ready.";

    public MainViewModel(IPrinterService printerService)
    {
        _printerService = printerService;

        PresetItems = new ReadOnlyCollection<PresetItem>(new[]
        {
            new PresetItem { Name = "Americano", DefaultPrice = 6.50m },
            new PresetItem { Name = "Latte", DefaultPrice = 8.90m },
            new PresetItem { Name = "Cappuccino", DefaultPrice = 8.90m },
            new PresetItem { Name = "Sandwich", DefaultPrice = 12.00m },
            new PresetItem { Name = "Muffin", DefaultPrice = 5.00m },
            new PresetItem { Name = "Mineral Water", DefaultPrice = 2.00m }
        });

        ConnectionModes = Enum.GetValues<PrinterConnectionMode>();
        ConnectionOptions = new PrinterConnectionOptions();
        CartLines = new ObservableCollection<CartLine>();

        AddItemCommand = new RelayCommand(AddPresetItem);
        TogglePrinterPanelCommand = new RelayCommand(_ => TogglePrinterPanel());
        ConnectPrinterCommand = new RelayCommand(_ => ConnectPrinter());
        DisconnectPrinterCommand = new RelayCommand(_ => DisconnectPrinter());
        _clearCartCommand = new RelayCommand(_ => ClearCart(), _ => CartLines.Count > 0);
        _printReceiptCommand = new RelayCommand(_ => PrintReceipt(), _ => CartLines.Count > 0);
        PricePadInputCommand = new RelayCommand(AppendPricePadInput, _ => HasSelectedCartLine);
        PricePadBackspaceCommand = new RelayCommand(_ => BackspacePricePadInput(), _ => HasSelectedCartLine);
        PricePadClearCommand = new RelayCommand(_ => ClearPricePadInput(), _ => HasSelectedCartLine);
        _applyFinalPriceCommand = new RelayCommand(_ => ApplyFinalPriceToSelectedLine(), _ => CanApplyFinalPrice());

        CartLines.CollectionChanged += (_, _) =>
        {
            RaiseTotalsChanged();
            _clearCartCommand.RaiseCanExecuteChanged();
            _printReceiptCommand.RaiseCanExecuteChanged();
            if (_selectedCartLine is not null && !CartLines.Contains(_selectedCartLine))
            {
                SelectedCartLine = null;
            }
        };
    }

    public ReadOnlyCollection<PresetItem> PresetItems { get; }

    public Array ConnectionModes { get; }

    public PrinterConnectionOptions ConnectionOptions { get; }

    public ObservableCollection<CartLine> CartLines { get; }

    public CartLine? SelectedCartLine
    {
        get => _selectedCartLine;
        set
        {
            if (!SetProperty(ref _selectedCartLine, value))
            {
                return;
            }

            SyncPricePadFromSelectedLine();
            RaisePropertyChanged(nameof(HasSelectedCartLine));
            RaisePropertyChanged(nameof(SelectedCartLineName));
            RaisePropertyChanged(nameof(SelectedCartLineTotalText));
            RaisePricePadCommandStateChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsPrinterPanelExpanded
    {
        get => _isPrinterPanelExpanded;
        private set => SetProperty(ref _isPrinterPanelExpanded, value);
    }

    public string PricePadInputText
    {
        get => _pricePadInputText;
        private set
        {
            if (!SetProperty(ref _pricePadInputText, value))
            {
                return;
            }

            _applyFinalPriceCommand.RaiseCanExecuteChanged();
        }
    }

    public bool HasSelectedCartLine => SelectedCartLine is not null;

    public string SelectedCartLineName => SelectedCartLine?.Name ?? "No product selected";

    public string SelectedCartLineTotalText =>
        SelectedCartLine is null
            ? "Select a product in cart to edit final price."
            : $"Current line total: HKD ${SelectedCartLine.LineTotal:F2}";

    public string PrinterPanelToggleText => IsPrinterPanelExpanded ? "Hide details" : "Show details";

    public string ConnectionStateText => _printerService.IsOpen ? "Connected" : "Disconnected";

    public decimal Total => CartLines.Sum(line => line.LineTotal);

    public string TotalText => $"Total: HKD ${Total:F2}";

    public RelayCommand AddItemCommand { get; }

    public RelayCommand TogglePrinterPanelCommand { get; }

    public RelayCommand ConnectPrinterCommand { get; }

    public RelayCommand DisconnectPrinterCommand { get; }

    public RelayCommand PricePadInputCommand { get; }

    public RelayCommand PricePadBackspaceCommand { get; }

    public RelayCommand PricePadClearCommand { get; }

    public RelayCommand ApplyFinalPriceCommand => _applyFinalPriceCommand;

    public RelayCommand ClearCartCommand => _clearCartCommand;

    public RelayCommand PrintReceiptCommand => _printReceiptCommand;

    private void AddPresetItem(object? parameter)
    {
        if (parameter is not PresetItem presetItem)
        {
            return;
        }

        var existingLine = CartLines.FirstOrDefault(line => line.Name == presetItem.Name);
        if (existingLine is not null)
        {
            existingLine.Quantity += 1;
            SelectedCartLine = existingLine;
            return;
        }

        var cartLine = new CartLine
        {
            Name = presetItem.Name,
            Quantity = 1,
            UnitPrice = presetItem.DefaultPrice
        };
        cartLine.PropertyChanged += (_, _) =>
        {
            RaiseTotalsChanged();
            _printReceiptCommand.RaiseCanExecuteChanged();
            if (ReferenceEquals(cartLine, SelectedCartLine))
            {
                RaisePropertyChanged(nameof(SelectedCartLineTotalText));
                _applyFinalPriceCommand.RaiseCanExecuteChanged();
            }
        };
        CartLines.Add(cartLine);
        SelectedCartLine = cartLine;
    }

    private void ConnectPrinter()
    {
        try
        {
            _printerService.Open(ConnectionOptions);
            StatusMessage = "Printer connected.";
            RaisePropertyChanged(nameof(ConnectionStateText));
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"Connection failed: {ex.Message}";
            RaisePropertyChanged(nameof(ConnectionStateText));
        }
        catch (DllNotFoundException ex)
        {
            StatusMessage = $"POSDLL load failed: {ex.Message}";
            RaisePropertyChanged(nameof(ConnectionStateText));
        }
        catch (BadImageFormatException ex)
        {
            StatusMessage = $"POSDLL architecture mismatch: {ex.Message}";
            RaisePropertyChanged(nameof(ConnectionStateText));
        }
    }

    private void DisconnectPrinter()
    {
        _printerService.Close();
        StatusMessage = "Printer disconnected.";
        RaisePropertyChanged(nameof(ConnectionStateText));
    }

    private void TogglePrinterPanel()
    {
        IsPrinterPanelExpanded = !IsPrinterPanelExpanded;
        RaisePropertyChanged(nameof(PrinterPanelToggleText));
    }

    private void AppendPricePadInput(object? parameter)
    {
        if (SelectedCartLine is null || parameter is not string key)
        {
            return;
        }

        if (key == ".")
        {
            if (PricePadInputText.Contains('.'))
            {
                return;
            }

            PricePadInputText += ".";
            return;
        }

        if (!key.All(char.IsDigit))
        {
            return;
        }

        var decimalPointIndex = PricePadInputText.IndexOf('.');
        if (decimalPointIndex >= 0 && PricePadInputText.Length - decimalPointIndex > 2)
        {
            return;
        }

        PricePadInputText = PricePadInputText == "0"
            ? key
            : PricePadInputText + key;
    }

    private void BackspacePricePadInput()
    {
        if (SelectedCartLine is null)
        {
            return;
        }

        if (PricePadInputText.Length <= 1)
        {
            PricePadInputText = "0";
            return;
        }

        PricePadInputText = PricePadInputText[..^1];
        if (PricePadInputText.EndsWith('.'))
        {
            PricePadInputText = PricePadInputText[..^1];
        }
    }

    private void ClearPricePadInput()
    {
        if (SelectedCartLine is null)
        {
            return;
        }

        PricePadInputText = "0";
    }

    private bool CanApplyFinalPrice()
    {
        return SelectedCartLine is not null &&
               decimal.TryParse(
                   PricePadInputText,
                   NumberStyles.AllowDecimalPoint,
                   CultureInfo.InvariantCulture,
                   out var finalPrice) &&
               finalPrice >= 0m;
    }

    private void ApplyFinalPriceToSelectedLine()
    {
        if (SelectedCartLine is null)
        {
            return;
        }

        if (!decimal.TryParse(
                PricePadInputText,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var finalPrice) ||
            finalPrice < 0m)
        {
            StatusMessage = "Invalid price input.";
            return;
        }

        SelectedCartLine.UnitPrice = finalPrice / SelectedCartLine.Quantity;
        StatusMessage = $"{SelectedCartLine.Name} final price updated to HKD ${finalPrice:F2}.";
        RaisePropertyChanged(nameof(SelectedCartLineTotalText));
    }

    private void SyncPricePadFromSelectedLine()
    {
        PricePadInputText = SelectedCartLine?.LineTotal.ToString("0.##", CultureInfo.InvariantCulture) ?? "0";
    }

    private void RaisePricePadCommandStateChanged()
    {
        PricePadInputCommand.RaiseCanExecuteChanged();
        PricePadBackspaceCommand.RaiseCanExecuteChanged();
        PricePadClearCommand.RaiseCanExecuteChanged();
        _applyFinalPriceCommand.RaiseCanExecuteChanged();
    }

    private void ClearCart()
    {
        CartLines.Clear();
        SelectedCartLine = null;
        StatusMessage = "Cart cleared.";
    }

    private void PrintReceipt()
    {
        var lines = ReceiptComposer.Build("KLCMC POS", CartLines, Total, DateTime.Now);

        try
        {
            _printerService.PrintReceipt(lines);
            StatusMessage = "Receipt printed.";
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"Print failed: {ex.Message}";
        }
        catch (DllNotFoundException ex)
        {
            StatusMessage = $"POSDLL load failed: {ex.Message}";
        }
        catch (BadImageFormatException ex)
        {
            StatusMessage = $"POSDLL architecture mismatch: {ex.Message}";
        }
    }

    private void RaiseTotalsChanged()
    {
        RaisePropertyChanged(nameof(Total));
        RaisePropertyChanged(nameof(TotalText));
    }
}

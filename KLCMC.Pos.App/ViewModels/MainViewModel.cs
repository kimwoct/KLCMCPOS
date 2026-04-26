using System.Collections.ObjectModel;
using KLCMC.Pos.App.Models;
using KLCMC.Pos.App.Services;

namespace KLCMC.Pos.App.ViewModels;

public sealed class MainViewModel : BindableBase
{
    private readonly IPrinterService _printerService;
    private readonly RelayCommand _clearCartCommand;
    private readonly RelayCommand _printReceiptCommand;
    private CartLine? _selectedCartLine;
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
        CartLines.CollectionChanged += (_, _) =>
        {
            RaiseTotalsChanged();
            _clearCartCommand.RaiseCanExecuteChanged();
            _printReceiptCommand.RaiseCanExecuteChanged();
        };

        AddItemCommand = new RelayCommand(AddPresetItem);
        ConnectPrinterCommand = new RelayCommand(_ => ConnectPrinter());
        DisconnectPrinterCommand = new RelayCommand(_ => DisconnectPrinter());
        _clearCartCommand = new RelayCommand(_ => ClearCart(), _ => CartLines.Count > 0);
        _printReceiptCommand = new RelayCommand(_ => PrintReceipt(), _ => CartLines.Count > 0);
    }

    public ReadOnlyCollection<PresetItem> PresetItems { get; }

    public Array ConnectionModes { get; }

    public PrinterConnectionOptions ConnectionOptions { get; }

    public ObservableCollection<CartLine> CartLines { get; }

    public CartLine? SelectedCartLine
    {
        get => _selectedCartLine;
        set => SetProperty(ref _selectedCartLine, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ConnectionStateText => _printerService.IsOpen ? "Connected" : "Disconnected";

    public decimal Total => CartLines.Sum(line => line.LineTotal);

    public string TotalText => $"Total: RM {Total:F2}";

    public RelayCommand AddItemCommand { get; }

    public RelayCommand ConnectPrinterCommand { get; }

    public RelayCommand DisconnectPrinterCommand { get; }

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
        };
        CartLines.Add(cartLine);
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

    private void ClearCart()
    {
        CartLines.Clear();
        StatusMessage = "Cart cleared.";
    }

    private void PrintReceipt()
    {
        var lines = BuildReceiptLines();

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

    private IReadOnlyList<ReceiptLine> BuildReceiptLines()
    {
        var lines = new List<ReceiptLine>
        {
            new() { Text = "KLCMC POS" },
            new() { Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new() { Text = "-------------------------------" }
        };

        foreach (var line in CartLines)
        {
            lines.Add(new ReceiptLine
            {
                Text = $"{line.Name} x{line.Quantity} @ {line.UnitPrice:F2} = {line.LineTotal:F2}"
            });
        }

        lines.Add(new ReceiptLine { Text = "-------------------------------" });
        lines.Add(new ReceiptLine { Text = $"TOTAL: RM {Total:F2}" });
        lines.Add(new ReceiptLine { Text = "Thank you!" });

        return lines;
    }

    private void RaiseTotalsChanged()
    {
        RaisePropertyChanged(nameof(Total));
        RaisePropertyChanged(nameof(TotalText));
    }
}

using System.Collections.ObjectModel;
using System.Globalization;
using KLCMC.Pos.Core.Data.Repositories;
using KLCMC.Pos.Core.Models;
using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Core.ViewModels;

public sealed class MainViewModel : BindableBase
{
    public event Action? SaleConfirmed;
    private readonly IPrinterService _printerService;
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IPrinterSettingsRepository _printerSettingsRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly RelayCommand _refreshInstalledPrintersCommand;
    private readonly RelayCommand _openPrinterPropertiesCommand;
    private readonly RelayCommand _probePrinterCapabilitiesCommand;
    private readonly RelayCommand _printAlignmentCalibrationCommand;
    private readonly RelayCommand _printCodePageSampleCommand;
    private readonly RelayCommand _printCutCalibrationCommand;
    private readonly RelayCommand _printDrawerPulseCalibrationCommand;
    private readonly RelayCommand _addNewProductCommand;
    private readonly RelayCommand _saveProductCommand;
    private readonly RelayCommand _deleteProductCommand;
    private readonly RelayCommand _clearCartCommand;
    private readonly RelayCommand _editCartLineCommand;
    private readonly RelayCommand _removeCartLineCommand;
    private readonly RelayCommand _dismissProductControlCommand;
    private readonly RelayCommand _printReceiptCommand;
    private readonly RelayCommand _applyFinalPriceCommand;
    private readonly RelayCommand _addPaymentCommand;
    private readonly RelayCommand _confirmCheckoutCommand;
    private readonly RelayCommand _openDrawerCommand;
    private readonly RelayCommand _printPrinterTestCommand;
    private readonly RelayCommand _addPaymentMethodCommand;
    private readonly RelayCommand _savePaymentMethodCommand;
    private readonly RelayCommand _deletePaymentMethodCommand;
    private CartLine? _selectedCartLine;
    private bool _isPrinterPanelExpanded = false;
    private bool _isAddProductPopupVisible;
    private bool _isCheckoutPopupVisible;
    private bool _isProductControlVisible;
    private string _newProductName = string.Empty;
    private string _newProductPriceText = string.Empty;
    private string _pricePadInputText = "0";
    private string _moneyPadInputText = "0";
    private string _printerCapabilitiesText = "No capability probe yet.";
    private string _selectedPaymentMethodName = "現金";
    private string _newPaymentMethodName = string.Empty;
    private string _statusMessage = "Ready.";
    private readonly ObservableCollection<string> _printerConsoleEntries = [];
    private readonly ObservableCollection<string> _installedPrinters = [];

    public MainViewModel(
        IPrinterService printerService,
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        IPrinterSettingsRepository printerSettingsRepository,
        IPaymentMethodRepository paymentMethodRepository)
    {
        _printerService = printerService;
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _printerSettingsRepository = printerSettingsRepository;
        _paymentMethodRepository = paymentMethodRepository;

        PresetItems = new ObservableCollection<PresetItem>(
            _productRepository.GetAll().Select(p => new PresetItem
            {
                Name = p.Name,
                DefaultPrice = p.DefaultPrice
            }));

        ReloadProductEditorRows();

        ConnectionModes = Enum.GetValues<PrinterConnectionMode>();
        ConnectionOptions = _printerSettingsRepository.Load();
        PaperWidths = [58, 80];
        CodePages = ["UTF-8", "ASCII", "Big5", "GB18030", "Shift_JIS", "Windows-1252"];
        CutModes = Enum.GetValues<PrinterCutMode>();
        CartLines = new ObservableCollection<CartLine>();
        Payments = new ObservableCollection<CheckoutPaymentLine>();
        CheckoutMethodOptions = [];
        PaymentMethodEditorRows = [];
        ReloadPaymentMethodOptions();

        AddItemCommand = new RelayCommand(AddPresetItem);
        _addNewProductCommand = new RelayCommand(_ => AddNewProduct(), _ => CanAddNewProduct());
        _saveProductCommand = new RelayCommand(SaveProduct);
        _deleteProductCommand = new RelayCommand(DeleteProduct);
        OpenAddProductPopupCommand = new RelayCommand(_ => OpenAddProductPopup());
        CloseAddProductPopupCommand = new RelayCommand(_ => CloseAddProductPopup());
        TogglePrinterPanelCommand = new RelayCommand(_ => TogglePrinterPanel());
        ConnectPrinterCommand = new RelayCommand(_ => ConnectPrinter());
        DisconnectPrinterCommand = new RelayCommand(_ => DisconnectPrinter());
        _refreshInstalledPrintersCommand = new RelayCommand(_ => RefreshInstalledPrinters());
        _openPrinterPropertiesCommand = new RelayCommand(_ => OpenPrinterProperties());
        _probePrinterCapabilitiesCommand = new RelayCommand(_ => ProbePrinterCapabilities());
        _openDrawerCommand = new RelayCommand(_ => OpenDrawer());
        _printPrinterTestCommand = new RelayCommand(_ => PrintPrinterTest());
        _printAlignmentCalibrationCommand = new RelayCommand(_ => PrintAlignmentCalibration());
        _printCodePageSampleCommand = new RelayCommand(_ => PrintCodePageSample());
        _printCutCalibrationCommand = new RelayCommand(_ => PrintCutCalibration());
        _printDrawerPulseCalibrationCommand = new RelayCommand(_ => PrintDrawerPulseCalibration());
        _clearCartCommand = new RelayCommand(_ => ClearCart(), _ => CartLines.Count > 0);
        _editCartLineCommand = new RelayCommand(EditCartLine);
        _removeCartLineCommand = new RelayCommand(RemoveCartLine);
        _dismissProductControlCommand = new RelayCommand(_ => DismissProductControl());
        _printReceiptCommand = new RelayCommand(_ => OpenCheckout(), _ => CartLines.Count > 0);
        PricePadInputCommand = new RelayCommand(AppendPricePadInput, _ => HasSelectedCartLine);
        PricePadBackspaceCommand = new RelayCommand(_ => BackspacePricePadInput(), _ => HasSelectedCartLine);
        PricePadClearCommand = new RelayCommand(_ => ClearPricePadInput(), _ => HasSelectedCartLine);
        SetPricePadAmountCommand = new RelayCommand(SetPricePadAmount, _ => HasSelectedCartLine);
        _applyFinalPriceCommand = new RelayCommand(_ => ApplyFinalPriceToSelectedLine(), _ => CanApplyFinalPrice());

        MoneyPadInputCommand = new RelayCommand(AppendMoneyPadInput);
        MoneyPadBackspaceCommand = new RelayCommand(_ => BackspaceMoneyPadInput());
        MoneyPadClearCommand = new RelayCommand(_ => ClearMoneyPadInput());
        MoneyPadQuickAmountCommand = new RelayCommand(SetMoneyPadAmount);
        SelectPaymentMethodCommand = new RelayCommand(SelectPaymentMethod);
        _addPaymentCommand = new RelayCommand(_ => AddPayment(), _ => CanAddPayment());
        RemovePaymentCommand = new RelayCommand(RemovePayment);
        _confirmCheckoutCommand = new RelayCommand(_ => ConfirmCheckout(), _ => CanConfirmCheckout());
        CancelCheckoutCommand = new RelayCommand(_ => CancelCheckout());
        _addPaymentMethodCommand = new RelayCommand(_ => AddPaymentMethod(), _ => CanAddPaymentMethod());
        _savePaymentMethodCommand = new RelayCommand(SavePaymentMethod);
        _deletePaymentMethodCommand = new RelayCommand(DeletePaymentMethod);
        AppendPrinterConsole("Printer console ready.");
        RefreshInstalledPrinters();
        ProbePrinterCapabilities();

        CartLines.CollectionChanged += (_, _) =>
        {
            RaiseTotalsChanged();
            _clearCartCommand.RaiseCanExecuteChanged();
            _printReceiptCommand.RaiseCanExecuteChanged();
            if (_selectedCartLine is not null && !CartLines.Contains(_selectedCartLine))
            {
                SelectedCartLine = null;
            }
            if (CartLines.Count == 0)
            {
                IsProductControlVisible = false;
            }
        };
    }

    public ObservableCollection<PresetItem> PresetItems { get; }

    public ObservableCollection<ProductEditorRow> ProductEditorRows { get; } = [];

    public Array ConnectionModes { get; }

    public PrinterConnectionOptions ConnectionOptions { get; }

    public IReadOnlyList<int> PaperWidths { get; }

    public IReadOnlyList<string> CodePages { get; }

    public Array CutModes { get; }

    public ObservableCollection<string> InstalledPrinters => _installedPrinters;

    public string PrinterCapabilitiesText
    {
        get => _printerCapabilitiesText;
        private set => SetProperty(ref _printerCapabilitiesText, value);
    }

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
            if (_selectedCartLine is null)
            {
                IsProductControlVisible = false;
            }
        }
    }

    public bool IsProductControlVisible
    {
        get => _isProductControlVisible;
        private set => SetProperty(ref _isProductControlVisible, value);
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

    public bool IsAddProductPopupVisible
    {
        get => _isAddProductPopupVisible;
        private set => SetProperty(ref _isAddProductPopupVisible, value);
    }

    public string NewProductName
    {
        get => _newProductName;
        set
        {
            if (!SetProperty(ref _newProductName, value))
            {
                return;
            }

            _addNewProductCommand.RaiseCanExecuteChanged();
        }
    }

    public string NewProductPriceText
    {
        get => _newProductPriceText;
        set
        {
            if (!SetProperty(ref _newProductPriceText, value))
            {
                return;
            }

            _addNewProductCommand.RaiseCanExecuteChanged();
        }
    }

    public string SelectedCartLineName => SelectedCartLine?.Name ?? "No product selected";

    public string SelectedCartLineTotalText =>
        SelectedCartLine is null
            ? "Select a product in cart to edit final price."
            : $"Current line total: HKD ${SelectedCartLine.LineTotal:F2}";

    public string PrinterPanelToggleText => IsPrinterPanelExpanded ? "▲" : "▼";

    public string ConnectionStateText => _printerService.IsOpen ? "● Connected" : "● Not Connected";

    public bool IsPrinterConnected => _printerService.IsOpen;

    public string PrinterConsoleText => string.Join(Environment.NewLine, _printerConsoleEntries);

    public decimal Total => CartLines.Sum(line => line.LineTotal);

    public string TotalText => $"總計: HKD ${Total:F2}";

    public RelayCommand AddItemCommand { get; }

    public RelayCommand AddNewProductCommand => _addNewProductCommand;

    public RelayCommand SaveProductCommand => _saveProductCommand;

    public RelayCommand DeleteProductCommand => _deleteProductCommand;

    public RelayCommand OpenAddProductPopupCommand { get; }

    public RelayCommand CloseAddProductPopupCommand { get; }

    public RelayCommand TogglePrinterPanelCommand { get; }

    public RelayCommand ConnectPrinterCommand { get; }

    public RelayCommand DisconnectPrinterCommand { get; }

    public RelayCommand RefreshInstalledPrintersCommand => _refreshInstalledPrintersCommand;

    public RelayCommand OpenPrinterPropertiesCommand => _openPrinterPropertiesCommand;

    public RelayCommand ProbePrinterCapabilitiesCommand => _probePrinterCapabilitiesCommand;

    public RelayCommand OpenDrawerCommand => _openDrawerCommand;

    public RelayCommand PrintPrinterTestCommand => _printPrinterTestCommand;

    public RelayCommand PrintAlignmentCalibrationCommand => _printAlignmentCalibrationCommand;

    public RelayCommand PrintCodePageSampleCommand => _printCodePageSampleCommand;

    public RelayCommand PrintCutCalibrationCommand => _printCutCalibrationCommand;

    public RelayCommand PrintDrawerPulseCalibrationCommand => _printDrawerPulseCalibrationCommand;

    public RelayCommand PricePadInputCommand { get; }

    public RelayCommand PricePadBackspaceCommand { get; }

    public RelayCommand PricePadClearCommand { get; }

    public RelayCommand SetPricePadAmountCommand { get; }

    public RelayCommand ApplyFinalPriceCommand => _applyFinalPriceCommand;

    public RelayCommand ClearCartCommand => _clearCartCommand;

    public RelayCommand EditCartLineCommand => _editCartLineCommand;

    public RelayCommand RemoveCartLineCommand => _removeCartLineCommand;

    public RelayCommand DismissProductControlCommand => _dismissProductControlCommand;

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

    private bool CanAddNewProduct()
    {
        return !string.IsNullOrWhiteSpace(NewProductName) &&
               decimal.TryParse(NewProductPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) &&
               price >= 0m;
    }

    private void AddNewProduct()
    {
        if (!CanAddNewProduct())
        {
            StatusMessage = "請輸入有效的產品名稱和價格。";
            return;
        }

        var name = NewProductName.Trim();
        if (PresetItems.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            StatusMessage = $"產品「{name}」已存在。";
            return;
        }

        var price = decimal.Parse(NewProductPriceText, NumberStyles.Number, CultureInfo.InvariantCulture);
        try
        {
            _productRepository.Add(name, price);
        }
        catch (Exception ex)
        {
            StatusMessage = $"儲存產品失敗：{ex.Message}";
            return;
        }

        PresetItems.Add(new PresetItem
        {
            Name = name,
            DefaultPrice = price
        });

        ProductEditorRows.Add(new ProductEditorRow
        {
            Id = _productRepository.GetAll().LastOrDefault(p => p.Name == name)?.Id ?? 0,
            Name = name,
            PriceText = price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
        });

        NewProductName = string.Empty;
        NewProductPriceText = string.Empty;
        IsAddProductPopupVisible = false;
        StatusMessage = $"產品「{name}」已新增。";
        _addNewProductCommand.RaiseCanExecuteChanged();
    }

    private void ReloadProductEditorRows()
    {
        ProductEditorRows.Clear();
        foreach (var p in _productRepository.GetAll())
        {
            ProductEditorRows.Add(new ProductEditorRow
            {
                Id = p.Id,
                Name = p.Name,
                PriceText = p.DefaultPrice.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
            });
        }
    }

    private void ReloadPaymentMethodOptions()
    {
        var methods = _paymentMethodRepository.GetAll();
        CheckoutMethodOptions.Clear();
        foreach (var m in methods)
            CheckoutMethodOptions.Add(new CheckoutMethodOption { Id = m.Id, Name = m.Name, IsSelected = m.Name == _selectedPaymentMethodName });

        // If current selection is no longer valid, default to first available
        if (!CheckoutMethodOptions.Any(o => o.IsSelected))
        {
            _selectedPaymentMethodName = CheckoutMethodOptions[0].Name;
            CheckoutMethodOptions[0].IsSelected = true;
        }

        ReloadPaymentMethodEditorRows(methods);
    }

    private void ReloadPaymentMethodEditorRows(IReadOnlyList<Data.Entities.PaymentMethodEntity>? methods = null)
    {
        methods ??= _paymentMethodRepository.GetAll();
        PaymentMethodEditorRows.Clear();
        foreach (var m in methods)
            PaymentMethodEditorRows.Add(new PaymentMethodEditorRow { Id = m.Id, Name = m.Name });
    }

    private bool CanAddPaymentMethod() => !string.IsNullOrWhiteSpace(_newPaymentMethodName);

    private void AddPaymentMethod()
    {
        var name = NewPaymentMethodName.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        _paymentMethodRepository.Add(name);
        NewPaymentMethodName = string.Empty;
        ReloadPaymentMethodOptions();
        StatusMessage = $"付款方式「{name}」已新增。";
    }

    private void SavePaymentMethod(object? parameter)
    {
        if (parameter is not PaymentMethodEditorRow row) return;
        var name = row.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) { StatusMessage = "付款方式名稱不能為空。"; return; }
        if (!_paymentMethodRepository.Update(row.Id, name)) { StatusMessage = $"找不到付款方式 #{row.Id}。"; return; }
        ReloadPaymentMethodOptions();
        StatusMessage = $"付款方式「{name}」已儲存。";
    }

    private void DeletePaymentMethod(object? parameter)
    {
        if (parameter is not PaymentMethodEditorRow row) return;
        if (!_paymentMethodRepository.Delete(row.Id)) { StatusMessage = $"找不到付款方式 #{row.Id}。"; return; }
        ReloadPaymentMethodOptions();
        StatusMessage = $"付款方式「{row.Name}」已刪除。";
    }

    private void SaveProduct(object? parameter)
    {
        if (parameter is not ProductEditorRow row)
            return;

        var name = row.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "產品名稱不能為空。";
            return;
        }

        if (!_productRepository.Update(row.Id, name, row.Price))
        {
            StatusMessage = $"找不到產品 #{row.Id}。";
            return;
        }

        // sync PresetItems
        var preset = PresetItems.FirstOrDefault(p => p.Name == row.Name || PresetItems.IndexOf(p) == ProductEditorRows.IndexOf(row));
        ReloadProductEditorRows();
        // rebuild preset items to reflect name/price change
        PresetItems.Clear();
        foreach (var p in _productRepository.GetAll())
            PresetItems.Add(new PresetItem { Name = p.Name, DefaultPrice = p.DefaultPrice });

        StatusMessage = $"產品「{name}」已儲存。";
    }

    private void DeleteProduct(object? parameter)
    {
        if (parameter is not ProductEditorRow row)
            return;

        if (!_productRepository.Delete(row.Id))
        {
            StatusMessage = $"找不到產品 #{row.Id}。";
            return;
        }

        ProductEditorRows.Remove(row);
        var preset = PresetItems.FirstOrDefault(p => p.Name == row.Name);
        if (preset is not null) PresetItems.Remove(preset);

        StatusMessage = $"產品「{row.Name}」已刪除。";
    }

    private void OpenAddProductPopup()
    {
        IsAddProductPopupVisible = true;
    }

    private void CloseAddProductPopup()
    {
        IsAddProductPopupVisible = false;
    }

    private void ConnectPrinter()
    {
        try
        {
            _printerSettingsRepository.Save(ConnectionOptions);
            _printerService.Open(ConnectionOptions);
            StatusMessage = "印表機已連接。";
            AppendPrinterConsole(StatusMessage);
            IsPrinterPanelExpanded = false;
            RaisePropertyChanged(nameof(ConnectionStateText));
            RaisePropertyChanged(nameof(IsPrinterConnected));
        }
        catch (InvalidOperationException ex)
        {
            _printerService.Close();
            StatusMessage = $"連接失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
            RaisePropertyChanged(nameof(ConnectionStateText));
        }
        catch (DllNotFoundException ex)
        {
            _printerService.Close();
            StatusMessage = $"印表機執行程式載入失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
            RaisePropertyChanged(nameof(ConnectionStateText));
        }
        catch (BadImageFormatException ex)
        {
            _printerService.Close();
            StatusMessage = $"印表機執行程式架構不符：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
            RaisePropertyChanged(nameof(ConnectionStateText));
        }
    }

    private void DisconnectPrinter()
    {
        _printerService.Close();
        StatusMessage = "印表機已中斷連接。";
        AppendPrinterConsole(StatusMessage);
        RaisePropertyChanged(nameof(ConnectionStateText));
        RaisePropertyChanged(nameof(IsPrinterConnected));
    }

    private void RefreshInstalledPrinters()
    {
        try
        {
            var printers = _printerService.GetInstalledPrinters();
            _installedPrinters.Clear();
            foreach (var printer in printers)
            {
                _installedPrinters.Add(printer);
            }

            if (_installedPrinters.Count > 0 &&
                !_installedPrinters.Any(x => string.Equals(x, ConnectionOptions.Endpoint, StringComparison.OrdinalIgnoreCase)))
            {
                ConnectionOptions.Endpoint = _installedPrinters[0];
            }

            StatusMessage = $"已載入 {_installedPrinters.Count} 台印表機。";
            AppendPrinterConsole(StatusMessage);
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"載入印表機失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
    }

    private void OpenPrinterProperties()
    {
        try
        {
            _printerService.OpenPrinterProperties(ConnectionOptions.Endpoint);
            StatusMessage = "已開啟 Windows 印表機屬性。";
            AppendPrinterConsole(StatusMessage);
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"開啟屬性失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
    }

    private void ProbePrinterCapabilities()
    {
        try
        {
            var capabilities = _printerService.ProbePrinter(ConnectionOptions.Endpoint);
            PrinterCapabilitiesText = FormatCapabilities(capabilities);
            StatusMessage = "印表機功能已更新。";
            AppendPrinterConsole(StatusMessage);
        }
        catch (InvalidOperationException ex)
        {
            PrinterCapabilitiesText = "Capability probe failed.";
            StatusMessage = $"功能探測失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
    }

    private void TogglePrinterPanel()
    {
        IsPrinterPanelExpanded = !IsPrinterPanelExpanded;
        RaisePropertyChanged(nameof(PrinterPanelToggleText));
    }

    private void OpenDrawer()
    {
        try
        {
            EnsurePrinterReady();
            _printerService.OpenDrawer();
            StatusMessage = "錢箱已開啟。";
            AppendPrinterConsole(StatusMessage);
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"開啟錢箱失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
        catch (DllNotFoundException ex)
        {
            StatusMessage = $"印表機執行程式載入失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
        catch (BadImageFormatException ex)
        {
            StatusMessage = $"印表機執行程式架構不符：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
    }

    private void PrintPrinterTest()
    {
        try
        {
            EnsurePrinterReady();
            _printerService.PrintReceipt(
            [
                new ReceiptLine { Text = "KLCMC POS" },
                new ReceiptLine { Text = "Printer Test" },
                new ReceiptLine { Text = $"Connection: {ConnectionOptions.Mode} {ConnectionOptions.Endpoint}" },
                new ReceiptLine { Text = $"Profile: {ConnectionOptions.PaperWidthMm}mm / {ConnectionOptions.CodePage} / {ConnectionOptions.CutMode}" },
                new ReceiptLine { Text = $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}" }
            ]);
            StatusMessage = "印表機測試已發送。";
            AppendPrinterConsole(StatusMessage);
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"印表機測試失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
        catch (DllNotFoundException ex)
        {
            StatusMessage = $"印表機執行程式載入失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
        catch (BadImageFormatException ex)
        {
            StatusMessage = $"印表機執行程式架構不符：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
    }

    private void PrintAlignmentCalibration()
    {
        ExecuteCalibrationPrint("Alignment calibration sent.",
        [
            new ReceiptLine { Text = "ALIGNMENT CALIBRATION" },
            new ReceiptLine { Text = "0123456789012345678901234567890123456789" },
            new ReceiptLine { Text = "|....|....|....|....|....|....|....|....|" },
            new ReceiptLine { Text = "----------------------------------------" },
            new ReceiptLine { Text = "Check left margin and line alignment." }
        ]);
    }

    private void PrintCodePageSample()
    {
        ExecuteCalibrationPrint("Codepage sample sent.",
        [
            new ReceiptLine { Text = "CODEPAGE SAMPLE" },
            new ReceiptLine { Text = $"Current: {ConnectionOptions.CodePage}" },
            new ReceiptLine { Text = "ASCII: ABCDEFG abcdefg 1234567890" },
            new ReceiptLine { Text = "Symbols: !@#$%^&*()[]{}<>/\\|`~" },
            new ReceiptLine { Text = "CJK: 中文測試 日本語 테스트" }
        ]);
    }

    private void PrintCutCalibration()
    {
        ExecuteCalibrationPrint("Cut calibration sent.",
        [
            new ReceiptLine { Text = "CUT CALIBRATION" },
            new ReceiptLine { Text = $"Cut mode: {ConnectionOptions.CutMode}" },
            new ReceiptLine { Text = "A cut command should trigger after this print." }
        ]);
    }

    private void PrintDrawerPulseCalibration()
    {
        try
        {
            EnsurePrinterReady();
            _printerService.OpenDrawer();
            StatusMessage = "錢箱脈衝校正已發送。";
            AppendPrinterConsole(StatusMessage);
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"錢箱脈衝校正失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
        catch (DllNotFoundException ex)
        {
            StatusMessage = $"印表機執行程式載入失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
        catch (BadImageFormatException ex)
        {
            StatusMessage = $"印表機執行程式架構不符：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
    }

    private void ExecuteCalibrationPrint(string successMessage, IReadOnlyList<ReceiptLine> lines)
    {
        try
        {
            EnsurePrinterReady();
            _printerService.PrintReceipt(lines);
            StatusMessage = successMessage;
            AppendPrinterConsole(StatusMessage);
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"校正列印失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
        catch (DllNotFoundException ex)
        {
            StatusMessage = $"印表機執行程式載入失敗：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
        catch (BadImageFormatException ex)
        {
            StatusMessage = $"印表機執行程式架構不符：{ex.Message}";
            AppendPrinterConsole(StatusMessage);
        }
    }

    private void EnsurePrinterReady()
    {
        _printerSettingsRepository.Save(ConnectionOptions);
        _printerService.Open(ConnectionOptions);
        RaisePropertyChanged(nameof(ConnectionStateText));
    }

    private static string FormatCapabilities(PrinterCapabilities capabilities)
    {
        var paperSizes = capabilities.PaperSizes.Count == 0
            ? "(none reported)"
            : string.Join(", ", capabilities.PaperSizes);
        return string.Join(Environment.NewLine,
        [
            $"Name        : {capabilities.PrinterName}",
            $"Driver      : {capabilities.DriverName}",
            $"Port        : {capabilities.PortName}",
            $"Default     : {(capabilities.IsDefault ? "Yes" : "No")}",
            $"Status      : {capabilities.StatusText}",
            $"Paper       : {capabilities.DefaultPaper}",
            $"Paper Sizes : {paperSizes}"
        ]);
    }

    private void AppendPrinterConsole(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        _printerConsoleEntries.Add(line);
        while (_printerConsoleEntries.Count > 120)
        {
            _printerConsoleEntries.RemoveAt(0);
        }

        RaisePropertyChanged(nameof(PrinterConsoleText));
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
            ? (key == "00" ? "0" : key)
            : PricePadInputText + key;
    }

    private void SetPricePadAmount(object? parameter)
    {
        if (SelectedCartLine is null || parameter is not string raw)
        {
            return;
        }

        if (decimal.TryParse(raw, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) && value >= 0m)
        {
            PricePadInputText = value.ToString("0.##", CultureInfo.InvariantCulture);
        }
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
            StatusMessage = "無效的價格輸入。";
            return;
        }

        SelectedCartLine.UnitPrice = finalPrice / SelectedCartLine.Quantity;
        StatusMessage = $"{SelectedCartLine.Name} 最終價格已更新為 HKD ${finalPrice:F2}。";
        RaisePropertyChanged(nameof(SelectedCartLineTotalText));
        IsProductControlVisible = false;
        SelectedCartLine = null;
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
        SetPricePadAmountCommand.RaiseCanExecuteChanged();
        _applyFinalPriceCommand.RaiseCanExecuteChanged();
    }

    private void ClearCart()
    {
        CartLines.Clear();
        SelectedCartLine = null;
        IsProductControlVisible = false;
        StatusMessage = "購物車已清空。";
    }

    private void EditCartLine(object? parameter)
    {
        if (parameter is not CartLine line)
        {
            return;
        }

        SelectedCartLine = line;
        IsProductControlVisible = true;
        StatusMessage = $"正在編輯「{line.Name}」。";
    }

    private void DismissProductControl()
    {
        SelectedCartLine = null;
        IsProductControlVisible = false;
    }

    private void RemoveCartLine(object? parameter)
    {
        if (parameter is not CartLine line)
        {
            return;
        }

        if (!CartLines.Remove(line))
        {
            return;
        }

        if (ReferenceEquals(SelectedCartLine, line))
        {
            SelectedCartLine = null;
            IsProductControlVisible = false;
        }

        StatusMessage = $"已從購物車移除「{line.Name}」。";
    }

    private void PrintReceipt()
    {
        // legacy direct print kept for reference; checkout flow is the primary path.
        ConfirmCheckout();
    }

    private void RaiseTotalsChanged()
    {
        RaisePropertyChanged(nameof(Total));
        RaisePropertyChanged(nameof(TotalText));
        RaisePropertyChanged(nameof(AmountDueText));
        RaisePropertyChanged(nameof(OutstandingAmount));
        RaisePropertyChanged(nameof(OutstandingText));
        RaisePropertyChanged(nameof(PaidTotal));
        RaisePropertyChanged(nameof(PaidTotalText));
        _confirmCheckoutCommand.RaiseCanExecuteChanged();
        _addPaymentCommand.RaiseCanExecuteChanged();
    }

    // ---- Checkout flow -----------------------------------------------------

    public ObservableCollection<CheckoutPaymentLine> Payments { get; }

    public ObservableCollection<CheckoutMethodOption> CheckoutMethodOptions { get; }

    public ObservableCollection<PaymentMethodEditorRow> PaymentMethodEditorRows { get; }

    public bool IsCheckoutPopupVisible
    {
        get => _isCheckoutPopupVisible;
        private set => SetProperty(ref _isCheckoutPopupVisible, value);
    }

    public string SelectedPaymentMethodName
    {
        get => _selectedPaymentMethodName;
        set
        {
            if (SetProperty(ref _selectedPaymentMethodName, value))
            {
                foreach (var opt in CheckoutMethodOptions)
                    opt.IsSelected = opt.Name == value;
                RaisePropertyChanged(nameof(IsCashMethodSelected));
                RaisePropertyChanged(nameof(MoneyPadLabelText));
                RaisePropertyChanged(nameof(ChangePreviewText));
                _addPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsCashMethodSelected => _selectedPaymentMethodName == "現金";

    public string NewPaymentMethodName
    {
        get => _newPaymentMethodName;
        set
        {
            if (SetProperty(ref _newPaymentMethodName, value))
                _addPaymentMethodCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand AddPaymentMethodCommand => _addPaymentMethodCommand;
    public RelayCommand SavePaymentMethodCommand => _savePaymentMethodCommand;
    public RelayCommand DeletePaymentMethodCommand => _deletePaymentMethodCommand;

    public string MoneyPadLabelText =>
        IsCashMethodSelected ? "Money Received (HKD)" : "Charge Amount (HKD)";

    public string MoneyPadInputText
    {
        get => _moneyPadInputText;
        private set
        {
            if (SetProperty(ref _moneyPadInputText, value))
            {
                RaisePropertyChanged(nameof(ChangePreviewText));
                _addPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string AmountDueText => $"應付金額: HKD ${Total:F2}";

    public decimal PaidTotal => Payments.Sum(p => p.TenderedAmount ?? p.Amount);

    public string PaidTotalText => $"已支付: HKD ${PaidTotal:F2}";

    public decimal OutstandingAmount
    {
        get
        {
            return Total - PaidTotal;
        }
    }

    public string OutstandingText => $"還欠金額: HKD ${OutstandingAmount:F2}";

    private decimal RemainingAmount
    {
        get
        {
            var remaining = Total - Payments.Sum(p => p.Amount);
            return remaining < 0m ? 0m : remaining;
        }
    }

    public string ChangePreviewText
    {
        get
        {
            if (!IsCashMethodSelected)
            {
                return string.Empty;
            }

            if (!decimal.TryParse(
                    MoneyPadInputText,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var tendered))
            {
                return string.Empty;
            }

            var change = tendered - RemainingAmount;
            return change > 0m ? $"Change preview: HKD ${change:F2}" : string.Empty;
        }
    }

    public RelayCommand MoneyPadInputCommand { get; }

    public RelayCommand MoneyPadBackspaceCommand { get; }

    public RelayCommand MoneyPadClearCommand { get; }

    public RelayCommand MoneyPadQuickAmountCommand { get; }

    public RelayCommand SelectPaymentMethodCommand { get; }

    public RelayCommand AddPaymentCommand => _addPaymentCommand;

    public RelayCommand RemovePaymentCommand { get; }

    public RelayCommand ConfirmCheckoutCommand => _confirmCheckoutCommand;

    public RelayCommand CancelCheckoutCommand { get; }

    private void OpenCheckout()
    {
        if (CartLines.Count == 0)
        {
            return;
        }

        Payments.Clear();
        SelectedPaymentMethodName = "現金";
        MoneyPadInputText = Total.ToString("0.##", CultureInfo.InvariantCulture);
        IsCheckoutPopupVisible = true;
        RaiseCheckoutTotalsChanged();
        StatusMessage = "結帳：請輸入付款資料。";
    }

    private void CancelCheckout()
    {
        IsCheckoutPopupVisible = false;
        Payments.Clear();
        MoneyPadInputText = "0";
        RaiseCheckoutTotalsChanged();
        StatusMessage = "結帳已取消。";
    }

    private void AppendMoneyPadInput(object? parameter)
    {
        if (parameter is not string key)
        {
            return;
        }

        if (key == ".")
        {
            if (MoneyPadInputText.Contains('.'))
            {
                return;
            }

            MoneyPadInputText += ".";
            return;
        }

        if (!key.All(char.IsDigit))
        {
            return;
        }

        var decimalPointIndex = MoneyPadInputText.IndexOf('.');
        if (decimalPointIndex >= 0 && MoneyPadInputText.Length - decimalPointIndex > 2)
        {
            return;
        }

        MoneyPadInputText = MoneyPadInputText == "0"
            ? (key == "00" ? "0" : key)
            : MoneyPadInputText + key;
    }

    private void BackspaceMoneyPadInput()
    {
        if (MoneyPadInputText.Length <= 1)
        {
            MoneyPadInputText = "0";
            return;
        }

        MoneyPadInputText = MoneyPadInputText[..^1];
        if (MoneyPadInputText.EndsWith('.'))
        {
            MoneyPadInputText = MoneyPadInputText[..^1];
        }
    }

    private void ClearMoneyPadInput()
    {
        MoneyPadInputText = "0";
    }

    private void SetMoneyPadAmount(object? parameter)
    {
        if (parameter is not string raw)
        {
            return;
        }

        if (decimal.TryParse(raw, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) && value >= 0m)
        {
            MoneyPadInputText = value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

    private bool CanAddPayment()
    {
        if (RemainingAmount <= 0m)
        {
            return false;
        }

        if (!decimal.TryParse(
                MoneyPadInputText,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var amount))
        {
            return false;
        }

        return amount > 0m;
    }

    private void SelectPaymentMethod(object? parameter)
    {
        if (parameter is string name)
            SelectedPaymentMethodName = name;
        else if (parameter is CheckoutMethodOption opt)
            SelectedPaymentMethodName = opt.Name;
    }

    private void AddPayment()
    {
        if (!CanAddPayment())
        {
            return;
        }

        var input = decimal.Parse(MoneyPadInputText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        var outstanding = RemainingAmount;

        CheckoutPaymentLine line;
        if (_selectedPaymentMethodName == "現金")
        {
            var applied = Math.Min(input, outstanding);
            var change = input - applied;
            line = new CheckoutPaymentLine
            {
                Method = "現金",
                Amount = applied,
                TenderedAmount = input,
                ChangeAmount = change > 0m ? change : 0m
            };
        }
        else
        {
            var applied = Math.Min(input, outstanding);
            line = new CheckoutPaymentLine
            {
                Method = _selectedPaymentMethodName,
                Amount = applied
            };
        }

        Payments.Add(line);
        RaiseCheckoutTotalsChanged();
        MoneyPadInputText = RemainingAmount.ToString("0.##", CultureInfo.InvariantCulture);
        StatusMessage = $"已新增付款：{line.DisplayText}。";
    }

    private void RemovePayment(object? parameter)
    {
        if (parameter is not CheckoutPaymentLine line)
        {
            return;
        }

        Payments.Remove(line);
        RaiseCheckoutTotalsChanged();
        MoneyPadInputText = RemainingAmount.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private bool CanConfirmCheckout()
    {
        return IsCheckoutPopupVisible && CartLines.Count > 0 && RemainingAmount <= 0m && Total > 0m;
    }

    private void ConfirmCheckout()
    {
        if (!CanConfirmCheckout())
        {
            StatusMessage = "無法確認：仍有未付金額。";
            return;
        }

        var paymentEntries = Payments.Select(p => p.ToEntry()).ToList();

        try
        {
            _saleRepository.Record(CartLines, Total, paymentEntries);
        }
        catch (Exception ex)
        {
            StatusMessage = $"記錄銷售失敗：{ex.Message}";
            return;
        }

        try
        {
            var lines = ReceiptComposer.Build("KLCMC POS", CartLines, Total, DateTime.Now, paymentEntries, ConnectionOptions);
            EnsurePrinterReady();
            _printerService.PrintReceipt(lines);
            StatusMessage = "銷售已記錄並列印收據。";
        }
        catch (Exception ex)
        {
            StatusMessage = $"銷售已記錄；列印失敗：{ex.Message}";
        }

        IsCheckoutPopupVisible = false;
        Payments.Clear();
        MoneyPadInputText = "0";
        CartLines.Clear();
        SelectedCartLine = null;
        RaiseCheckoutTotalsChanged();
        SaleConfirmed?.Invoke();
    }

    private void RaiseCheckoutTotalsChanged()
    {
        RaisePropertyChanged(nameof(PaidTotal));
        RaisePropertyChanged(nameof(PaidTotalText));
        RaisePropertyChanged(nameof(OutstandingAmount));
        RaisePropertyChanged(nameof(OutstandingText));
        RaisePropertyChanged(nameof(AmountDueText));
        RaisePropertyChanged(nameof(ChangePreviewText));
        _confirmCheckoutCommand.RaiseCanExecuteChanged();
        _addPaymentCommand.RaiseCanExecuteChanged();
    }
}

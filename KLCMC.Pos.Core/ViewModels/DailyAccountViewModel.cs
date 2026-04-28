using System.Collections.ObjectModel;
using KLCMC.Pos.Core.Data.Repositories;
using KLCMC.Pos.Core.Models;
using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Core.ViewModels;

public sealed class DailyAccountViewModel : BindableBase
{
    private readonly ISaleRepository _saleRepository;
    private readonly IFileLauncher _fileLauncher;
    private readonly IConfirmDialog _confirmDialog;
    private DateTime _selectedDate = DateTime.Today;
    private DailySummary _summary;
    private string _statusMessage = string.Empty;
    private string? _lastExportPath;

    public DailyAccountViewModel(ISaleRepository saleRepository, IPrinterService printerService, IFileLauncher fileLauncher, IConfirmDialog confirmDialog)
    {
        _saleRepository = saleRepository;
        _fileLauncher = fileLauncher;
        _confirmDialog = confirmDialog;
        _summary = DailySummary.Empty(DateOnly.FromDateTime(DateTime.Today));

        ByMethod = new ObservableCollection<MethodTotal>();
        Transactions = new ObservableCollection<SaleSummary>();

        LoadCommand = new RelayCommand(_ => Load());
        TodayCommand = new RelayCommand(_ =>
        {
            SelectedDate = DateTime.Today;
        });
        PrevDayCommand = new RelayCommand(_ => SelectedDate = SelectedDate.AddDays(-1));
        NextDayCommand = new RelayCommand(_ => SelectedDate = SelectedDate.AddDays(1));
        ExportDailyReportCommand = new RelayCommand(_ => ExportDailyReport(), _ => Summary.TransactionCount > 0);
        OpenLastExportCommand = new RelayCommand(_ => OpenLastExport(), _ => _lastExportPath != null);
        DeleteDayDataCommand = new RelayCommand(_ => DeleteDayData(), _ => Summary.TransactionCount > 0);
        VoidTransactionCommand = new RelayCommand(id => VoidTransaction(id));

        Load();
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            var normalized = value.Date;
            if (SetProperty(ref _selectedDate, normalized))
            {
                Load();
            }
        }
    }

    public DailySummary Summary
    {
        get => _summary;
        private set
        {
            if (SetProperty(ref _summary, value))
            {
                RaisePropertyChanged(nameof(TransactionCountText));
                RaisePropertyChanged(nameof(GrossTotalText));
                RaisePropertyChanged(nameof(HeaderText));
                ((RelayCommand)ExportDailyReportCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteDayDataCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<MethodTotal> ByMethod { get; }

    public ObservableCollection<SaleSummary> Transactions { get; }

    public string HeaderText => $"每日帳目 — {SelectedDate:yyyy-MM-dd}";

    public string TransactionCountText => $"交易次數：{Summary.TransactionCount}";

    public string GrossTotalText => $"總收入：HKD ${Summary.GrossTotal:F2}";

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string? LastExportFileName => _lastExportPath == null ? null : Path.GetFileName(_lastExportPath);

    public bool HasLastExport => _lastExportPath != null;

    public RelayCommand LoadCommand { get; }
    public RelayCommand TodayCommand { get; }
    public RelayCommand PrevDayCommand { get; }
    public RelayCommand NextDayCommand { get; }
    public RelayCommand ExportDailyReportCommand { get; }
    public RelayCommand OpenLastExportCommand { get; }
    public RelayCommand DeleteDayDataCommand { get; }
    public RelayCommand VoidTransactionCommand { get; }

    public void Load()
    {
        try
        {
            var local = DateOnly.FromDateTime(_selectedDate);
            var summary = _saleRepository.GetDailySummary(local);
            Summary = summary;

            ByMethod.Clear();
            foreach (var m in summary.ByMethod)
            {
                ByMethod.Add(m);
            }

            Transactions.Clear();
            foreach (var t in summary.Transactions)
            {
                Transactions.Add(t);
            }

            StatusMessage = summary.TransactionCount == 0
                ? "所選日期沒有交易記錄。"
                : $"已載入 {summary.TransactionCount} 筆交易。";
        }
        catch (Exception ex)
        {
            StatusMessage = $"載入失敗：{ex.Message}";
        }
    }

    private async void ExportDailyReport()
    {
        string filePath;
        try
        {
            var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (!Directory.Exists(downloads))
                downloads = Path.GetTempPath();
            filePath = ExcelExportService.ExportDailyReport(Summary, downloads);
        }
        catch (Exception ex)
        {
            StatusMessage = $"匯出失敗：{ex.Message}";
            return;
        }

        _lastExportPath = filePath;
        RaisePropertyChanged(nameof(LastExportFileName));
        RaisePropertyChanged(nameof(HasLastExport));
        ((RelayCommand)OpenLastExportCommand).RaiseCanExecuteChanged();
        StatusMessage = "已匯出：";

        try
        {
            await _fileLauncher.OpenFileAsync(filePath);
        }
        catch
        {
            // Open failed — file is saved, link is still clickable
        }
    }

    private async void OpenLastExport()
    {
        if (_lastExportPath == null) return;
        try { await _fileLauncher.OpenFileAsync(_lastExportPath); } catch { }
    }

    private async void DeleteDayData()
    {
        var dateLabel = SelectedDate.ToString("yyyy-MM-dd");
        var confirmed = await _confirmDialog.ConfirmAsync(
            title: $"刪除 {dateLabel} 的記錄",
            message: $"確定要刪除 {dateLabel} 的所有交易記錄嗎？此操作不可撤銷。",
            accept: "刪除",
            cancel: "取消");

        if (!confirmed) return;

        try
        {
            _saleRepository.DeleteForDate(DateOnly.FromDateTime(SelectedDate));
            Load();
            StatusMessage = $"已刪除 {dateLabel} 的所有交易記錄。";
        }
        catch (Exception ex)
        {
            StatusMessage = $"刪除失敗：{ex.Message}";
        }
    }

    private async void VoidTransaction(object? parameter)
    {
        if (parameter is not int saleId) return;

        var confirmed = await _confirmDialog.ConfirmAsync(
            title: $"作廢交易 #{saleId}",
            message: $"確定要作廢交易 #{saleId} 嗎？此操作不可撤銷。",
            accept: "作廢",
            cancel: "取消");

        if (!confirmed) return;

        try
        {
            _saleRepository.DeleteById(saleId);
            Load();
            StatusMessage = $"交易 #{saleId} 已作廢。";
        }
        catch (Exception ex)
        {
            StatusMessage = $"作廢失敗：{ex.Message}";
        }
    }
}

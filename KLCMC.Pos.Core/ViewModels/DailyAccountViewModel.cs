using System.Collections.ObjectModel;
using KLCMC.Pos.Core.Data.Repositories;
using KLCMC.Pos.Core.Models;
using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Core.ViewModels;

public sealed class DailyAccountViewModel : BindableBase
{
    private readonly ISaleRepository _saleRepository;
    private readonly IFileLauncher _fileLauncher;
    private DateTime _selectedDate = DateTime.Today;
    private DailySummary _summary;
    private string _statusMessage = string.Empty;

    public DailyAccountViewModel(ISaleRepository saleRepository, IPrinterService printerService, IFileLauncher fileLauncher)
    {
        _saleRepository = saleRepository;
        _fileLauncher = fileLauncher;
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

    public RelayCommand LoadCommand { get; }
    public RelayCommand TodayCommand { get; }
    public RelayCommand PrevDayCommand { get; }
    public RelayCommand NextDayCommand { get; }
    public RelayCommand ExportDailyReportCommand { get; }

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
        try
        {
            var folder = Path.GetTempPath();
            var filePath = ExcelExportService.ExportDailyReport(Summary, folder);
            StatusMessage = $"已匯出：{Path.GetFileName(filePath)}";
            await _fileLauncher.OpenFileAsync(filePath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"匯出失敗：{ex.Message}";
        }
    }
}

using System.Collections.ObjectModel;
using KLCMC.Pos.Core.Data.Repositories;
using KLCMC.Pos.Core.Models;
using KLCMC.Pos.Core.Services;

namespace KLCMC.Pos.Core.ViewModels;

public sealed class DailyAccountViewModel : BindableBase
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPrinterService _printerService;
    private DateTime _selectedDate = DateTime.Today;
    private DailySummary _summary;
    private string _statusMessage = string.Empty;

    public DailyAccountViewModel(ISaleRepository saleRepository, IPrinterService printerService)
    {
        _saleRepository = saleRepository;
        _printerService = printerService;
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
        PrintDailyReportCommand = new RelayCommand(_ => PrintDailyReport(), _ => Summary.TransactionCount > 0);

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
                ((RelayCommand)PrintDailyReportCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<MethodTotal> ByMethod { get; }

    public ObservableCollection<SaleSummary> Transactions { get; }

    public string HeaderText => $"Daily Account — {SelectedDate:yyyy-MM-dd}";

    public string TransactionCountText => $"Transactions: {Summary.TransactionCount}";

    public string GrossTotalText => $"Gross: HKD ${Summary.GrossTotal:F2}";

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand LoadCommand { get; }
    public RelayCommand TodayCommand { get; }
    public RelayCommand PrevDayCommand { get; }
    public RelayCommand NextDayCommand { get; }
    public RelayCommand PrintDailyReportCommand { get; }

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
                ? "No transactions for selected date."
                : $"Loaded {summary.TransactionCount} transaction(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load: {ex.Message}";
        }
    }

    private void PrintDailyReport()
    {
        var lines = ReceiptComposer.BuildDailyReport("KLCMC POS", Summary);
        try
        {
            _printerService.PrintReceipt(lines);
            StatusMessage = "Daily report printed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Print failed: {ex.Message}";
        }
    }
}

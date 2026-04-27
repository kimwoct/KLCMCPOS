using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Data.Repositories;

public interface IPrinterSettingsRepository
{
    PrinterConnectionOptions Load();
    void Save(PrinterConnectionOptions options);
}

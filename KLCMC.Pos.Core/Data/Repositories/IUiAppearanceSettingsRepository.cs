using KLCMC.Pos.Core.Models;

namespace KLCMC.Pos.Core.Data.Repositories;

public interface IUiAppearanceSettingsRepository
{
    UiAppearanceOptions Load();
    void Save(UiAppearanceOptions options);
}

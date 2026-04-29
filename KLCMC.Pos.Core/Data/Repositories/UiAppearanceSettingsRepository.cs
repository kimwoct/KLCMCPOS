using KLCMC.Pos.Core.Data.Entities;
using KLCMC.Pos.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.Core.Data.Repositories;

public sealed class UiAppearanceSettingsRepository : IUiAppearanceSettingsRepository
{
    private readonly IDbContextFactory<PosDbContext> _dbFactory;

    public UiAppearanceSettingsRepository(IDbContextFactory<PosDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public UiAppearanceOptions Load()
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = db.UiAppearanceSettings.Find(1);
        if (entity is null)
        {
            entity = CreateDefaultEntity();
            db.UiAppearanceSettings.Add(entity);
            db.SaveChanges();
        }

        return new UiAppearanceOptions
        {
            FontScale = entity.FontScale,
            PrimaryTextColor = entity.PrimaryTextColor,
            SecondaryTextColor = entity.SecondaryTextColor,
            BackgroundColor = entity.BackgroundColor,
            AccentColor = entity.AccentColor
        };
    }

    public void Save(UiAppearanceOptions options)
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = db.UiAppearanceSettings.Find(1);
        if (entity is null)
        {
            entity = CreateDefaultEntity();
            db.UiAppearanceSettings.Add(entity);
        }

        entity.FontScale = options.FontScale;
        entity.PrimaryTextColor = options.PrimaryTextColor;
        entity.SecondaryTextColor = options.SecondaryTextColor;
        entity.BackgroundColor = options.BackgroundColor;
        entity.AccentColor = options.AccentColor;
        db.SaveChanges();
    }

    private static UiAppearanceSettingEntity CreateDefaultEntity()
    {
        return new UiAppearanceSettingEntity
        {
            Id = 1,
            FontScale = UiAppearanceOptions.DefaultFontScale,
            PrimaryTextColor = UiAppearanceOptions.DefaultPrimaryTextColor,
            SecondaryTextColor = UiAppearanceOptions.DefaultSecondaryTextColor,
            BackgroundColor = UiAppearanceOptions.DefaultBackgroundColor,
            AccentColor = UiAppearanceOptions.DefaultAccentColor
        };
    }
}

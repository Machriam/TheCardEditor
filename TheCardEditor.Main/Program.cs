﻿using System.Collections;
using System.Globalization;
using Blazored.Modal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCardEditor.DataModel;
using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Main.Core;
using TheCardEditor.Main.Core.Grid;
using TheCardEditor.Services;
using TheCardEditor.Shared;
using TheCardEditor.SheetComponent;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace TheCardEditor.Main;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var directory = Path.GetFullPath(Directory.GetCurrentDirectory());
        var builder = new ConfigurationBuilder()
                        .SetBasePath(directory)
                        .AddJsonFile(AppSettings.AppsettingsName, optional: false, reloadOnChange: true);
        var configuration = builder.Build();
        var environmentConfiguration = new EnvironmentConfiguration(configuration);
        var services = new ServiceCollection();
        services.AddSingleton<IEnvironmentConfiguration>(environmentConfiguration);
        services.AddWindowsFormsBlazorWebView();
        services.AddEntityFrameworkSqlite();
        services.AddDbContext<DataContext>(opt =>
                    opt.UseSqlite($"Data Source={environmentConfiguration.DatabasePath}",
                    a => a.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
        services.AddSingleton<MainForm>();
        services.AddSingleton<IModalHelper, ModalHelper>();
        services.AddTransient<IErrorLogger, ErrorLogger>();
        services.AddTransient<ILocalStorageInterop, LocalStorageInterop>();
        services.AddTransient<IGridViewFactory, GridViewFactory>();
        services.AddSingleton<ApplicationStorage>();
        services.AddTransient(s => new ServiceAccessor<FontService>(s));
        services.AddTransient(s => new ServiceAccessor<GameService>(s));
        services.AddTransient(s => new ServiceAccessor<CardSetService>(s));
        services.AddTransient(s => new ServiceAccessor<CardService>(s));
        services.AddTransient(s => new ServiceAccessor<PictureService>(s));
        services.AddTransient(s => new ServiceAccessor<TemplateService>(s));
        services.AddTransient<ISheetViewFactory, SheetViewFactory>();
        services.AddTransient<ICanvasInteropFactory, CanvasInteropFactory>();
        services.AddTransient<IShortcutRegistrator, ShortcutRegistrator>();
        services.AddBlazoredModal();
        services.AddHotKeys2();
        var provider = services.BuildServiceProvider();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            db.Migrate();
        }
        var mainForm = provider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
}

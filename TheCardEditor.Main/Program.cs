﻿using Blazored.Modal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Main.Core;
using TheCardEditor.Main.Core.Grid;
using TheCardEditor.Services;
using TheCardEditor.Shared;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace TheCardEditor.Main;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
#if DEBUG
        const string AppsettingsName = "appsettings.Development.json";
        var directory = Path.GetFullPath(Directory.GetCurrentDirectory());
#else
        const string AppsettingsName = "appsettings.json";
        var directory = Directory.GetCurrentDirectory();
#endif
        var builder = new ConfigurationBuilder()
                        .SetBasePath(directory)
                        .AddJsonFile(AppsettingsName, optional: false, reloadOnChange: true);
        var configuration = builder.Build();
        var environmentConfiguration = new EnvironmentConfiguration(configuration);
        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
        services.AddEntityFrameworkSqlite();
        services.AddDbContext<DataContext>(opt =>
                    opt.UseSqlite($"Data Source={environmentConfiguration.DatabasePath}", a =>
        {
            a.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
        services.AddSingleton<MainForm>();
        services.AddSingleton<IModalHelper, ModalHelper>();
        services.AddTransient<IErrorLogger, JsInterop>();
        services.AddTransient<IJsInterop, JsInterop>();
        services.AddTransient<ILocalStorageInterop, LocalStorageInterop>();
        services.AddTransient<IGridViewFactory, GridViewFactory>();
        services.AddSingleton<ApplicationStorage>();
        services.AddTransient(s => new ServiceAccessor<FontService>(s));
        services.AddTransient(s => new ServiceAccessor<GameService>(s));
        services.AddTransient(s => new ServiceAccessor<CardSetService>(s));
        services.AddTransient(s => new ServiceAccessor<CardService>(s));
        services.AddTransient<ICanvasInteropFactory, CanvasInteropFactory>();
        services.AddTransient<IShortcutRegistrator, ShortcutRegistrator>();
        services.AddBlazoredModal();
        services.AddHotKeys2();
        var mainForm = services.BuildServiceProvider().GetService<MainForm>();
        Application.Run(mainForm);
    }
}

using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using TheCardEditor.Main.Core;
using TheCardEditor.Main.Features.AppLayout;

namespace TheCardEditor;

public partial class MainForm : Form
{
    private readonly IServiceProvider _services;

    public MainForm(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
        blazorWebView1.HostPage = "wwwroot\\index.html";
        blazorWebView1.Services = services;
        blazorWebView1.RootComponents.Add<App>("#app");
        FormClosing += MainForm_FormClosing;
        Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        var configuration = _services.GetRequiredService<IEnvironmentConfiguration>();
        var windowPosition = configuration.WindowPosition;
        Location = new(windowPosition.LocationX, windowPosition.LocationY);
        Size = new(windowPosition.SizeX, windowPosition.SizeY);
        WindowState = windowPosition.State;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        var configuration = _services.GetRequiredService<IEnvironmentConfiguration>();
        configuration.SaveNewWindowPosition(new()
        {
            LocationX = WindowState == FormWindowState.Normal ? Location.X : RestoreBounds.Location.X,
            LocationY = WindowState == FormWindowState.Normal ? Location.Y : RestoreBounds.Location.Y,
            SizeX = WindowState == FormWindowState.Normal ? Size.Width : RestoreBounds.Size.Width,
            SizeY = WindowState == FormWindowState.Normal ? Size.Height : RestoreBounds.Size.Height,
            State = WindowState
        });
    }
}

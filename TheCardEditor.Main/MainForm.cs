using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using TheCardEditor.AppLayout;

namespace TheCardEditor;

public partial class MainForm : Form
{
    public MainForm(IServiceProvider services)
    {
        InitializeComponent();
        blazorWebView1.HostPage = "wwwroot\\index.html";
        blazorWebView1.Services = services;
        blazorWebView1.RootComponents.Add<App>("#app");
    }
}

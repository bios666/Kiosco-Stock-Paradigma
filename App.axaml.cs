using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StockVentas.Services;
using StockVentas.Views;

namespace StockVentas
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Los servicios se arman una única vez, igual que hacía Program.cs en la versión de consola.
                var historial = new HistorialService();
                var stockService = new StockService(historial);
                var pagoService = new PagoService();
                var ventaService = new VentaService(stockService, pagoService, historial);

                desktop.MainWindow = new MainWindow(stockService, pagoService, ventaService, historial);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

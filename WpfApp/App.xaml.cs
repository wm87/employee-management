using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using WpfApp.Model.Data;
using WpfApp.Services;

namespace WpfApp
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; } = null!;
        public static ILoggerFactory? LoggerFactory { get; private set; }

        public App()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(@"C:\Users\mawei\Documents\repos\WpfApp\WpfApp")
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config) // 🔥 zentral über Konfiguration
                .CreateLogger();

            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();
            });

            string? connStr = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connStr))
                throw new InvalidOperationException("ConnectionString cannot be null or empty.");

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(config);

                    services.AddDbContextFactory<AppDbContext>(options =>
                    {
                        options.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
                    });

                    services.AddSingleton<DbManager>();
                    services.AddSingleton<ElasticsearchService>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .UseSerilog() // ← Logging an Host übergeben
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}

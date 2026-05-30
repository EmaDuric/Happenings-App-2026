using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows.Forms;

namespace Happenings.WinUI
{
    internal static class Program
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Uèitaj konfiguraciju iz appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            Application.Run(new Forms.frmLogin());
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<APIService>();
        }
    }
}
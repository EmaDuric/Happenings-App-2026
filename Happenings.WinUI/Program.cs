using Microsoft.Extensions.DependencyInjection;
using System;
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

            // Setup Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Pokreni Login Form umjesto Form1
            Application.Run(new Forms.frmLogin());
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<APIService>();
        }
    }
}
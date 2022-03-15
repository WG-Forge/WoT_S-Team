using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UIClient.Model;
using UIClient.Model.Config;
using UIClient.View.Pages;
using UIClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static UIClient.Model.Core;

namespace UIClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IHost __Host;
        public static IHost Host => __Host ??= EntryPoint.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        private static Core __Core;
        public static Core Core => __Core ??= new Core();

        private static AppConfig __AppConfig;
        public static AppConfig AppConfig => __AppConfig ??= Host.Services.GetRequiredService<AppConfig>();

        internal static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<AppConfigJson>(host.Configuration.GetSection(nameof(AppConfigJson)));
            services.AddSingleton<AppConfig>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LoadPageViewModel>();
            services.AddSingleton<LoadPage>();
            services.AddSingleton<GamePageViewModel>();
            services.AddSingleton<GamePage>();
            services.AddTransient<EscapeMenuPageViewModel>();
            services.AddTransient<EscapeMenuPage>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var host = Host;

            if (e.Args.Length > 0)
            {
                //u zxcv p 111 g z  pc 1 tc 45 o 0  ai 0
                try
                {
                    var page = Host.Services.GetRequiredService<LoadPageViewModel>();
                    var page_game = Host.Services.GetRequiredService<GamePageViewModel>();

                    int index_u = Array.IndexOf(e.Args, "-u");
                    int index_p = Array.IndexOf(e.Args, "-p");
                    int index_g = Array.IndexOf(e.Args, "-g");
                    int index_pc = Array.IndexOf(e.Args, "-pc");
                    int index_tc = Array.IndexOf(e.Args, "-tc");
                    int index_o = Array.IndexOf(e.Args, "-o");
                    int index_ai = Array.IndexOf(e.Args, "-ai");
                    int index_exit = Array.IndexOf(e.Args, "-q");


                    if (index_exit != -1) AppConfig.ExitEnd = true;
                    page.UserName = e.Args[index_u + 1];
                    page.Pass = e.Args[index_p + 1];
                    page.GameName = e.Args[index_g + 1];
                    page.PlayersMax = Convert.ToInt32(e.Args[index_pc + 1]);
                    page.TurnMax = Convert.ToInt32(e.Args[index_tc + 1]);
                    page.IsObserver = Convert.ToBoolean(Convert.ToInt32(e.Args[index_o + 1]));
                    page_game.Field.AIEnable = Convert.ToBoolean(Convert.ToInt32(e.Args[index_ai + 1]));
                    await App.Current.Dispatcher.BeginInvoke(new Action(() => { page.LoginCommand.Execute(null); }));
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }

            await host.StartAsync().ConfigureAwait(false);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            var host = Host;
            await host.StopAsync().ConfigureAwait(false);
            host.Dispose();
            __Host = null;
        }
    }
}


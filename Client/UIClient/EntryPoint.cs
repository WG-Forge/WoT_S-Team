using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using UIClient.Model;
using UIClient.Model.Config;
using static UIClient.Model.Core;

namespace UIClient
{
    public class EntryPoint
    {
        public const string config_path = "AppConfig.json";

        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
            UpdateAppSetting();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host_builder = Host.CreateDefaultBuilder(args);
            host_builder.UseContentRoot(Environment.CurrentDirectory);
            host_builder.ConfigureAppConfiguration((host, cfg) =>
            {
                cfg.SetBasePath(Environment.CurrentDirectory);
                cfg.AddJsonFile(config_path, false, true);
                cfg.AddEnvironmentVariables();
                if (args != null) cfg.AddCommandLine(args);

                try
                {
                    cfg.Build();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    App.Current.Shutdown();
                }
            });

            host_builder.ConfigureServices(App.ConfigureServices);
            return host_builder;
        }

        public static void UpdateAppSetting()
        {
            var conf = App.AppConfig;
            string json_str = JsonConvert.SerializeObject(conf, Formatting.Indented);
            File.WriteAllText(config_path, json_str);
        }
    }
}

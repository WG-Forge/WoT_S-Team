using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UIClient.Model;
using UIClient.View.Pages;

namespace UIClient.ViewModel
{
    internal class ViewModelLocator
    {
        public MainWindowViewModel MainWindowViewModel => App.Host.Services.GetRequiredService<MainWindowViewModel>();
        public LoadPageViewModel LoadPageViewModel => App.Host.Services.GetRequiredService<LoadPageViewModel>();
        public GamePageViewModel GamePageViewModel => App.Host.Services.GetRequiredService<GamePageViewModel>();
        public EscapeMenuPageViewModel EscapeMenuPageViewModel => App.Host.Services.GetRequiredService<EscapeMenuPageViewModel>();
    }
}

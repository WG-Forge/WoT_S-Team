using UIClient.Infrastructure.Command;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using UIClient.View.Pages;
using UIClient.Model.Config;
using UIClient.Model;
using System.Threading;
using System.Reflection;
using System.Windows.Media;

namespace UIClient.ViewModel
{
    internal class MainWindowViewModel : Base.ViewModelBase
    {
        #region Properties
        #region Core Core : ядро
        private Core _Core;
        /// <summary>ядро</summary>
        public Core Core
        {
            get { return _Core; }
            set { Set(ref _Core, value); }
        }
        #endregion
        #region string Title : Заголовок акна
        private string _Title = "MainWindowViewModel";
        /// <summary>Заголовок акна</summary>
        public string Title
        {
            get { return _Title; }
            set { Set(ref _Title, value); }
        }
        #endregion
        #region string Version : текущая версия приложения
        private string _Version;
        /// <summary>текущая версия приложения</summary>
        public string Version
        {
            get { return _Version; }
            set { Set(ref _Version, value); }
        }
        #endregion
        #region Page SelectedPage : Выбранная страница
        private Page _SelectedPage;
        /// <summary>выбранная страница</summary>
        public Page SelectedPage
        {
            get { return _SelectedPage; }
            set { Set(ref _SelectedPage, value); }
        }
        #endregion
        #region double Width : ширина окна
        private double _Width = 1200;
        /// <summary>ширина окна</summary>
        public double Width
        {
            get { return _Width; }
            set { Set(ref _Width, value); }
        }
        #endregion
        #region double Height : высота окна
        private double _Height = 700;
        /// <summary>высота окна</summary>
        public double Height
        {
            get { return _Height; }
            set { Set(ref _Height, value); }
        }
        #endregion
        #region double Left : summury
        private double _Left;
        /// <summary>summury</summary>
        public double Left
        {
            get { return _Left; }
            set { Set(ref _Left, value); }
        }
        #endregion
        #region double Top : summury
        private double _Top;
        /// <summary>summury</summary>
        public double Top
        {
            get { return _Top; }
            set { Set(ref _Top, value); }
        }


        #endregion
        #region double Opacity : прозрачность окна
        private double _Opacity = 1;
        /// <summary>прозрачность окна</summary>
        public double Opacity
        {
            get { return _Opacity; }
            set { Set(ref _Opacity, value); }
        }
        #endregion

        #region bool SongEnable : включены ли звуки
        private bool _SongEnable;
        /// <summary>включены ли звуки</summary>
        public bool SongEnable
        {
            get { return _SongEnable; }
            set
            {
                if (Set(ref _SongEnable, value))
                {
                    var conf = App.AppConfig;
                    conf.AppConfigJson.Song = value;
                    if (value)
                    {
                        SongText = "";
                        MediaPlayer.Play();
                    }
                    else
                    {
                        SongText = "";
                        MediaPlayer.Pause();
                    }
                }
            }
        }
        #endregion
        #region MediaPlayer MediaPlayer : воспроизводит звуки
        private MediaPlayer _MediaPlayer = new MediaPlayer();
        /// <summary>воспроизводит звуки</summary>
        public MediaPlayer MediaPlayer
        {
            get { return _MediaPlayer; }
            set { Set(ref _MediaPlayer, value); }
        }
        #endregion
        #region string SongText : текст кнопки
        private string _SongText;
        /// <summary>текст кнопки</summary>
        public string SongText
        {
            get { return _SongText; }
            set { Set(ref _SongText, value); }
        }
        #endregion

        #region Visibility ControlEnable : сокрытие контролов
        private Visibility _ControlEnable;
        /// <summary>сокрытие контролов</summary>
        public Visibility ControlEnable
        {
            get { return _ControlEnable; }
            set { Set(ref _ControlEnable, value); }
        }
        #endregion

        #region bool FullScreen : summury
        private bool _FullScreen;
        /// <summary>summury</summary>
        public bool FullScreen
        {
            get { return _FullScreen; }
            set
            {
                if (Set(ref _FullScreen, value))
                {
                    var conf = App.AppConfig;
                    conf.AppConfigJson.FullScreen = value;
                    if (value)
                    {
                        App.Current.MainWindow.WindowState = WindowState.Maximized;
                        ControlEnable = Visibility.Hidden;
                    }
                    else
                    {
                        ControlEnable = Visibility.Visible;
                        App.Current.MainWindow.WindowState = WindowState.Normal;
                        Width = conf.AppConfigJson.Width;
                        Height = conf.AppConfigJson.Height;
                        CenterWindowOnScreen();
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Commands
        #region CloseAppCommand : Закрытие порграммы
        /// <summary>Закрытие порграммы</summary>
        public ICommand CloseAppCommand { get; }
        private bool CanCloseAppCommandExecute(object p) => true;
        private void OnCloseAppCommandExecuted(object p)
        {
            App.Host.Services.GetRequiredService<GamePageViewModel>().LogoutCommand.Execute(null);
            Application.Current.Shutdown(0);
        }
        #endregion
        #region HideAppCommand : Свернуть программу
        /// <summary>Свернуть программу</summary>
        public ICommand HideAppCommand { get; }
        private bool CanHideAppCommandExecute(object p) => true;
        private void OnHideAppCommandExecuted(object p)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        #endregion
        #region AboutCommand : о программе
        /// <summary>о программе</summary>
        public ICommand AboutCommand { get; }
        private bool CanAboutCommandExecute(object p) => true;
        private void OnAboutCommandExecuted(object p)
        {
            string text = "Сборка: " + Assembly.GetExecutingAssembly().GetName().ProcessorArchitecture;
            text += "\n" + "Разработано в рамках WG Forge:";
            text += "\n" + "@mrpetrashevitch";
            text += "\n" + "@Artyom-Master";
            text += "\n" + "@banany2001";
            MessageBox.Show(text, "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
        #region EnableSongCommand : включить звуки
        /// <summary>включить звуки</summary>
        public ICommand EnableSongCommand { get; }
        private bool CanEnableSongCommandExecute(object p) => true;
        private void OnEnableSongCommandExecuted(object p)
        {
            SongEnable = !SongEnable;
        }
        #endregion

        #endregion

        public void SelectGamePage()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var page = App.Host.Services.GetRequiredService<LoadPageViewModel>();
                SelectedPage = App.Host.Services.GetRequiredService<GamePage>();
            }));
        }

        public void SelectLoadPage()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var page = App.Host.Services.GetRequiredService<LoadPageViewModel>();
                page.IsLoadVisible = Visibility.Hidden;
                page.IsControlVisible = Visibility.Visible;
                SelectedPage = App.Host.Services.GetRequiredService<LoadPage>();
            }));
        }

        public void SelectEscapePage(Page page)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedPage = page;
            }));
        }

        public void ShowEscapeMenu(EscapeCommands comm)
        {
            var p = App.Host.Services.GetRequiredService<EscapeMenuPage>();
            var vm = (EscapeMenuPageViewModel)p.DataContext;
            vm.Load(SelectedPage, comm);
            SelectPage(p);
        }

        public void SelectPage(Page page)
        {
            if (page.DataContext is ViewModel.LoadPageViewModel vml)
            {
                SelectLoadPage();
                return;
            }
            if (page.DataContext is ViewModel.GamePageViewModel vmg)
            {
                SelectGamePage();
                return;
            }
            if (page.DataContext is ViewModel.EscapeMenuPageViewModel vme)
            {
                SelectEscapePage(page);
                return;
            }
        }

        public void KeyPress(KeyEventArgs e)
        {
            if (SelectedPage is LoadPage && SelectedPage.DataContext is LoadPageViewModel vml)
                vml.KeyPress(e.Key);

            if (SelectedPage is GamePage && SelectedPage.DataContext is GamePageViewModel vmg)
                vmg.KeyPress(e.Key);

            if (SelectedPage is EscapeMenuPage && SelectedPage.DataContext is EscapeMenuPageViewModel vme)
                vme.KeyPress(e.Key);
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = Width;
            double windowHeight = Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void MediaPlayerMediaEnded(object sender, EventArgs e)
        {
            MediaPlayer.Position = TimeSpan.FromMilliseconds(1);
        }

        public MainWindowViewModel()
        {
            #region Properties
            Core = App.Core;
            MediaPlayer.Open(new Uri("Resources/Songs/main_theme.mp3", UriKind.Relative));
            MediaPlayer.MediaEnded += MediaPlayerMediaEnded;
            Title = "World of Tanks: Strategy";
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SelectedPage = App.Host.Services.GetRequiredService<LoadPage>();
            #endregion

            #region Commands
            CloseAppCommand = new LambdaCommand(OnCloseAppCommandExecuted, CanCloseAppCommandExecute);
            HideAppCommand = new LambdaCommand(OnHideAppCommandExecuted, CanHideAppCommandExecute);
            AboutCommand = new LambdaCommand(OnAboutCommandExecuted, CanAboutCommandExecute);
            EnableSongCommand = new LambdaCommand(OnEnableSongCommandExecuted, CanEnableSongCommandExecute);
            #endregion

            //load config
            var conf = App.AppConfig;
            SongEnable = conf.AppConfigJson.Song;

            Width = conf.AppConfigJson.Width;
            Height = conf.AppConfigJson.Height;
            CenterWindowOnScreen();
            FullScreen = conf.AppConfigJson.FullScreen;

            App.Host.Services.GetRequiredService<GamePage>();
        }
    }
}

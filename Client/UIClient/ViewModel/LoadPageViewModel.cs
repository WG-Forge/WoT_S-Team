using UIClient.Infrastructure.Command;
using UIClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using UIClient.View.Pages;
using UIClient.Model.Client;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace UIClient.ViewModel
{
    class LoadPageViewModel : Base.ViewModelBase
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

        #region string UserName : имя пользователя
        private string _UserName;
        /// <summary>имя пользователя</summary>
        public string UserName
        {
            get { return _UserName; }
            set { Set(ref _UserName, value); }
        }
        #endregion

        #region string Pass : пароль
        private string _Pass = "";
        /// <summary>пароль</summary>
        public string Pass
        {
            get { return _Pass; }
            set { Set(ref _Pass, value); }
        }
        #endregion

        #region string GameName : имя игры
        private string _GameName;
        /// <summary>имя игры</summary>
        public string GameName
        {
            get { return _GameName; }
            set { Set(ref _GameName, value); }
        }
        #endregion

        #region int PlayersMax : максимальное количество игроков
        private int _PlayersMax;
        /// <summary>максимальное количество игроков</summary>
        public int PlayersMax
        {
            get { return _PlayersMax; }
            set { Set(ref _PlayersMax, value); }
        }
        #endregion

        #region int? TurnMax : максимальное количество раундов
        private int? _TurnMax;
        /// <summary>максимальное количество раундов</summary>
        public int? TurnMax
        {
            get { return _TurnMax; }
            set { Set(ref _TurnMax, value); }
        }
        #endregion

        #region bool IsObserver : наблюдать
        private bool _IsObserver;
        /// <summary>наблюдать</summary>
        public bool IsObserver
        {
            get { return _IsObserver; }
            set { Set(ref _IsObserver, value); }
        }
        #endregion

        #region Visibility IsLoadVisible : summury
        private Visibility _IsLoadVisible = Visibility.Hidden;
        /// <summary>summury</summary>
        public Visibility IsLoadVisible
        {
            get { return _IsLoadVisible; }
            set { Set(ref _IsLoadVisible, value); }
        }
        #endregion

        #region Visibility IsControlVisible : summury
        private Visibility _IsControlVisible;
        /// <summary>summury</summary>
        public Visibility IsControlVisible
        {
            get { return _IsControlVisible; }
            set { Set(ref _IsControlVisible, value); }
        }
        #endregion

        #region ImageSource Image : путь
        private ImageSource _Image;
        /// <summary>путь</summary>
        public ImageSource Image
        {
            get { return _Image; }
            set { Set(ref _Image, value); }
        }
        #endregion

        //..
        #endregion

        #region Commands
        #region LoginCommand : войти
        /// <summary>войти</summary>
        public ICommand LoginCommand { get; }
        private bool CanLoginCommandExecute(object p)
        {
            if (UserName == null || UserName.Length < 4) return false;
            if (GameName != null && GameName.Length == 0) return false;
            if (PlayersMax < 1 || PlayersMax > 3) return false;
            if (TurnMax != null && TurnMax < 1) return false;
            return true;
        }
        private async void OnLoginCommandExecuted(object p)
        {
            var vm = App.Host.Services.GetRequiredService<GamePageViewModel>();
            LoginCreate log = new LoginCreate();
            log.name = UserName;
            log.password = Pass;
            log.game = GameName;
            log.num_turns = TurnMax;
            log.num_players = PlayersMax;
            log.is_observer = IsObserver;

            IsControlVisible = Visibility.Hidden;
            IsLoadVisible = Visibility.Visible;

            await vm.Core.ConnectAsync().ConfigureAwait(false);

            if (vm.Core.Connected)
                await vm.Field.RunGame(log);

            IsControlVisible = Visibility.Visible;
            IsLoadVisible = Visibility.Hidden;

            if(App.AppConfig.ExitEnd)
            {
                var mwvm = App.Host.Services.GetRequiredService<MainWindowViewModel>();
                mwvm.CloseAppCommand.Execute(null);
            }
        }
        #endregion

        //..
        #endregion

        public void KeyPress(Key key)
        {
            var wind_vm = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            switch (key)
            {
                case Key.Enter: // login
                    if (CanLoginCommandExecute(null))
                        OnLoginCommandExecuted(null);
                    break;
                case Key.Escape:
                    wind_vm.ShowEscapeMenu(EscapeCommands.Exit);
                    break;
                default:
                    break;
            }
        }

        public LoadPageViewModel()
        {
            #region Properties
            Image = new BitmapImage(new Uri("Resources/Images/logo.jpg", UriKind.Relative));
            Core = App.Core;
            PlayersMax = 1;
            //..
            #endregion

            #region Commands
            LoginCommand = new LambdaCommand(OnLoginCommandExecuted, CanLoginCommandExecute);
            //..
            #endregion
        }
    }
}

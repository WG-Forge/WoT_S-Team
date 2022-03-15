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
using System.IO;

namespace UIClient.ViewModel
{
    public enum EscapeCommands
    {
        Exit,
        LogOut,
        Menu,
        Settings
    }

    class EscapeMenuPageViewModel : Base.ViewModelBase
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

        #region int SelectedTab : выбранное меню
        private int _SelectedTab = -1;
        /// <summary>выбранное меню</summary>
        public int SelectedTab
        {
            get { return _SelectedTab; }
            set { Set(ref _SelectedTab, value); }
        }
        #endregion

        #region string Message : сообщение
        private string _Message;
        /// <summary>сообщение</summary>
        public string Message
        {
            get { return _Message; }
            set { Set(ref _Message, value); }
        }
        #endregion

        #region string ImagePath : summury
        private string _ImagePath;
        /// <summary>summury</summary>
        public string ImagePath
        {
            get { return _ImagePath; }
            set { Set(ref _ImagePath, value); }
        }
        #endregion



        #region MainWindowViewModel VM : summury
        private MainWindowViewModel _VM;
        /// <summary>summury</summary>
        public MainWindowViewModel VM
        {
            get { return _VM; }
            set { Set(ref _VM, value); }
        }
        #endregion

        #endregion

        #region Commands

        #region YesCommand : да
        /// <summary>да</summary>
        public ICommand YesCommand { get; }
        private bool CanYesCommandExecute(object p) => true;
        private void OnYesCommandExecuted(object p)
        {
            switch (_comm)
            {
                case EscapeCommands.Exit:
                    VM.CloseAppCommand.Execute(null);
                    break;
                case EscapeCommands.LogOut:
                    var gpvm = App.Host.Services.GetRequiredService<GamePageViewModel>();
                    gpvm.LogoutCommand.Execute(null);
                    break;
                default:
                    break;
            }
        }
        #endregion


        #region NoCommand : нет
        /// <summary>нет</summary>
        public ICommand NoCommand { get; }
        private bool CanNoCommandExecute(object p) => true;
        private void OnNoCommandExecuted(object p)
        {
            switch (_comm)
            {
                case EscapeCommands.Exit:
                case EscapeCommands.LogOut:
                    VM.SelectPage(_curr_page);
                    break;
                default:
                    break;
            }
        }
        #endregion


        #region LogoutCommand : покинуть бой
        /// <summary>покинуть бой</summary>
        public ICommand LogoutCommand { get; }
        private bool CanLogoutCommandExecute(object p) => true;
        private void OnLogoutCommandExecuted(object p)
        {
            VM.ShowEscapeMenu(EscapeCommands.LogOut);
        }
        #endregion


        #region ExitGameCommand : выйти из игры
        /// <summary>выйти из игры</summary>
        public ICommand ExitGameCommand { get; }
        private bool CanExitGameCommandExecute(object p) => true;
        private void OnExitGameCommandExecuted(object p)
        {
            VM.ShowEscapeMenu(EscapeCommands.Exit);
        }
        #endregion


        #region SettingsCommand : настройки
        /// <summary>настройки</summary>
        public ICommand SettingsCommand { get; }
        private bool CanSettingsCommandExecute(object p) => true;
        private void OnSettingsCommandExecuted(object p)
        {
            VM.ShowEscapeMenu(EscapeCommands.Settings);
        }
        #endregion


        //..
        #endregion

        public void KeyPress(Key key)
        {
            switch (key)
            {
                case Key.Escape:
                    VM.SelectPage(_curr_page);
                    break;
                default:
                    break;
            }
        }

        public void Load(Page page, EscapeCommands comm)
        {
            _curr_page = page;
            _comm = comm;

            if (_comm == EscapeCommands.Exit)
            {
                Message = "Вы действительно хотите выйти из игры?";
                SelectedTab = 0;
            }
            if (_comm == EscapeCommands.LogOut)
            {
                Message = "Вы действительно хотите покинуть бой?";
                SelectedTab = 0;
            }
            if (_comm == EscapeCommands.Menu)
            {
                Message = "";
                SelectedTab = 1;
            }
            if (_comm == EscapeCommands.Settings)
            {
                Message = "";
                SelectedTab = 2;
            }
        }

        private Page _curr_page;
        private EscapeCommands _comm;
        public EscapeMenuPageViewModel(MainWindowViewModel vm)
        {
            #region Properties
            VM = vm;
            ImagePath = Directory.GetCurrentDirectory() + "\\Resources\\Images\\back_menu.jpg";
            Core = App.Core;

            //..
            #endregion

            #region Commands
            YesCommand = new LambdaCommand(OnYesCommandExecuted, CanYesCommandExecute);
            NoCommand = new LambdaCommand(OnNoCommandExecuted, CanNoCommandExecute);
            LogoutCommand = new LambdaCommand(OnLogoutCommandExecuted, CanLogoutCommandExecute);
            ExitGameCommand = new LambdaCommand(OnExitGameCommandExecuted, CanExitGameCommandExecute);
            SettingsCommand = new LambdaCommand(OnSettingsCommandExecuted, CanSettingsCommandExecute);
            //..
            #endregion
        }
    }
}

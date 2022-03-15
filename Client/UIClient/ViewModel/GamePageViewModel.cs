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
using UIClient.Model.Config;
using UIClient.Infrastructure.Command.Base;
using System.Runtime.InteropServices;
using UIClient.Infrastructure.Controls;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Net;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using UIClient.Model.Server;
using System.Windows.Media;
using UIClient.Model.Client;
using UIClient.Model.Client.Api;
using System.Windows.Media.Imaging;

namespace UIClient.ViewModel
{
    public class GamePageViewModel : Base.ViewModelBase
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
        #region string ChatText : вписанный текст
        private string _ChatText;
        /// <summary>вписанный текст</summary>
        public string ChatText
        {
            get { return _ChatText; }
            set { Set(ref _ChatText, value); }
        }
        #endregion
        #region HexField Field : игорвое поле
        private HexField _Field;
        /// <summary>игорвое поле</summary>
        public HexField Field
        {
            get { return _Field; }
            set { Set(ref _Field, value); }
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
        #endregion

        #region Commands
        #region LogoutCommand : выйти из сессии
        /// <summary>выйти из сессии</summary>
        public ICommand LogoutCommand { get; }
        private bool CanLogoutCommandExecute(object p) => Core.Connected;
        private async void OnLogoutCommandExecuted(object p)
        {
            var main_page = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            main_page.SelectLoadPage();
            await Field.LogoutAsync();
            Reset();
        }
        #endregion
        #region SendChatMessageCommand : отправить сообщение в чат
        /// <summary>отправить сообщение в чат</summary>
        public ICommand SendChatMessageCommand { get; }
        private bool CanSendChatMessageCommandExecute(object p) => Core.Connected && ChatText?.Length > 0 && Field.StepEnable;
        private async void OnSendChatMessageCommandExecuted(object p)
        {
            if (await Field.SendChatMessageAsync(ChatText))
                ChatText = "";
        }
        #endregion
        #region TurnCommand : переключить ход
        /// <summary>переключить ход</summary>
        public ICommand TurnCommand { get; }
        private bool CanTurnCommandExecute(object p) => Field.StepEnable;
        private void OnTurnCommandExecuted(object p)
        {
            Field.TurnNextAsync();
        }
        #endregion
        #endregion

        public void KeyPress(Key key)
        {
            var wind_vm = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            switch (key)
            {
                case Key.Escape:
                    wind_vm.ShowEscapeMenu(EscapeCommands.Menu);
                    break;
                default:
                    break;
            }
        }

        public GamePageViewModel()
        {
            #region Properties
            Image = new BitmapImage(new Uri("Resources/Images/back.jpg", UriKind.Relative));
            Core = App.Core;
            Reset();
            #endregion

            #region Commands
            LogoutCommand = new LambdaCommand(OnLogoutCommandExecuted, CanLogoutCommandExecute);
            SendChatMessageCommand = new LambdaCommand(OnSendChatMessageCommandExecuted, CanSendChatMessageCommandExecute);
            TurnCommand = new LambdaCommand(OnTurnCommandExecuted, CanTurnCommandExecute);
            #endregion
        }

        public void Reset()
        {
            Field = new HexField();
        }
    }
}

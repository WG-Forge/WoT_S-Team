using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UIClient.Infrastructure.Command;
using UIClient.Infrastructure.Command.Base;
using UIClient.Infrastructure.Controls;
using UIClient.Model.Client;
using UIClient.Model.Client.Api;
using UIClient.Model.Config;
using UIClient.Model.Server;
using UIClient.View.Pages;
using UIClient.ViewModel;

namespace UIClient.Model
{
    public enum WebAction
    {
        LOGIN = 1,
        LOGOUT = 2,
        MAP = 3,
        GAME_STATE = 4,
        GAME_ACTIONS = 5,
        TURN = 6,
        CHAT = 100,
        MOVE = 101,
        SHOOT = 102,
    }

    public enum Result : int
    {
        OKEY = 0,
        BAD_COMMAND = 1,
        ACCESS_DENIED = 2,
        INAPPROPRIATE_GAME_STATE = 3,
        TIMEOUT = 4,
        INTERNAL_SERVER_ERROR = 500,

        // for client
        ERROR_WSA_INIT,
        ERROR_SOCKET,
        ERROR_CONNECT,
        CONNECTED_FALSE,
        ERROR_SEND,
        ERROR_RECV,
        IVALID_PARAM,
    };

    public class Core : ViewModel.Base.ViewModelBase
    {
        #region bool Connected : подключен ли к серверу
        private bool _Connected;
        /// <summary>подключен ли к серверу</summary>
        public bool Connected
        {
            get { return _Connected; }
            set { Set(ref _Connected, value); }
        }
        #endregion
        #region ObservableCollection<string> Logs : програмные логи
        private ObservableCollection<string> _Logs = new ObservableCollection<string>();
        /// <summary>програмные логи</summary>
        public ObservableCollection<string> Logs
        {
            get { return _Logs; }
            set { Set(ref _Logs, value); }
        }
        public void Log(string str)
        {
            Logs.Insert(0, str);
        }
        #endregion

        IntPtr web = IntPtr.Zero;
        IntPtr ai = IntPtr.Zero;
        object locker = new object();
        public Core()
        {
            web = WebClientDll.create_wc();
            if (web == IntPtr.Zero) throw new Exception("Error create web!");
            Log("Web ядро создано");

            ai = AIDll.create_ai();
            if (ai == IntPtr.Zero) throw new Exception("Error create ai!");
            Log("AI ядро создано");

            Log("UI ядро создано");
        }

        ~Core()
        {
            if (web != IntPtr.Zero)
            {
                WebClientDll.detach(web);
                WebClientDll.destroy_wc(web);
            }
            if (ai != IntPtr.Zero) 
                AIDll.destroy_ai(ai);
        }

        public (Result, string) SendPacket<T>(WebAction action, T data, bool skip_data = false)
        {
            string json_str = "";

            if (!skip_data)
                json_str = JsonConvert.SerializeObject(data);

            byte[] size_msg = System.Text.Encoding.UTF8.GetBytes(json_str);
            GCHandle pinned_msg = GCHandle.Alloc(size_msg, GCHandleType.Pinned);
            IntPtr pointer_msg = pinned_msg.AddrOfPinnedObject();
            byte[] buffer = new byte[4096];
            GCHandle pinned_buffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pointer_buffer = pinned_buffer.AddrOfPinnedObject();
            byte[] out_size = new byte[4];
            GCHandle pinned_out_size = GCHandle.Alloc(out_size, GCHandleType.Pinned);
            IntPtr pointer_out_size = pinned_out_size.AddrOfPinnedObject();

            Result res;
            string message = "";
            lock (locker)
            {
                if (skip_data)
                    res = WebClientDll.send_packet(web, action, 0, IntPtr.Zero, pointer_out_size, pointer_buffer);
                else
                    res = WebClientDll.send_packet(web, action, size_msg.Length, pointer_msg, pointer_out_size, pointer_buffer);

                message = Encoding.UTF8.GetString(buffer, 0, BitConverter.ToInt32(out_size, 0));
            }
            pinned_msg.Free();
            pinned_out_size.Free();
            pinned_buffer.Free();
            return (res, message);
        }

        public async Task<(Result, Player)> SendLoginAsync(LoginCreate log)
        {
            if (!Connected) return (Result.CONNECTED_FALSE, null);
            var res = await Task.Run(() => SendPacket(WebAction.LOGIN, log)).ConfigureAwait(false);
            return (res.Item1, JsonConvert.DeserializeObject<Player>(res.Item2));
        }

        public async Task<Result> SendLogoutAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            var res = await Task.Run(() => SendPacket(WebAction.LOGOUT, 0, true)).ConfigureAwait(false);
            return (res.Item1);
        }

        public async Task<(Result, Map)> SendMapAsync()
        {
            if (!Connected) return (Result.CONNECTED_FALSE, null);
            var res = await Task.Run(() => SendPacket(WebAction.MAP, 0, true)).ConfigureAwait(false);
            return (res.Item1, JsonConvert.DeserializeObject<Map>(res.Item2));
        }

        public async Task<(Result, GameState)> SendGameStateAsync()
        {
            if (!Connected) return (Result.CONNECTED_FALSE, null);
            var res = await Task.Run(() => SendPacket(WebAction.GAME_STATE, 0, true)).ConfigureAwait(false);
            return (res.Item1, JsonConvert.DeserializeObject<GameState>(res.Item2));
        }

        public async Task<(Result, Actions)> SendActionsAsync()
        {
            if (!Connected) return (Result.CONNECTED_FALSE, null);
            var res = await Task.Run(() => SendPacket(WebAction.GAME_ACTIONS, 0, true)).ConfigureAwait(false);
            return (res.Item1, JsonConvert.DeserializeObject<Actions>(res.Item2));
        }

        public async Task<Result> SendTurnAsync()
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            var res = await Task.Run(() => SendPacket(WebAction.TURN, 0, true)).ConfigureAwait(false);
            return res.Item1;
        }

        public async Task<Result> SendMoveAsync(int id, Point3 point)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ActionMove move = new ActionMove();
            move.target = point;
            move.vehicle_id = id;
            var res = await Task.Run(() => SendPacket(WebAction.MOVE, move)).ConfigureAwait(false);
            return res.Item1;
        }

        public async Task<Result> SendChatAsync(string msg)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ChatMessage msgs = new ChatMessage();
            msgs.message = msg;
            var res = await Task.Run(() => SendPacket(WebAction.CHAT, msgs)).ConfigureAwait(false);
            return res.Item1;
        }

        public async Task<Result> SendShootAsync(int id, Point3 point)
        {
            if (!Connected) return Result.CONNECTED_FALSE;
            ActionShoot shoot = new ActionShoot();
            shoot.target = point;
            shoot.vehicle_id = id;
            var res = await Task.Run(() => SendPacket(WebAction.SHOOT, shoot)).ConfigureAwait(false);
            return (res.Item1);
        }

        public async Task ConnectAsync()
        {
            if (Connected) return;
            try
            {
                Log("Подключение к серверу...");
                var config = App.AppConfig;

                IPAddress[] addresslist = Dns.GetHostAddresses(config.AppConfigJson.HostName);
#pragma warning disable CS0618 // Тип или член устарел
                await Task.Run(() =>
                {
                    WebClientDll.connect_(web, (uint)addresslist[0].Address, config.AppConfigJson.Port);
                });
#pragma warning restore CS0618 // Тип или член устарел

                Connected = true;
                Log("Подключен к серверу");
            }
            catch
            {
                Connected = false;
                Log("Ошибка подключения к серверу");
            }
        }

        public async Task DetachAsync()
        {
            if (!Connected) return;
            Log("Отключение от сервера...");
            await Task.Run(() =>
            {
                WebClientDll.detach(web);
            });
            Connected = false;
            Log("Отключен от сервера");
        }

        public action_ret GetAIActions(int player_id, GameState GameState, Map Map)
        {
            IntPtr player_s_ptr = IntPtr.Zero;
            GCHandle player_pipe = default(GCHandle);
            IntPtr vehicle_s_ptr = IntPtr.Zero;
            GCHandle vehicle_pipe = default(GCHandle);
            IntPtr winpoints_s_ptr = IntPtr.Zero;
            GCHandle winpoints_pipe = default(GCHandle);
            IntPtr attackmatrix_s_ptr = IntPtr.Zero;
            GCHandle attackmatrix_pipe = default(GCHandle);
            IntPtr base_s_ptr = IntPtr.Zero;
            GCHandle base_pipe = default(GCHandle);
            IntPtr obstacle_s_ptr = IntPtr.Zero;
            GCHandle obstacle_pipe = default(GCHandle);
            IntPtr light_repair_s_ptr = IntPtr.Zero;
            GCHandle light_repair_pipe = default(GCHandle);
            IntPtr hard_repair_s_ptr = IntPtr.Zero;
            GCHandle hard_repair_pipe = default(GCHandle);
            IntPtr catapult_s_ptr = IntPtr.Zero;
            GCHandle catapult_pipe = default(GCHandle);
            IntPtr catapult_usage_s_ptr = IntPtr.Zero;
            GCHandle catapult_usage_pipe = default(GCHandle);

            player_native[] player_s = null;
            if (GameState.players != null)
            {
                player_s = new player_native[GameState.players.Length];
                for (int i = 0; i < GameState.players.Length; i++)
                {
                    player_s[i].idx = GameState.players[i].idx;
                    player_s[i].is_observer = GameState.players[i].is_observer ? 1 : 0;
                }
            }
            vehicle_native[] vehicle_s = null;
            if (GameState.vehicles.Count > 0)
            {
                vehicle_s = new vehicle_native[GameState.vehicles.Count];
                int i = 0;
                foreach (var item in GameState.vehicles)
                {
                    vehicle_s[i].vehicle_id = item.Key;
                    vehicle_s[i].capture_points = item.Value.capture_points;
                    vehicle_s[i].health = item.Value.health;
                    vehicle_s[i].player_id = item.Value.player_id;
                    vehicle_s[i].position.x = item.Value.position.x;
                    vehicle_s[i].position.y = item.Value.position.y;
                    vehicle_s[i].position.z = item.Value.position.z;
                    vehicle_s[i].spawn_position.x = item.Value.spawn_position.x;
                    vehicle_s[i].spawn_position.y = item.Value.spawn_position.y;
                    vehicle_s[i].spawn_position.z = item.Value.spawn_position.z;
                    vehicle_s[i].vehicle_type = item.Value.vehicle_type;
                    vehicle_s[i].shoot_range_bonus = item.Value.shoot_range_bonus;
                    i++;
                }
            }
            win_points_native[] winpoints_s = null;
            if (GameState.win_points.Count > 0)
            {
                winpoints_s = new win_points_native[GameState.win_points.Count];
                int i = 0;
                foreach (var item in GameState.win_points)
                {
                    winpoints_s[i].id = item.Key;
                    winpoints_s[i].capture = item.Value.capture;
                    winpoints_s[i].kill = item.Value.kill;
                    i++;
                }
            }
            attack_matrix_native[] attackmatrix_s = null;
            if (GameState.attack_matrix.Count > 0)
            {
                attackmatrix_s = new attack_matrix_native[GameState.attack_matrix.Count];
                int i = 0;
                unsafe
                {
                    foreach (var item in GameState.attack_matrix)
                    {
                        int j = 0;
                        attackmatrix_s[i].id = item.Key;
                        foreach (var ind in item.Value)
                        {
                            attackmatrix_s[i].attack[j] = ind;
                            j++;
                        }
                        i++;
                    }
                }
            }
            point[] base_s = null;
            if (Map.content._base != null)
            {
                base_s = new point[Map.content._base.Length];
                int i = 0;
                foreach (var item in Map.content._base)
                {
                    base_s[i].x = item.x;
                    base_s[i].y = item.y;
                    base_s[i].z = item.z;
                    i++;
                }
            }
            point[] obstacle_s = null;
            if (Map.content.obstacle != null)
            {
                obstacle_s = new point[Map.content.obstacle.Length];
                int i = 0;
                foreach (var item in Map.content.obstacle)
                {
                    obstacle_s[i].x = item.x;
                    obstacle_s[i].y = item.y;
                    obstacle_s[i].z = item.z;
                    i++;
                }
            }
            point[] light_repair_s = null;
            if (Map.content.light_repair != null)
            {
                light_repair_s = new point[Map.content.light_repair.Length];
                int i = 0;
                foreach (var item in Map.content.light_repair)
                {
                    light_repair_s[i].x = item.x;
                    light_repair_s[i].y = item.y;
                    light_repair_s[i].z = item.z;
                    i++;
                }
            }
            point[] hard_repair_s = null;
            if (Map.content.hard_repair != null)
            {
                hard_repair_s = new point[Map.content.hard_repair.Length];
                int i = 0;
                foreach (var item in Map.content.hard_repair)
                {
                    hard_repair_s[i].x = item.x;
                    hard_repair_s[i].y = item.y;
                    hard_repair_s[i].z = item.z;
                    i++;
                }
            }
            point[] catapult_s = null;
            if (Map.content.catapult != null)
            {
                catapult_s = new point[Map.content.catapult.Length];
                int i = 0;
                foreach (var item in Map.content.catapult)
                {
                    catapult_s[i].x = item.x;
                    catapult_s[i].y = item.y;
                    catapult_s[i].z = item.z;
                    i++;
                }
            }
            point[] catapult_usage_s = null;
            if (GameState.catapult_usage != null)
            {
                catapult_usage_s = new point[GameState.catapult_usage.Length];
                int i = 0;
                foreach (var item in GameState.catapult_usage)
                {
                    catapult_usage_s[i].x = item.x;
                    catapult_usage_s[i].y = item.y;
                    catapult_usage_s[i].z = item.z;
                    i++;
                }
            }

            if (player_s != null)
            {
                player_pipe = GCHandle.Alloc(player_s, GCHandleType.Pinned);
                player_s_ptr = player_pipe.AddrOfPinnedObject();
            }
            if (vehicle_s != null)
            {
                vehicle_pipe = GCHandle.Alloc(vehicle_s, GCHandleType.Pinned);
                vehicle_s_ptr = vehicle_pipe.AddrOfPinnedObject();
            }
            if (winpoints_s != null)
            {
                winpoints_pipe = GCHandle.Alloc(winpoints_s, GCHandleType.Pinned);
                winpoints_s_ptr = winpoints_pipe.AddrOfPinnedObject();
            }
            if (attackmatrix_s != null)
            {
                attackmatrix_pipe = GCHandle.Alloc(attackmatrix_s, GCHandleType.Pinned);
                attackmatrix_s_ptr = attackmatrix_pipe.AddrOfPinnedObject();
            }
            if (base_s != null)
            {
                base_pipe = GCHandle.Alloc(base_s, GCHandleType.Pinned);
                base_s_ptr = base_pipe.AddrOfPinnedObject();
            }
            if (obstacle_s != null)
            {
                obstacle_pipe = GCHandle.Alloc(obstacle_s, GCHandleType.Pinned);
                obstacle_s_ptr = obstacle_pipe.AddrOfPinnedObject();
            }
            if (light_repair_s != null)
            {
                light_repair_pipe = GCHandle.Alloc(light_repair_s, GCHandleType.Pinned);
                light_repair_s_ptr = light_repair_pipe.AddrOfPinnedObject();
            }
            if (hard_repair_s != null)
            {
                hard_repair_pipe = GCHandle.Alloc(hard_repair_s, GCHandleType.Pinned);
                hard_repair_s_ptr = hard_repair_pipe.AddrOfPinnedObject();
            }
            if (catapult_s != null)
            {
                catapult_pipe = GCHandle.Alloc(catapult_s, GCHandleType.Pinned);
                catapult_s_ptr = catapult_pipe.AddrOfPinnedObject();
            }
            if (catapult_usage_s != null)
            {
                catapult_usage_pipe = GCHandle.Alloc(catapult_usage_s, GCHandleType.Pinned);
                catapult_usage_s_ptr = catapult_usage_pipe.AddrOfPinnedObject();
            }

            action_ret action_re;
            var act = AIDll.get_action(ai, player_id,
                player_s_ptr, player_s?.Length ?? 0,
                vehicle_s_ptr, vehicle_s?.Length ?? 0,
                winpoints_s_ptr, winpoints_s?.Length ?? 0,
                attackmatrix_s_ptr, attackmatrix_s?.Length ?? 0,
                base_s_ptr, base_s?.Length ?? 0,
                obstacle_s_ptr, obstacle_s?.Length ?? 0,
                light_repair_s_ptr, light_repair_s?.Length ?? 0,
                hard_repair_s_ptr, hard_repair_s?.Length ?? 0,
                catapult_s_ptr, catapult_s?.Length ?? 0,
                catapult_usage_s_ptr, catapult_usage_s?.Length ?? 0,
                out action_re);

            if (player_s != null) player_pipe.Free();
            if (vehicle_s != null) vehicle_pipe.Free();
            if (winpoints_s != null) winpoints_pipe.Free();
            if (attackmatrix_s != null) attackmatrix_pipe.Free();
            if (base_s != null) base_pipe.Free();
            if (obstacle_s != null) obstacle_pipe.Free();
            if (light_repair_s != null) light_repair_pipe.Free();
            if (hard_repair_s != null) hard_repair_pipe.Free();
            if (catapult_s != null) catapult_pipe.Free();
            if (catapult_usage_s != null) catapult_usage_pipe.Free();

            return action_re;
        }
    }
}
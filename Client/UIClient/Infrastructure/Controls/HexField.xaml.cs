using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIClient.Infrastructure.Command;
using UIClient.Infrastructure.Command.Base;
using UIClient.Model;
using UIClient.Model.Client;
using UIClient.Model.Client.Api;
using UIClient.Model.Server;
using UIClient.ViewModel;

namespace UIClient.Infrastructure.Controls
{
    /// <summary>
    /// Логика взаимодействия для CellField.xaml
    /// </summary>
    public partial class HexField : UserControl, INotifyPropertyChanged
    {
        public bool Inited { get; private set; }
        public Dictionary<int, PlayerEx> Players { get; private set; }
        public async Task<bool> SendChatMessageAsync(string text)
        {
            if (!App.Core.Connected) return false;
            if (!StepEnable) return false;
            if (text.Length < 1) return false;
            var res = await App.Core.SendChatAsync(text).ConfigureAwait(false);
            if (res != Result.OKEY)
            { App.Core.Log("Ошибка: " + res.ToString()); return false; }
            return true;
        }
        public bool TurnNextAsync()
        {
            if (!App.Core.Connected) return false;
            if (!StepEnable) return false;
            _EventWait?.TrySetResult(true);
            return true;
        }
        public async Task<bool> LogoutAsync()
        {
            _game_over = true;
            _EventWait?.TrySetResult(true);
            if (!App.Core.Connected) return false;

            var res = await App.Core.SendLogoutAsync();
            if (res != Result.OKEY)
            {
                App.Core.Log("Ошибка: " + res.ToString());
                return false;
            }
            return true;
        }
        public async Task<bool> RunGame(LoginCreate login)
        {
            if (!await GetLogin(login).ConfigureAwait(false)) return false;
            if (!await GetMap().ConfigureAwait(false)) return false;
            if (!await WaitPlayers().ConfigureAwait(false)) return false;

            //game loop
            _game_over = false;
            while (!_game_over)
            {
                if (!await UpdateGameState()) break;
                if (!await GetActions()) break;

                if (AIEnable)
                {
                    if (GameState.current_player_idx == Player.CurrentPlayer.idx)
                    {
                        var actions = App.Core.GetAIActions(Player.CurrentPlayer.idx, GameState, Map);
                        foreach (var i in actions.actions)
                        {
                            Result result = Result.OKEY;
                            if (i.action_type == action_type.move)
                            {
                                result = await MoveAsync(i.vec_id, new Point3() { x = i.point.x, y = i.point.y, z = i.point.z }).ConfigureAwait(false);
                            }
                            else if (i.action_type == action_type.shoot)
                            {
                                result = await ShootAsync(i.vec_id, new Point3() { x = i.point.x, y = i.point.y, z = i.point.z }).ConfigureAwait(false);
                            }

                            if (result != Result.OKEY && result != Result.BAD_COMMAND)
                            {
                                App.Core.Log(result.ToString());
                                break;
                            }
                        }
                    }
                    await TurnAsync();
                }
                else
                {
                    if (GameState.current_player_idx == Player.CurrentPlayer.idx)
                        await Task.WhenAny(_EventWait.Task, Task.Delay(9000));
                    await TurnAsync();
                }
            }
            return true;
        }
        public async Task OnHexClick(Hex curr_hex, GamePageViewModel vm)
        {
            foreach (var item in _SelectedCanMove)
                item.CanMove = Visibility.Hidden;
            foreach (var item in _SelectedCanShoot)
                item.CanShoot = Visibility.Hidden;
            _SelectedCanMove.Clear();
            _SelectedCanShoot.Clear();

            Tank tank = (Tank)curr_hex.Tank;
            if (tank != null && tank.Vehicle.vehicle.player_id == Player.CurrentPlayer.idx)
            {
                _SelectedCanMove = GetCanMove(curr_hex, tank.Speed);
                foreach (var item in _SelectedCanMove)
                    item.CanMove = Visibility.Visible;

                _SelectedCanShoot = GetCanShoot(tank);
                foreach (var item in _SelectedCanShoot)
                    item.CanShoot = Visibility.Visible;
            }

            Hex last_hex = _SelectedHex;
            _SelectedHex = curr_hex;
            if (last_hex == null || last_hex.Tank == null)
                return;

            tank = last_hex.Tank;
            if (tank.Vehicle.vehicle.player_id != Player.CurrentPlayer.idx)
                return;

            Tank new_tank = (Tank)curr_hex.Tank;

            if (new_tank == null)
            {
                await MoveAsync(tank.Vehicle.id, curr_hex.Point3).ConfigureAwait(false);
            }
            else
            {
                if (new_tank.Vehicle.vehicle.player_id == Player.CurrentPlayer.idx)
                    return;

                if (tank.Vehicle.vehicle.vehicle_type == VehicleType.ПТ)
                {
                    Point3 point = GetShootPointPT(last_hex.Point3, curr_hex.Point3);
                    await ShootAsync(tank.Vehicle.id, point).ConfigureAwait(false);
                }
                else
                    await ShootAsync(tank.Vehicle.id, curr_hex.Point3).ConfigureAwait(false);
            }
            _SelectedHex = null;
        }

        #region ObservableCollection<string> Chat : логи чата
        private ObservableCollection<string> _Chat;
        /// <summary>логи чата</summary>
        public ObservableCollection<string> Chat
        {
            get { return _Chat; }
            private set { Set(ref _Chat, value); }
        }
        #endregion
        #region bool StepEnable : есть ли возможность ходить
        private bool _StepEnable;
        /// <summary>есть ли возможность ходить</summary>
        public bool StepEnable
        {
            get { return _StepEnable; }
            private set
            {
                if (Set(ref _StepEnable, value))
                {
                    if (value) MessageWaitVisible = Visibility.Collapsed;
                    else MessageWaitVisible = Visibility.Visible;
                }
            }
        }
        #endregion
        #region string MessageWait : сообщение ожидания
        private string _MessageWait;
        /// <summary>сообщение ожидания</summary>
        public string MessageWait
        {
            get { return _MessageWait; }
            private set { Set(ref _MessageWait, value); }
        }
        #endregion
        #region Visibility MessageWaitVisible : надпись ожидания
        private Visibility _MessageWaitVisible;
        /// <summary>надпись ожидания</summary>
        public Visibility MessageWaitVisible
        {
            get { return _MessageWaitVisible; }
            private set { Set(ref _MessageWaitVisible, value); }
        }
        #endregion
        #region bool AIEnable : включить автоматическую игру
        private bool _AIEnable;
        /// <summary>включить автоматическую игру</summary>
        public bool AIEnable
        {
            get { return _AIEnable; }
            set { Set(ref _AIEnable, value); }
        }
        #endregion
        #region PlayerEx Player : текущий игрок
        private PlayerEx _Player;
        /// <summary>текущий игрок</summary>
        public PlayerEx Player
        {
            get { return _Player; }
            private set { Set(ref _Player, value); }
        }
        #endregion
        #region int PlayerCount : количество игроков
        private int _PlayerCount;
        /// <summary>количество игроков</summary>
        public int PlayerCount
        {
            get { return _PlayerCount; }
            private set { Set(ref _PlayerCount, value); }
        }
        #endregion
        #region Map Map : карта
        private Map _Map;
        /// <summary>карта</summary>
        public Map Map
        {
            get { return _Map; }
            private set { Set(ref _Map, value); }
        }
        #endregion
        #region GameState GameState : статус игры
        private GameState _GameState;
        /// <summary>статус игры</summary>
        public GameState GameState
        {
            get { return _GameState; }
            private set { Set(ref _GameState, value); }
        }
        #endregion
        #region double FieldSizeY : размер поля в пикмелях
        public double FieldSizeX
        {
            get { return (double)GetValue(FieldSizeXProperty); }
            private set { SetValue(FieldSizeXProperty, value); }
        }
        public static readonly DependencyProperty FieldSizeXProperty =
            DependencyProperty.Register(nameof(FieldSizeX), typeof(double), typeof(HexField), new PropertyMetadata(default(double)));
        #endregion
        #region double FieldSizeY : размер поля в пикмелях
        public double FieldSizeY
        {
            get { return (double)GetValue(FieldSizeYProperty); }
            private set { SetValue(FieldSizeYProperty, value); }
        }
        public static readonly DependencyProperty FieldSizeYProperty =
            DependencyProperty.Register(nameof(FieldSizeY), typeof(double), typeof(HexField), new PropertyMetadata(default(double)));
        #endregion

        public HexField()
        {
            InitializeComponent();
            _SelectedCanMove = new List<Hex>();
            _SelectedCanShoot = new List<Hex>();
            Chat = new ObservableCollection<string>();
            Players = new Dictionary<int, PlayerEx>();
            _Vehicles = new Dictionary<int, VehicleEx>();

            Player = new PlayerEx() { TeamColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xf8, 0x57, 0x06)) };
            Inited = false;
        }

        private bool _game_over = true;
        private static readonly Dictionary<int, Brush> _TeamColors = new Dictionary<int, Brush>()
        {
            { 0, Brushes.Aqua},
            { 1, Brushes.Red},
            { 2, Brushes.Lime}
        };
        private Hex[,] _HexField2D = null;
        private bool _Even = false;
        private int _FieldSize = 0;
        private int _CurrentX = 0, _CurrentY = 0;
        private Point2 _Offset2D = new Point2();
        private TaskCompletionSource<bool> _EventWait;
        private Dictionary<int, VehicleEx> _Vehicles;
        private Hex _SelectedHex;
        private List<Hex> _SelectedCanMove;
        private List<Hex> _SelectedCanShoot;

        private void CreateHexField(int size)
        {
            Canv.Children.Clear();
            _CurrentX = 0; _CurrentY = 0;
            _Offset2D.x = size - 1;
            _Offset2D.y = size - 1;
            FieldSizeX = (_FieldSize + 1) * Hex.size_x * 0.75 - Hex.size_y / 2;
            FieldSizeY = (_FieldSize) * Hex.size_y;

            int l = size - 2, r = size;
            int i = 0;

            for (; i < size / 2; i++)
            {
                for (int j = 0; j < _FieldSize; j++)
                    if (j >= l && j <= r) AddHex(Hex.HexType.Free);
                    else AddHex(Hex.HexType.Nun);
                l -= 2;
                r += 2;
            }

            if (_Even)
            {
                for (; i < size + size / 2 - 1; i++)
                {
                    for (int j = 0; j < _FieldSize; j++)
                        AddHex(Hex.HexType.Free);
                }
            }
            else
            {
                for (; i < size + size / 2; i++)
                {
                    for (int j = 0; j < _FieldSize; j++)
                        AddHex(Hex.HexType.Free);
                }
            }

            l += 1;
            r -= 1;
            for (; i < size + size - 1; i++)
            {
                l += 2;
                r -= 2;

                for (int j = 0; j < _FieldSize; j++)
                    if (j >= l && j <= r) AddHex(Hex.HexType.Free);
                    else AddHex(Hex.HexType.Nun);
            }
        }

        private Hex GetHex(Point3 p)
        {
            int of_x = _Offset2D.x + p.x;
            if (of_x < 0 || of_x > _FieldSize - 1) return null;
            int of_y = _Offset2D.y + (p.z + (p.x - (p.x & 1)) / 2);
            if (of_y < 0 || of_y > _FieldSize - 1) return null;
            return _HexField2D[of_y, of_x];
        }

        private int GetDistance(Point3 a, Point3 b)
        {
            return (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z)) / 2;
        }

        private List<Hex> GetHexAround(Point3 point, int n, int N)
        {
            List<Hex> res = new List<Hex>();
            if (n > N || n < 0 || N < 0) return res;

            Point3 hex = new Point3();
            for (int dx = -N; dx <= N; dx++)
            {
                for (int dy = -N; dy <= N; dy++)
                {
                    for (int dz = -N; dz <= N; dz++)
                    {
                        if (dx + dy + dz == 0)
                        {
                            hex.x = dx + point.x;
                            hex.y = dy + point.y;
                            hex.z = dz + point.z;
                            int d = GetDistance(point, hex);
                            if (d < n || d > N) continue;

                            var add_hex = GetHex(hex);
                            if (add_hex != null)
                                res.Add(add_hex);
                        }
                    }
                }
            }
            return res;
        }

        private List<Hex> GetCanShoot(Tank tank)
        {
            var hex = tank.Vehicle.hex;
            var list = new List<Hex>();

            if (tank.Vehicle.vehicle.vehicle_type == VehicleType.ПТ)
                list = GetCanShootPT(hex, tank.ShootMax + tank.Vehicle.vehicle.shoot_range_bonus);
            else
                list = GetHexAround(hex.Point3, tank.ShootMin, tank.ShootMax + tank.Vehicle.vehicle.shoot_range_bonus);
            list.RemoveAll(item => item.Tank == null || item.Tank.Vehicle.vehicle.player_id == Player.CurrentPlayer.idx);
            return list;
        }

        private List<Hex> GetCanShootPT(Hex hex, int shoot_max)
        {
            var set = new HashSet<Hex>();
            var list = new List<Hex>();
            list.Add(hex);

            for (int i = 1; i <= shoot_max; i++)
            {
                List<Hex> temp = new List<Hex>();
                foreach (var item in list)
                {
                    List<Hex> l = GetHexAround(item.Point3, 1, 1);
                    l.RemoveAll(item => item.Type == Hex.HexType.Rock || item.Type == Hex.HexType.Nun);
                    l.RemoveAll(item => item.Point3.x != hex.Point3.x && item.Point3.y != hex.Point3.y && item.Point3.z != hex.Point3.z);

                    foreach (var it in l)
                        if (set.Add(it))
                            temp.Add(it);
                }
                list = temp;
            }

            list = new List<Hex>(set.ToArray());
            return list;
        }

        private Point3 GetShootPointPT(Point3 tank, Point3 target)
        {
            List<Hex> around = GetHexAround(tank, 1, 1);
            around.RemoveAll(item => item.Point3.x != target.x && item.Point3.y != target.y && item.Point3.z != target.z);
            if (around.Count == 0) throw new Exception("Error GetShootPointPT");
            if (around.Count == 1) return around[0].Point3;
            if (GetDistance(around[0].Point3, target) < GetDistance(around[1].Point3, target)) return around[0].Point3;
            return around[1].Point3;
        }

        private List<Hex> GetCanMove(Hex hex, int speed)
        {
            var set = new HashSet<Hex>();
            var list = new List<Hex>();
            list.Add(hex);

            for (int i = 1; i <= speed; i++)
            {
                List<Hex> temp = new List<Hex>();
                foreach (var item in list)
                {
                    List<Hex> l = GetHexAround(item.Point3, 1, 1);
                    l.RemoveAll(item => item.Type == Hex.HexType.Rock || item.Type == Hex.HexType.Nun);

                    foreach (var it in l)
                        if (set.Add(it))
                            temp.Add(it);
                }
                list = temp;
            }

            list = new List<Hex>(set.ToArray());
            list.RemoveAll(item => item.Tank != null);
            return list;
        }

        private VehicleEx GetVehicle(Point3 p)
        {
            foreach (var item in _Vehicles)
            {
                var new_vec = item.Value.vehicle;
                if (new_vec.position.x == p.x &&
                    new_vec.position.y == p.y &&
                    new_vec.position.z == p.z)
                {
                    return item.Value;
                }
            }
            return null;
        }

        private Point2 GetPoint2(Point3 p)
        {
            Point2 ret = new Point2();
            ret.x = _Offset2D.x + p.x;
            ret.y = _Offset2D.y + (p.z + (p.x - (p.x & 1)) / 2);
            return ret;
        }

        private Point3 GetPoint3(Point2 p)
        {
            Point3 ret = new Point3();
            p.x -= _Offset2D.x;
            p.y -= _Offset2D.y;
            ret.x = p.x;
            ret.z = p.y - (p.x - (p.x & 1)) / 2;
            ret.y = -ret.x - ret.z;
            return ret;
        }

        private void CreateContent(GameState state)
        {
            if (state == null) return;
            if (state.players == null) return;
            PlayerCount = state.players.Length;

            this.Players.Clear();
            PlayerCount = 0;

            int i = 0;
            foreach (var item in state.players)
            {
                PlayerEx px = new PlayerEx();
                px.CurrentPlayer = item;
                px.TeamColor = _TeamColors[i];
                this.Players.Add(item.idx, px);
                i++;
                PlayerCount++;
            }

            _Vehicles.Clear();
            Dictionary<int, Vehicle> v = state.vehicles;
            if (v != null)
                foreach (var it in v)
                {
                    VehicleEx vx = new VehicleEx();
                    vx.vehicle = it.Value;
                    vx.hex = GetHex(vx.vehicle.position);
                    vx.id = it.Key;
                    Tank tank = new Tank(vx, Players[vx.vehicle.player_id].TeamColor);
                    vx.hex.Tank = tank;
                    _Vehicles.Add(it.Key, vx);
                }

            PlayerEx currentPlayer;
            if (Players.TryGetValue(Player.CurrentPlayer.idx, out currentPlayer))
                Player = currentPlayer;
            Inited = true;
        }

        private void UpdateContent(GameState state)
        {
            Dictionary<int, Vehicle> v = state.vehicles;
            if (v != null)
                foreach (var i in v)
                {
                    //if (i.Value.player_id == App.Core.Player.idx)
                    //    continue;

                    var new_vec = i.Value;
                    var curr_vec = _Vehicles[i.Key];

                    if (new_vec.health != curr_vec.vehicle.health)
                    {
                        curr_vec.vehicle.health = new_vec.health;
                        ((Tank)curr_vec.hex.Tank).HP = new_vec.health;
                    }
                    if (new_vec.position.x != curr_vec.hex.Point3.x ||
                        new_vec.position.y != curr_vec.hex.Point3.y ||
                        new_vec.position.z != curr_vec.hex.Point3.z)
                    {
                        VehicleMove(i.Key, new_vec.position);
                    }
                    if (new_vec.shoot_range_bonus != curr_vec.vehicle.shoot_range_bonus)
                    {
                        curr_vec.vehicle.shoot_range_bonus = new_vec.shoot_range_bonus;
                    }
                }
        }

        private void VehicleMove(int id, Point3 point)
        {
            Hex to = GetHex(point);
            VehicleEx v = _Vehicles[id];
            Tank tank = (Tank)v.hex.Tank;

            if (to == v.hex) return;
            if (tank == null) return;
            if (to.Tank != null) return;

            v.hex.Tank = null;
            to.Tank = tank;
            v.hex = to;
            v.vehicle.position = to.Point3;
        }

        private void VehicleShoot(int id, Point3 point)
        {
            var target = GetVehicle(point);
            if (target == null) return;
            if (target.hex.Tank == null) return;

            Tank tank = (Tank)target.hex.Tank;
            tank.HP--;
            target.vehicle.health--;
            if (tank.HP == 0)
            {
                tank.HP = 2;
                target.vehicle.health = 2;
                VehicleMove(target.id, target.vehicle.spawn_position);
            }
        }

        private void AddBase(Point3 p)
        {
            GetHex(p).Type = Hex.HexType.Base;
        }

        private void AddObstacle(Point3 p)
        {
            GetHex(p).Type = Hex.HexType.Rock;
        }

        private void AddSpawn(Point3 p)
        {
            GetHex(p).Type = Hex.HexType.Spawn;
        }

        private void AddCatapult(Point3 p)
        {
            GetHex(p).Type = Hex.HexType.Catapult;
        }

        private void AddHardRepair(Point3 p)
        {
            GetHex(p).Type = Hex.HexType.HardRepair;
        }

        private void AddLightRepair(Point3 p)
        {
            GetHex(p).Type = Hex.HexType.LightRepair;
        }

        private void CreateField(Map map)
        {
            if (map.size % 2 == 0) _Even = true;
            else _Even = false;
            _FieldSize = map.size * 2 - 1;
            _HexField2D = new Hex[_FieldSize, _FieldSize];
            CreateHexField(map.size);

            if (map.content._base != null)
                foreach (var point in map.content._base)
                    AddBase(point);

            if (map.content.obstacle != null)
                foreach (var point in map.content.obstacle)
                    AddObstacle(point);

            if (map.spawn_points != null)
            {
                foreach (var player in map.spawn_points)
                {
                    if (player.medium_tank != null)
                        foreach (var spawn_point in player.medium_tank)
                            AddSpawn(spawn_point);
                    if (player.light_tank != null)
                        foreach (var spawn_point in player.light_tank)
                            AddSpawn(spawn_point);
                    if (player.heavy_tank != null)
                        foreach (var spawn_point in player.heavy_tank)
                            AddSpawn(spawn_point);
                    if (player.at_spg != null)
                        foreach (var spawn_point in player.at_spg)
                            AddSpawn(spawn_point);
                    if (player.spg != null)
                        foreach (var spawn_point in player.spg)
                            AddSpawn(spawn_point);
                }
            }

            if (map.content.light_repair != null)
                foreach (var point in map.content.light_repair)
                    AddLightRepair(point);

            if (map.content.hard_repair != null)
                foreach (var point in map.content.hard_repair)
                    AddHardRepair(point);

            if (map.content.catapult != null)
                foreach (var point in map.content.catapult)
                    AddCatapult(point);
        }

        private void AddHex(Hex.HexType type)
        {
            Point2 point2 = new Point2() { x = _CurrentX, y = _CurrentY };
            Hex bt = new Hex(point2, GetPoint3(point2), type);

            Canvas.SetLeft(bt, _CurrentX * (Hex.size_x * 0.75));
            if (_Even)
            {
                if (_CurrentX % 2 != 0) Canvas.SetTop(bt, _CurrentY * Hex.size_y);
                else Canvas.SetTop(bt, _CurrentY * Hex.size_y + (Hex.size_y / 2));
            }
            else
            {
                if (_CurrentX % 2 == 0) Canvas.SetTop(bt, _CurrentY * Hex.size_y);
                else Canvas.SetTop(bt, _CurrentY * Hex.size_y + (Hex.size_y / 2));
            }
            Canv.Children.Add(bt);
            _HexField2D[_CurrentY, _CurrentX] = bt;
            _CurrentX++;

            if (_CurrentX == _FieldSize)
            {
                _CurrentY++;
                _CurrentX = 0;
            }
        }

        private async Task<Result> MoveAsync(int id, Point3 point)
        {
            Result res = await App.Core.SendMoveAsync(id, point).ConfigureAwait(false);

            if (res == Result.OKEY)
            {
                App.Current.Dispatcher.Invoke(() => { VehicleMove(id, point); });
                App.Core.Log("Машина перемещена по x: " + point.x + ", y: " + point.y + ", z: " + point.z);
            }
            else if (res == Result.BAD_COMMAND)
            {
                App.Core.Log("MoveAsync: Неверный ход");
            }
            return res;
        }

        private async Task<Result> ShootAsync(int id, Point3 point)
        {
            Result res = await App.Core.SendShootAsync(id, point).ConfigureAwait(false);
            if (res == Result.OKEY)
            {
                App.Current.Dispatcher.Invoke(() => { VehicleShoot(id, point); });
                App.Core.Log("Выстрел по x: " + point.x + ", y: " + point.y + ", z: " + point.z);
            }
            else if (res == Result.BAD_COMMAND)
            {
                App.Core.Log("ShootAsync: Неверный ход");
            }
            return res;
        }

        private async Task<Result> TurnAsync()
        {
            App.Current.Dispatcher.Invoke(() => { StepEnable = false; CommandBase.RaiseCanExecuteChanged(); });
            return await App.Core.SendTurnAsync().ConfigureAwait(false);
        }

        private async Task<bool> GetLogin(LoginCreate login)
        {
            var login_res = await App.Core.SendLoginAsync(login).ConfigureAwait(false);
            if (login_res.Item1 != Result.OKEY)
            {
                App.Core.Log("Ошибка: " + login_res.Item1.ToString());
                return false;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                var playerex = new PlayerEx();
                playerex.CurrentPlayer = login_res.Item2;
                playerex.TeamColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xf8, 0x57, 0x06));
                Player = playerex;
            });
            App.Core.Log("Авторизация выполнена");
            return true;
        }

        private async Task<bool> GetMap()
        {
            var res_map = await App.Core.SendMapAsync().ConfigureAwait(false);
            if (res_map.Item1 != Result.OKEY)
            {
                App.Core.Log("Ошибка: " + res_map.Item1.ToString());
                return false;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                Map = res_map.Item2;
                CreateField(Map);
                MessageWait = "Ожидание хода...";
            });

            var main_page = App.Host.Services.GetRequiredService<MainWindowViewModel>();
            main_page.SelectGamePage();

            App.Core.Log("Карта загружена");
            return true;
        }

        private async Task<bool> WaitPlayers(int timeOut = int.MaxValue)
        {
            while (timeOut > 0)
            {
                var res = await App.Core.SendGameStateAsync().ConfigureAwait(false);
                if (res.Item1 == Result.OKEY && res.Item2.num_players == res.Item2.players.Length)
                {
                    App.Current.Dispatcher.Invoke(() => { GameState = res.Item2; CreateContent(res.Item2); });
                }

                if (!Inited)
                    await Task.Delay(500);
                else break;
                timeOut -= 500;
            }

            if (timeOut < 0)
            {
                App.Core.Log("Превышено время ожидания игроков");
                return false;
            }
            App.Core.Log("Все игроки подключены");
            return true;
        }

        private async Task<bool> UpdateGameState()
        {
            var res = await App.Core.SendGameStateAsync().ConfigureAwait(false);
            GameState = res.Item2;
            if (GameState == null) return false;

            if (GameState.finished)
            {
                if (GameState.winner != null)
                {
                    MessageWait = "Победитель: " + Players[GameState.winner.Value].CurrentPlayer.name;
                }
                else MessageWait = "Ничья";
                return false;
            }

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                StepEnable = true;
                _EventWait = new TaskCompletionSource<bool>();
                UpdateContent(GameState);
                CommandBase.RaiseCanExecuteChanged();
            }));
            return StepEnable;
        }

        private async Task<bool> GetActions()
        {
            var act = await App.Core.SendActionsAsync().ConfigureAwait(false);
            if (act.Item1 != Result.OKEY)
                return false;

            if (act.Item2 != null)
            {
                foreach (var i in act.Item2.actions)
                {
                    if (i.action_type == Model.WebAction.CHAT)
                    {
                        ChatMessage msg = JsonConvert.DeserializeObject<ChatMessage>(i.data.ToString());
                        App.Current.Dispatcher.Invoke(() => { Chat.Add(msg.message); });
                    }
                }
            }
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string PropetryName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropetryName));
        }
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropetryName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropetryName);
            return true;
        }
        protected virtual bool Update([CallerMemberName] string PropetryName = null)
        {
            OnPropertyChanged(PropetryName);
            return true;
        }
        #endregion
    }
}

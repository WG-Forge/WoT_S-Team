using System;
using System.Collections.Generic;
using System.Linq;
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
using UIClient.Model;
using UIClient.Model.Server;

namespace UIClient.Infrastructure.Controls
{
    /// <summary>
    /// Логика взаимодействия для CellHex.xaml
    /// </summary>
    public partial class Hex : UserControl
    {
        public enum HexType
        {
            Free,
            Nun,
            Rock,
            Base,
            Spawn,
            LightRepair,
            HardRepair,
            Catapult,
        }

        readonly static Dictionary<HexType, Style> Stats = new Dictionary<HexType, Style>()
        {
             { HexType.Free, Application.Current.FindResource("ButtonFreeHex") as Style },
             { HexType.Nun, Application.Current.FindResource("ButtonNunHex") as Style },
             { HexType.Rock, Application.Current.FindResource("ButtonRockHex") as Style },
             { HexType.Base, Application.Current.FindResource("ButtonBaseHex") as Style },
             { HexType.Spawn, Application.Current.FindResource("ButtonSpawnHex") as Style },
             { HexType.LightRepair, Application.Current.FindResource("ButtonLightRepairHex") as Style },
             { HexType.HardRepair, Application.Current.FindResource("ButtonHardRepairHex") as Style },
             { HexType.Catapult, Application.Current.FindResource("ButtonCatapultHex") as Style },
        };

        public static double size_x = 40;
        public static double size_y = 35;

        public Hex(Point2 point2, Point3 point3, HexType type = HexType.Free)
        {
            InitializeComponent();
            Type = type;
            Point2 = point2;
            Point3 = point3;
        }

        public Tank Tank
        {
            get { return (Tank)GetValue(TankProperty); }
            set { SetValue(TankProperty, value); }
        }
        public static readonly DependencyProperty TankProperty =
            DependencyProperty.Register(nameof(Tank), typeof(Tank), typeof(Hex), new PropertyMetadata(default(Tank)));

        public HexType Type
        {
            get { return (HexType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(HexType), typeof(Hex), new PropertyMetadata(new PropertyChangedCallback(TypeChanged)));

        public static void TypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is Hex cell)
            {
                HexType type = (HexType)e.NewValue;
                cell.Btn.Style = Stats[type];
            }
        }

        public Visibility CanMove
        {
            get { return (Visibility)GetValue(CanMoveProperty); }
            set { SetValue(CanMoveProperty, value); }
        }
        public static readonly DependencyProperty CanMoveProperty =
            DependencyProperty.Register(nameof(CanMove), typeof(Visibility), typeof(Hex), new PropertyMetadata(Visibility.Hidden));

        public Visibility CanShoot
        {
            get { return (Visibility)GetValue(CanShootProperty); }
            set { SetValue(CanShootProperty, value); }
        }
        public static readonly DependencyProperty CanShootProperty =
            DependencyProperty.Register(nameof(CanShoot), typeof(Visibility), typeof(Hex), new PropertyMetadata(Visibility.Hidden));

        public Point2 Point2 { get; set; }
        public Point3 Point3 { get; set; }

        public override string ToString()
        {
            return String.Concat("x: ", Point3.x, ", y: ", Point3.y, ", z: ", Point3.z, ", tank: ", Tank);
        }
    }
}

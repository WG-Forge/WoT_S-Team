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
using UIClient.Model.Client;

namespace UIClient.Infrastructure.Controls
{
    /// <summary>
    /// Логика взаимодействия для Tank.xaml
    /// </summary>
    public partial class Tank : UserControl
    {
        public Tank(VehicleEx vehicle, Brush team_color)
        {
            InitializeComponent();
            HP = vehicle.vehicle.health;
            Vehicle = vehicle;
            SetVahicleType(vehicle.vehicle.vehicle_type);
            TeamBrush = team_color;
            TeamColor = ((SolidColorBrush)team_color).Color;
        }

        public void SetVahicleType(Model.Server.VehicleType type)
        {
            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.FillRule = FillRule.Nonzero;

            switch (type)
            {
                case Model.Server.VehicleType.СТ:
                    PathFigure figure = new PathFigure() { StartPoint = new Point(0.0, 36.0), IsClosed = true };
                    figure.Segments.Add(new LineSegment() { Point = new Point(33.0, 0.0) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(66.0, 36.0) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(33.0, 72.0) });
                    pathGeometry.Figures.Add(figure);
                    PathFigure figure1 = new PathFigure() { StartPoint = new Point(16.5, 54.0), IsClosed = true };
                    figure1.Segments.Add(new LineSegment() { Point = new Point(49.5, 18.0) });
                    pathGeometry.Figures.Add(figure1);

                    HPMax = 2;
                    Speed = 2;
                    Damage = 1;
                    Gold = 2;
                    ShootMin = 2;
                    ShootMax = 2;
                    break;
                case Model.Server.VehicleType.ЛТ:
                    figure = new PathFigure() { StartPoint = new Point(0.0, 36.0), IsClosed = true };
                    figure.Segments.Add(new LineSegment() { Point = new Point(33.0, 0.0) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(66.0, 36.0) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(33.0, 72.0) });
                    pathGeometry.Figures.Add(figure);


                    HPMax = 1;
                    Speed = 3;
                    Damage = 1;
                    Gold = 1;
                    ShootMin = 2;
                    ShootMax = 2;
                    break;
                case Model.Server.VehicleType.ТТ:
                    figure = new PathFigure() { StartPoint = new Point(0.0, 36.0), IsClosed = true };
                    figure.Segments.Add(new LineSegment() { Point = new Point(33.0, 0.0) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(66.0, 36.0) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(33.0, 72.0) });
                    pathGeometry.Figures.Add(figure);

                    figure1 = new PathFigure() { StartPoint = new Point(44.0, 12.0), IsClosed = true };
                    figure1.Segments.Add(new LineSegment() { Point = new Point(11.0, 48.0) });
                    pathGeometry.Figures.Add(figure1);

                    PathFigure figure2 = new PathFigure() { StartPoint = new Point(22.0, 60.0), IsClosed = true };
                    figure2.Segments.Add(new LineSegment() { Point = new Point(55.0, 24.0) });
                    pathGeometry.Figures.Add(figure2);

                    HPMax = 3;
                    Speed = 1;
                    Damage = 1;
                    Gold = 3;
                    ShootMin = 1;
                    ShootMax = 2;
                    break;
                case Model.Server.VehicleType.ПТ:
                    figure = new PathFigure() { StartPoint = new Point(0.0, 7.421), IsClosed = true };
                    figure.Segments.Add(new LineSegment() { Point = new Point(66.0, 7.421) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(33.0, 64.579) });
                    pathGeometry.Figures.Add(figure);

                    HPMax = 2;
                    Speed = 1;
                    Damage = 1;
                    Gold = 2;
                    ShootMin = 1;
                    ShootMax = 3;
                    break;
                case Model.Server.VehicleType.САУ:
                    figure = new PathFigure() { StartPoint = new Point(9.0, 12.0), IsClosed = true };
                    figure.Segments.Add(new LineSegment() { Point = new Point(57.0, 12.0) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(57.0, 60.0) });
                    figure.Segments.Add(new LineSegment() { Point = new Point(9.0, 60.0) });
                    pathGeometry.Figures.Add(figure);

                    HPMax = 1;
                    Speed = 1;
                    Damage = 1;
                    Gold = 1;
                    ShootMin = 3;
                    ShootMax = 3;
                    break;
                default:
                    throw new Exception("Error vehicle type");
            }
            Type = pathGeometry;
        }

        public VehicleEx Vehicle { get; set; }
        public int HPMax { get; set; }
        public int Speed { get; set; }
        public int Damage { get; set; }
        public int Gold { get; set; }
        public int ShootMin { get; set; }
        public int ShootMax { get; set; }

        public Color TeamColor
        {
            get { return (Color)GetValue(TeamColorProperty); }
            set { SetValue(TeamColorProperty, value); }
        }
        public static readonly DependencyProperty TeamColorProperty =
            DependencyProperty.Register(nameof(TeamColor), typeof(Color), typeof(Tank), new PropertyMetadata(default(Color)));

        public Brush TeamBrush
        {
            get { return (Brush)GetValue(TeamBrushProperty); }
            set { SetValue(TeamBrushProperty, value); }
        }
        public static readonly DependencyProperty TeamBrushProperty =
            DependencyProperty.Register(nameof(TeamBrush), typeof(Brush), typeof(Tank), new PropertyMetadata(default(Brush)));

        public PathGeometry Type
        {
            get { return (PathGeometry )GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(PathGeometry ), typeof(Tank), new PropertyMetadata(default(PathGeometry )));

        public int HP
        {
            get { return (int)GetValue(HPProperty); }
            set { SetValue(HPProperty, value); }
        }
        public static readonly DependencyProperty HPProperty =
            DependencyProperty.Register(nameof(HP), typeof(int), typeof(Tank), new PropertyMetadata(default(int)));

        public override string ToString()
        {
            return String.Concat("Type: ", Vehicle.vehicle.vehicle_type);
        }
    }
}

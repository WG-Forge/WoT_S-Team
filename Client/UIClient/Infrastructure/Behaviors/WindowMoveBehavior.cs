using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UIClient.Infrastructure.Behavior
{
    public class WindowMoveBehavior : Behavior<UIElement>
    {
        private DependencyObject FindVisualRoot(DependencyObject obj)
        {
            do
            {
                var parent = VisualTreeHelper.GetParent(obj);
                if (parent is null) return obj;
                if (parent is Window) return parent;
                obj = parent;
            }
            while (true);
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseMove;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseMove;
        }

        private void AssociatedObject_MouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var window = FindVisualRoot(AssociatedObject) as Window;
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.Top = 3;
                }
                window.DragMove();
            }
        }
    }
}

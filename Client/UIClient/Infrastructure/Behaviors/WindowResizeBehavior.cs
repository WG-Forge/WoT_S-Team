using Microsoft.Xaml.Behaviors;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace UIClient.Infrastructure.Behavior
{
    public class WindowResizeBehavior : Behavior<UIElement>
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
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

        private Window _window = null;
        private IntPtr _handl = IntPtr.Zero;

        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_window == null) _window = FindVisualRoot(AssociatedObject) as Window;
            if (_window == null) return;
            if (_handl == IntPtr.Zero) _handl = new WindowInteropHelper(_window).Handle;
            if (_handl == IntPtr.Zero) return;
            SendMessage(_handl, 0x0112, 0xF000 + 8, IntPtr.Zero);
        }
    }
}

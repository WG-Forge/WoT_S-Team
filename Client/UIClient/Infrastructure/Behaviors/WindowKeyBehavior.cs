using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UIClient.Infrastructure.Behavior
{
    public class WindowKeyBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseMove;

            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not MainWindow curr_hex) return;
            if (AssociatedObject.DataContext is not ViewModel.MainWindowViewModel vm) return;
            vm.KeyPress(e);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseMove;
        }

        private void AssociatedObject_MouseMove(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}

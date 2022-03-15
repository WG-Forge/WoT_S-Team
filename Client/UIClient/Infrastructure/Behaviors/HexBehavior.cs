using Microsoft.Xaml.Behaviors;
using UIClient.Model;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using UIClient.Infrastructure.Controls;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UIClient.Model.Server;

namespace UIClient.Infrastructure.Behavior
{
    public class HexBehavior : Behavior<Hex>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
        }

        private async void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Hex curr_hex) return;
            if (AssociatedObject.DataContext is not ViewModel.GamePageViewModel vm) return;
            await vm.Field.OnHexClick(curr_hex, vm).ConfigureAwait(false);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
        }
    }
}

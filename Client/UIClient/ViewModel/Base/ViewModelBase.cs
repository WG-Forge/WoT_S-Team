using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace UIClient.ViewModel.Base
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
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
    }
}

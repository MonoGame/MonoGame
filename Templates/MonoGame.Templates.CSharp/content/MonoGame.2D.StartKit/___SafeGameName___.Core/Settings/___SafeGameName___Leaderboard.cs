using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ___SafeGameName___.Core.Settings
{
    internal class ___SafeGameName___Leaderboard : INotifyPropertyChanged
    {
        // TODO public string PlayerName { get; set; }
        public TimeSpan FastestTime { get; set; }
        public int GemsCollected { get; set; }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

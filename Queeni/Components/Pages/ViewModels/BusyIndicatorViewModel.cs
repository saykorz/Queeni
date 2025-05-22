using Queeni.Components.Library.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.ViewModels
{
    public partial class BusyIndicatorViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _showMessageBox;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyMessage = "Please Wait...";

        public ObservableCollection<string> BusyPool { get; set; } = new ObservableCollection<string>();

        public void StartProgress(string poolMessage)
        {
            BusyPool.Add(poolMessage);
        }

        public void EndProgress(string poolMessage)
        {
            BusyPool.TryRemove(poolMessage);
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> operation, string message)
        {
            BusyPool.Add(message);

            try
            {
                T result = await operation();
                return result;
            }
            finally
            {
                BusyPool.TryRemove(message);
            }
        }

        public async Task RunAsync(Func<Task> operation, string message)
        {
            BusyPool.Add(message);

            try
            {
                await operation();
            }
            finally
            {
                BusyPool.TryRemove(message);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Services
{
    public class BusyIndicatorService : IDisposable
    {
        public bool IsBusy { get; set; }
        public string BusyMessage { get; set; } = "Please Wait...";
        public ObservableCollection<string> BusyPool { get; } = new ObservableCollection<string>();

        public BusyIndicatorService()
        {
            BusyPool.CollectionChanged += BusyPool_CollectionChanged;
        }

        private void BusyPool_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (BusyPool.Count == 0)
            {
                IsBusy = false;
            }
            else
            {
                BusyMessage = BusyPool.FirstOrDefault() ?? "Please Wait...";
                IsBusy = true;
            }

            // Notify state changed
            OnStateChanged?.Invoke();
        }

        public event Action? OnStateChanged;

        public void StartProgress(string poolMessage)
        {
            BusyPool.Add(poolMessage);
        }

        public void EndProgress(string poolMessage)
        {
            BusyPool.Remove(poolMessage);
        }

        public async Task<T> RunAsync<T>(Func<Task<T>> operation, string message)
        {
            BusyPool.Add(message);

            try
            {
                return await operation();
            }
            finally
            {
                BusyPool.Remove(message);
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
                BusyPool.Remove(message);
            }
        }

        public void Dispose()
        {
            BusyPool.CollectionChanged -= BusyPool_CollectionChanged;
        }
    }
}

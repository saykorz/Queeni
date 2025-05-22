using Queeni.Components.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Interfaces
{
    public interface IRealtimeSyncService
    {
        event EventHandler<SyncMessage> MessageReceived;

        Task InitAsync();

        Task NotifyReadAsync(TaskViewModel item);

        Task NotifyItemCreatedOrUpdateAsync(TaskViewModel item);

        Task NotifyItemDeleteAsync(int id);

        Task CloseAsync();
    }
}

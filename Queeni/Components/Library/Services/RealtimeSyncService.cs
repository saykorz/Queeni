using Queeni.Components.Library.Extensions;
using Queeni.Components.Library.Interfaces;
using Queeni.Components.Models;
using IO.Ably;
using IO.Ably.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Services
{
    public class RealtimeSyncService : IRealtimeSyncService
    {
        //private readonly string _deviceId;
        private AblyRealtime _client;
        private IRealtimeChannel _channel;

        public event EventHandler<SyncMessage>? MessageReceived;
        public RealtimeSyncService()
        {
            AppCache.DeviceId = Preferences.Default.Get("DeviceId", Guid.NewGuid().ToString());
            Preferences.Default.Set("DeviceId", AppCache.DeviceId);
        }
        public async Task InitAsync()
        {
            if (string.IsNullOrWhiteSpace(AppCache.Settings.AblyKey))
                return;

            _client = new AblyRealtime(AppCache.Settings.AblyKey);
            _client.Connection.On(ConnectionEvent.Connected, args =>
            {
                Console.Out.WriteLine("Connected to Ably!");
            });

            _channel = _client.Channels.Get(AppCache.Settings.AblyChannel);
            _channel.Subscribe(message =>
            {
                var json = message.Data.ToString();
                var syncMsg = JsonSerializer.Deserialize<SyncMessage>(json);

                if (syncMsg.DeviceId == AppCache.DeviceId)
                    return;

                MessageReceived?.Invoke(this, syncMsg);
            });

            await Task.CompletedTask;
        }
        public async Task NotifyReadAsync(TaskViewModel item)
        => await PublishAsync(new SyncMessage
        {
            DeviceId = AppCache.DeviceId,
            Item = item,
            MessageType = Enumerations.MessageTypes.ReadOnly
        });
        public async Task NotifyItemCreatedOrUpdateAsync(TaskViewModel item)
            => await PublishAsync(new SyncMessage
            {
                DeviceId = AppCache.DeviceId,
                Item = item,
                MessageType = Enumerations.MessageTypes.AddOrUpdate
            });
        public async Task NotifyItemDeleteAsync(int id)
            => await PublishAsync(new SyncMessage
            {
                Id = id,
                DeviceId = AppCache.DeviceId,
                MessageType = Enumerations.MessageTypes.Delete
            });
        private async Task PublishAsync(SyncMessage message)
        {
            if(_channel == null) return;

            var json = message.SerializeToJson();
            await _channel.PublishAsync("sync-event", json);
        }
        public async Task CloseAsync()
        {
            if (_client == null) return;

            _client.Connection.Close();
            _client.Connection.On(ConnectionEvent.Closed, args =>
            {
                Console.Out.WriteLine("Closed the connection to Ably.");
            });

            await Task.CompletedTask;
        }
    }
}

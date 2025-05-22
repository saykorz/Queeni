using Queeni.Components.Library.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Models
{
    public class SyncMessage
    {
        public int Id { get; set; }

        public required string DeviceId { get; set; }

        public MessageTypes MessageType { get; set; }

        public TaskViewModel Item { get; set; } = null;
    }
}

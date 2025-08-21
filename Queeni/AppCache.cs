using Queeni.Components.Library.AI;
using Queeni.Components.ViewModels;
using Queeni.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni
{
    public static class AppCache
    {
        public static string DeviceId { get; set; } = string.Empty;

        public static string DatabaseAddress { get; set; } = string.Empty;

        public static QueeniConfigModel Settings { get; set; }

        public static IServiceProvider Services { get; set; }

        static AppCache()
        {
        }
    }
}

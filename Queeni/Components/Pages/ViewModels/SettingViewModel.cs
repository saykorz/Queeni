using AutoMapper;
using AutonomiFileManager.App.Components.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Queeni.Components.Library.Interfaces;
using Queeni.Data;
using Queeni.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Pages.ViewModels
{
    public partial class SettingViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string privateKey;

        public SettingViewModel(IMapper mapper, IRealtimeSyncService sync, IUowData data) 
            : base(mapper, sync, data)
        {
        }

        public async Task SetPrivateKey()
        {
            await QueeniConfigManager.SaveAsync(AppCache.Settings);
        }
    }
}

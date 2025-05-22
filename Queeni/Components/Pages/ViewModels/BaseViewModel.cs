using AutoMapper;
using Queeni.Components.Library.Interfaces;
using Queeni.Data.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutonomiFileManager.App.Components.ViewModels
{
    public  class BaseViewModel: ObservableObject
    {
        protected readonly IMapper _mapper;

        protected readonly IRealtimeSyncService _sync;

        protected IUowData Data { get; set; }

        public BaseViewModel(IMapper mapper, IRealtimeSyncService sync, IUowData data)
        {
            _mapper = mapper;
            _sync = sync;
            Data = data;
        }
    }
}

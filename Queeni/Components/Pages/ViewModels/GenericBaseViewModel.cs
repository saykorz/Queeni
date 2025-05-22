using AutoMapper;
using AutonomiFileManager.App.Components.ViewModels;
using Queeni.Components.Library.Extensions;
using Queeni.Components.Library.Interfaces;
using Queeni.Data.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Pages.ViewModels
{
    public abstract partial class GenericBaseViewModel<TModel, TViewModel> : BaseViewModel
        where TModel : class, IEntityModel
        where TViewModel : class, IEntityModel
    {
        [ObservableProperty]
        private long? _itemCount;

        [ObservableProperty]
        private int _pageSize;

        internal IRepository<TModel> Context;

        public ObservableCollection<TViewModel> Items { get; set; }

        internal abstract Task<TModel> CreateOrUpdateData(TViewModel model, string id);

        protected abstract Task<bool> DeleteData(TViewModel model);

        protected GenericBaseViewModel(IMapper mapper, IRealtimeSyncService sync, IUowData data, IRepository<TModel> context)
            : base(mapper, sync, data)
        {
            Context = context;
            Items = new ObservableCollection<TViewModel>();
            PageSize = 18;
        }
        public virtual async Task<List<TViewModel>> ReadMulti(Expression<Func<TModel, bool>> predicate = null)
        {
            var result = await GetMultiData(predicate);
            if (result == null)
                return null;

            Items.Clear();
            Items.AddRange(result);
            return result;
        }
        public virtual async Task<TViewModel> CreateOrUpdate(TViewModel model, bool isNotifySend, string id = null)
        {
            if (model != null)
            {
                var result = await CreateOrUpdateData(model, id);
                _mapper.Map(result, model);
            }

            return model;
        }

        internal virtual async Task<List<TViewModel>> GetMultiData(Expression<Func<TModel, bool>> predicate = null)
        {
            if (Context == null)
                return null;

            var result = await GetRawMultiData(predicate);
            var model = _mapper.Map<List<TViewModel>>(result);
            return model;
        }

        internal virtual async Task<List<TModel>> GetRawMultiData(Expression<Func<TModel, bool>> predicate = null)
        {
            if (Context == null)
                return null;

            var result = await Context.All(predicate);
            if (result != null)
                return result.ToList();

            return null;
        }
    }
}

using AutoMapper;
using Queeni.Components.Models;
using Queeni.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CategoryModel, CategoryViewModel>();

            CreateMap<CategoryViewModel, CategoryModel>();

            CreateMap<TaskModel, TaskViewModel>()
                .ForMember(x => x.BindValue, c => c.Ignore())
                .ForMember(x => x.BindTags, c => c.Ignore());

            CreateMap<TaskViewModel, TaskModel>();
        }

        private int SafeParseId(string id) => int.TryParse(id, out var result) ? result : 0;

        private int? SafeParseNullableId(string id) => int.TryParse(id, out var result) ? result : null;

    }
}

using Queeni.Data.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Models
{
    public partial class BaseTitleViewModel : ObservableObject, IEntityModel
    {
        [Display(AutoGenerateField = false)]
        public int Id { get; set; }

        [ObservableProperty]
        private string title = string.Empty;
    }
}

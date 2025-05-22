using Queeni.Components.Library.Extensions;
using Queeni.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Formats.Asn1;
using System.Text.Json.Serialization;

namespace Queeni.Components.Models
{
    public partial class TaskViewModel: BaseTitleViewModel
    {
        [ObservableProperty]
        [Display(AutoGenerateField = false)]
        private string categoryTitle = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        public string priority = string.Empty;

        [ObservableProperty]
        public string cardTags = string.Empty;

        public int CategoryId { get; set; }

        [JsonIgnore]
        public CategoryViewModel? Category { get; set; }

        [JsonIgnore]
        public string BindValue { 
            get 
            { 
                return CategoryId.ToString(); 
            }
            set 
            {
                CategoryId = value.ToInt();
            }
        }

        [JsonIgnore]
        public List<string> BindTags
        {
            get
            {
                if (CardTags != null && CardTags.Any())
                    return CardTags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(x => x.Trim())
                          .ToList(); 
                return new List<string>();
            }
            set
            {
                CardTags = string.Join(",", value);
            }
        }
    }
}
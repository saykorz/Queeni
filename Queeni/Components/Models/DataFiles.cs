using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Models
{
    public class DataFile
    {
        public string FileName { get; set; }

        public DateTime Date { get; set; }

        public string DisplayDate => Date.ToString("yyyy-MM-dd HH:mm:ss");

        public DataFile(string fileName)
        {
            FileName = fileName;

            // Пример: 20250415_201530_taskdata.json
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var timestampPart = nameWithoutExtension.Split('_').FirstOrDefault();

            if (DateTime.TryParseExact(timestampPart, "yyyyMMddHHmmss", null,
                                       System.Globalization.DateTimeStyles.None,
                                       out DateTime parsedDate))
            {
                Date = parsedDate;
            }
            else
            {
                Date = DateTime.MinValue;
            }
        }
    }
}

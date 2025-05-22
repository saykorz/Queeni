using Queeni.Components.Models;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Queeni.Components.Library.Extensions
{
    public static class SerializationExtensions
    {
        // Serialize any object to JSON string
        public static string SerializeToJson<T>(this T source)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver
                {
                    Modifiers = { ExcludeEmptyStrings }
                },
                WriteIndented = true // prettier output
            };

            return JsonSerializer.Serialize(source, options);
        }

        // Deserialize JSON string back to an object of type T
        public static T DeserializeFromJson<T>(this string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // handle case differences in property names
            };

            return JsonSerializer.Deserialize<T>(json, options);
        }

        public static string SaveToJsonFile<T>(this T source, string fileName, string? filePath = null)
        {
            var json = source.SerializeToJson();
            string fullPath = GetFullFilePath(fileName, filePath);
            File.WriteAllText(fullPath, json);
            return fullPath;
        }

        public static T? LoadFromJsonFile<T>(string fileName, string? filePath = null)
        {
            string fullPath = GetFullFilePath(fileName, filePath);
            if (File.Exists(fullPath))
            {
                var json = File.ReadAllText(fullPath);
                return json.DeserializeFromJson<T>();
            }
            return default;
        }

        public static string SaveWithTimestamp<T>(this T source, string suffix, string? filePath = null)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"{timestamp}_{suffix}.json";
            source.SaveToJsonFile(fileName);
            return filePath;
        }

        public static ObservableCollection<DataFile> GetAllTaskDataFiles(string baseFileName, string? filePath = null)
        {
            string fileName = $"*_{baseFileName}.json";
            string folder = FileSystem.AppDataDirectory;
            if (!string.IsNullOrEmpty(filePath))
                folder = filePath;

            var dataFiles = Directory.GetFiles(folder, fileName)
                .Select(Path.GetFileName)
                .Select(name => new DataFile(name))
                .OrderByDescending(df => df.Date)
                .ToList();

            return new ObservableCollection<DataFile>(dataFiles);
        }

        public static string GetFullFilePath(string fileName, string? filePath = null)
        {
            string fullPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            if (!string.IsNullOrEmpty(filePath))
                fullPath = Path.Combine(filePath, fileName);
            return fullPath;
        }

        private static void ExcludeEmptyStrings(JsonTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
                return;

            foreach (JsonPropertyInfo jsonPropertyInfo in jsonTypeInfo.Properties)
            {
                if (jsonPropertyInfo.PropertyType == typeof(string))
                {
                    jsonPropertyInfo.ShouldSerialize = static (obj, value) =>
                        !string.IsNullOrEmpty((string)value);
                }
            }
        }
    }
}

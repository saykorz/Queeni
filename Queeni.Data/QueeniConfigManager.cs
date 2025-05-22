using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Queeni.Data
{
    public static class QueeniConfigManager
    {
        public static readonly string DefaultDbFileName = "Queeni.db";

        public static readonly string AppPath = AppContext.BaseDirectory;

        private static readonly string ConfigPath = Path.Combine(AppPath, "queeni-config.json");

        public static readonly string RegisterSigningKeyPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "autonomi", "client", "register_signing_key");

        public static async Task<QueeniConfigModel> LoadAsync()
        {
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new QueeniConfigModel();
                await SaveAsync(defaultConfig);
                return defaultConfig;
            }

            var json = await File.ReadAllTextAsync(ConfigPath);
            return JsonSerializer.Deserialize<QueeniConfigModel>(json);
        }

        public static async Task SaveAsync(QueeniConfigModel config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
            await File.WriteAllTextAsync(ConfigPath, json);
        }

        
    }

}

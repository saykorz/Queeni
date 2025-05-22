using OpenAI.Moderations;
using Queeni.Data;
using Syncfusion.Blazor.Kanban;

namespace Queeni.Components.Pages.ViewModels
{
    public partial class HomeViewModel
    {
        public async Task<bool> LoadData()
        {
            var result = false;
            result = await AppCache.BusyIndicator.RunAsync(async () =>
            {
                AppCache.Settings = await QueeniConfigManager.LoadAsync();
                if (string.IsNullOrWhiteSpace(AppCache.Settings.SecretKey))
                {
                    AppCache.Settings.Messages = "Please add Wallet Private Key";
                    return false;
                }

                AutonomiNet.Client.SecretKey = AppCache.Settings.SecretKey;

                if (string.IsNullOrWhiteSpace(AppCache.Settings.RegisterSigningKey))
                {
                    if (File.Exists(QueeniConfigManager.RegisterSigningKeyPath))
                    {
                        AppCache.Settings.RegisterSigningKey = await File.ReadAllTextAsync(QueeniConfigManager.RegisterSigningKeyPath);
                    }
                    else
                    {
                        AppCache.Settings.Messages = await AutonomiNet.Register.GenerateKey();
                        if (AppCache.Settings.Messages.Contains("Created new register key", StringComparison.OrdinalIgnoreCase) && 
                            File.Exists(QueeniConfigManager.RegisterSigningKeyPath))
                        {
                            AppCache.Settings.RegisterSigningKey = await File.ReadAllTextAsync(QueeniConfigManager.RegisterSigningKeyPath);
                        }
                        else
                        {
                            AppCache.Settings.Messages = "Please add Register Signing Key";
                            return false;
                        }
                    }

                    await QueeniConfigManager.SaveAsync(AppCache.Settings);
                }

                AutonomiNet.Client.RegisterSigningKey = AppCache.Settings.RegisterSigningKey;

                await DownloadDatabaseAsync();

                return true;
            }, "Loading Settings...");
            return result;
        }

        
        private static async Task DownloadDatabaseAsync()
        {
            AutonomiNet.Client.IsTestNetwork = true;

            if (string.IsNullOrEmpty(AppCache.Settings.Messages))
            {
                if (string.IsNullOrEmpty(AppCache.Settings.DatabaseKey))
                {
                    AppCache.Settings.DatabaseKey = await AutonomiNet.Register.Create(QueeniConfigManager.DefaultDbFileName, "9999999999999999999");
                    await QueeniConfigManager.SaveAsync(AppCache.Settings);
                }

                var vault = await AutonomiNet.Vault.Load();
                AppCache.DatabaseAddress = await AutonomiNet.Register.GetByName(QueeniConfigManager.DefaultDbFileName);
                var dowbloadDatabase = await AutonomiNet.File.Download(AppCache.DatabaseAddress, ApplicationDbContextFactory.GetDatabaseDirectoryPath());
            }
        }
    }
}
using IO.Ably;
using OpenAI.Moderations;
using Queeni.Components.Library.Services;
using Queeni.Data;
using Syncfusion.Blazor.Kanban;
using System.Text.RegularExpressions;

namespace Queeni.Components.Pages.ViewModels
{
    public partial class HomeViewModel
    {
        private readonly BusyIndicatorService _busyIndicator;

        public HomeViewModel(BusyIndicatorService busyIndicator)
        {
            _busyIndicator = busyIndicator;
        }

        public async Task<bool> LoadData()
        {
            var result = false;
            result = await _busyIndicator.RunAsync(async () =>
            {
                AppCache.Settings = await QueeniConfigManager.LoadAsync();
                if (string.IsNullOrWhiteSpace(AppCache.Settings.SecretKey))
                {
                    AppCache.Settings.Messages = "Please add Wallet Private Key";
                    return false;
                }

                AutonomiNet.Client.SecretKey = AppCache.Settings.SecretKey;
                AutonomiNet.Client.IsLive = AppCache.Settings.IsLive;

                var vault = await AutonomiNet.Vault.Load();
                if (!vault.Contains("Successfully loaded vault"))
                {
                    await AutonomiNet.Vault.Create();
                }

                if (string.IsNullOrWhiteSpace(AppCache.Settings.ScratchpadSigningKey))
                {
                    if (File.Exists(QueeniConfigManager.ScratchpadSigningKeyPath))
                    {
                        AppCache.Settings.ScratchpadSigningKey = await File.ReadAllTextAsync(QueeniConfigManager.ScratchpadSigningKeyPath);
                    }
                    else
                    {
                        AppCache.Settings.Messages = await AutonomiNet.Scratchpad.GenerateKey();
                        if (AppCache.Settings.Messages.Contains("Created new scratchpad key", StringComparison.OrdinalIgnoreCase) &&
                            File.Exists(QueeniConfigManager.ScratchpadSigningKeyPath))
                        {
                            AppCache.Settings.ScratchpadSigningKey = await File.ReadAllTextAsync(QueeniConfigManager.ScratchpadSigningKeyPath);
                            await AutonomiNet.Vault.Sync();
                        }
                        else
                        {
                            AppCache.Settings.Messages = "Please add Scratchpad Signing Key";
                            return false;
                        }
                    }

                    await QueeniConfigManager.SaveAsync(AppCache.Settings);
                }

                AutonomiNet.Client.ScratchpadSigningKey = AppCache.Settings.ScratchpadSigningKey;

                await DownloadDatabaseAsync();

                return true;
            }, "Loading Settings...");
            return result;
        }
        
        private static async Task DownloadDatabaseAsync()
        {
            if (string.IsNullOrEmpty(AppCache.Settings.Messages))
            {
                if (string.IsNullOrEmpty(AppCache.Settings.DatabaseKey))
                {
                    var resultCreated = await AutonomiNet.Scratchpad.Create(QueeniConfigManager.DefaultDbFileName, "9999999999999999999");
                    if (resultCreated != null && (resultCreated.Contains("Scratchpad created at address") ||
                        resultCreated.Contains("Scratchpad already exists at this address")))
                    {
                        AppCache.Settings.DatabaseKey = QueeniConfigManager.DefaultDbFileName;
                        await QueeniConfigManager.SaveAsync(AppCache.Settings);
                    }
                }

                var vault = await AutonomiNet.Vault.Load();
                var resultGet = await AutonomiNet.Scratchpad.Get(QueeniConfigManager.DefaultDbFileName);
                var match = Regex.Match(resultGet, @"Data:\s*(.+)");
                if (match.Success)
                {
                    var dataValue = match.Groups[1].Value.Trim();
                    AppCache.DatabaseAddress = dataValue;
                }
                var fullNewDbPath = ApplicationDbContextFactory.GetWorkingDatabasePath();
                var dowbloadDatabase = await AutonomiNet.File.Download(AppCache.DatabaseAddress, fullNewDbPath);
            }
        }
    }
}
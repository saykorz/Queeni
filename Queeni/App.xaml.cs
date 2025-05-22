using Queeni.Components.Library.Extensions;
using Queeni.Data;
using Queeni.Data.Interfaces;

namespace Queeni
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new MainPage()) { Title = "Queeni" };
#if WINDOWS
            //window.Created += (s, e) =>
            //{

            //    //we need this to use Microsoft.UI.Windowing functions for our window
            //    var handle = WinRT.Interop.WindowNative.GetWindowHandle(window.Handler.PlatformView);
            //    var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
            //    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);

            //    //and here it is
            //    appWindow.Closing += async (s, e) =>
            //            {
            //                e.Cancel = true;
            //                string action = await App.Current.MainPage.DisplayActionSheet(
            //                    "Choose an action",
            //                    "Cancel",          // Cancel button (в случая 'Close')
            //                    null,             // Destruction button (null няма)
            //                    "Save and Exit"); // опция 1

            //                switch (action)
            //                {
            //                    case "Save and Exit":
            //                        await SaveChanges();
            //                        break;

            //                    case "Close":
            //                        // просто затваряме, извън бутона Cancel в DisplayActionSheet
            //                        //App.Current.Quit();
            //                        break;
            //                }
            //            };
            //};
#endif
            return window;
        }

        public static async Task SaveChanges()
        {
            var uow = AppCache.Services.GetRequiredService<IUowData>();
            uow.Dispose();

            var result = await AppCache.BusyIndicator.RunAsync(async () => {
                var fullNewDbPath = Path.Combine(ApplicationDbContextFactory.GetDatabaseDirectoryPath(), $"{ShortGuid.NewGuid()}-{QueeniConfigManager.AppPath}");
                File.Move(ApplicationDbContextFactory.GetFullDatabasePath(), fullNewDbPath, true);

                AppCache.DatabaseAddress = await AutonomiNet.File.Upload(ApplicationDbContextFactory.GetFullDatabasePath(), true);
                var result = await AutonomiNet.Register.Edit(QueeniConfigManager.DefaultDbFileName, AppCache.DatabaseAddress);
                var vault = await AutonomiNet.Vault.Sync();
                return result;
            }, "Saving data...");

            App.Current.Quit();
        }
    }
}

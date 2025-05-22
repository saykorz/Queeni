using AutoMapper;
using Queeni.Components.Library.AI;
using Queeni.Components.Library.Helpers;
using Queeni.Components.Library.Interfaces;
using Queeni.Components.Library.Services;
using Queeni.Data;
using Queeni.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Syncfusion.Blazor;
using Syncfusion.Maui.Core.Hosting;
using Queeni.Components.Pages.ViewModels;

namespace Queeni
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

#if WINDOWS
//https://github.com/jfversluis/MauiWindowsFullscreenMinimizeMaximizeSample
                builder.ConfigureLifecycleEvents(events =>
        {
            // Make sure to add "using Microsoft.Maui.LifecycleEvents;" in the top of the file 
            events.AddWindows(windowsLifecycleBuilder =>
            {
                windowsLifecycleBuilder.OnWindowCreated(window =>
                {
                    window.ExtendsContentIntoTitleBar = false;
                    var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
                    switch (appWindow.Presenter)
                    {
                        case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                            //overlappedPresenter.SetBorderAndTitleBar(false, false);
                            overlappedPresenter.Maximize();
                            break;
                    }
                });
            });
        });
#endif

            builder.Services.AddSyncfusionBlazor();
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("");

            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            }, typeof(MappingProfile).Assembly);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Filename={ApplicationDbContextFactory.GetFullDatabasePath()}",
                    x => x.MigrationsAssembly("Queeni.Data")));

            builder.Services.AddScoped<IUowData, UowData>();
            builder.Services.AddSingleton<IRealtimeSyncService, RealtimeSyncService>();
            builder.Services.AddSingleton<IOpenAIConversation, OpenAIConversation>();
            builder.Services.AddScoped<HomeViewModel>();
            builder.Services.AddScoped<SettingViewModel>();
            builder.Services.AddScoped<DashboardViewModel>();

            var tempProvider = builder.Services.BuildServiceProvider();
            var mapper = tempProvider.GetRequiredService<IMapper>();

            try
            {
                mapper.ConfigurationProvider.AssertConfigurationIsValid();
            }
            catch (AutoMapper.AutoMapperConfigurationException ex)
            {
                System.Diagnostics.Debug.WriteLine("AutoMapper configuration error(s):");

                var typeMapErrorsProperty = typeof(AutoMapper.AutoMapperConfigurationException).GetProperty("TypeMapConfigErrors");
                if (typeMapErrorsProperty != null)
                {
                    var typeMapErrors = typeMapErrorsProperty.GetValue(ex) as System.Collections.IEnumerable;
                    if (typeMapErrors != null)
                    {
                        foreach (var error in typeMapErrors)
                        {
                            var typeMapProperty = error.GetType().GetProperty("TypeMap");
                            var memberConfigErrorsProperty = error.GetType().GetProperty("MemberConfigErrors");

                            var typeMap = typeMapProperty?.GetValue(error);
                            var sourceType = typeMap?.GetType().GetProperty("SourceType")?.GetValue(typeMap) as Type;
                            var destinationType = typeMap?.GetType().GetProperty("DestinationType")?.GetValue(typeMap) as Type;

                            System.Diagnostics.Debug.WriteLine($"Problem with mapping from {sourceType?.Name} to {destinationType?.Name}:");

                            var memberErrors = memberConfigErrorsProperty?.GetValue(error) as System.Collections.IEnumerable;
                            if (memberErrors != null)
                            {
                                foreach (var memberError in memberErrors)
                                {
                                    System.Diagnostics.Debug.WriteLine($" - {memberError}");
                                }
                            }
                        }
                    }
                }

                throw;
            }

            var app = builder.Build();
            AppCache.Services = app.Services;
            Task.Run(async () =>
            {
                await InitializeAppAsync(app.Services);
            }).Wait();

            return app;
        }

        private static async Task InitializeAppAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            //load init data
        }
    }
}

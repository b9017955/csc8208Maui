using csc8208Maui.Views;
using csc8208Maui.Views.User;
using csc8208Maui.Views.Verifier;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace csc8208Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseBarcodeReader();

            #if DEBUG
    		builder.Logging.AddDebug();
            #endif
            builder.Services.AddSingleton<LoginPage>();//login
            builder.Services.AddSingleton<VerifierLandingPage>();
            builder.Services.AddSingleton<UserLandingPage>();
            
            return builder.Build();
        }
    }
}

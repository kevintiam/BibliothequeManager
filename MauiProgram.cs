using Microsoft.Extensions.Logging;

namespace BibliothequeManager
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
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                    fonts.AddFont("FontAwesome6Solid.otf", "FontAwesomeSolid");


                    // PlaypenSans – toutes les variantes
                    fonts.AddFont("PlaypenSans-Regular.ttf", "PlaypenSansRegular");
                    fonts.AddFont("PlaypenSans-Bold.ttf", "PlaypenSansBold");
                    fonts.AddFont("PlaypenSans-ExtraBold.ttf", "PlaypenSansExtraBold");
                    fonts.AddFont("PlaypenSans-ExtraLight.ttf", "PlaypenSansExtraLight");
                    fonts.AddFont("PlaypenSans-Light.ttf", "PlaypenSansLight");
                    fonts.AddFont("PlaypenSans-Medium.ttf", "PlaypenSansMedium");
                    fonts.AddFont("PlaypenSans-SemiBold.ttf", "PlaypenSansSemiBold");
                    fonts.AddFont("PlaypenSans-Thin.ttf", "PlaypenSansThin");
                    fonts.AddFont("PlaypenSans-VariableFont_wght.ttf", "PlaypenSansVariable");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

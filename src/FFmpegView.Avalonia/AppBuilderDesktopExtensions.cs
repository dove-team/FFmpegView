using Avalonia;
using Avalonia.Logging;
using Avalonia.Markup.Xaml.Styling;
using System;

namespace FFmpegView
{
    public static class AppBuilderDesktopExtensions
    {
        public static AppBuilder UseFFmpeg(this AppBuilder builder, string libffmpegDirectoryPath = null)
        {
            builder.AfterSetup((_) =>
            {
                InitXamlStyle(builder);
                Core.Initialize(libffmpegDirectoryPath);
            });
            return builder;
        }
        private static void InitXamlStyle(object builder)
        {
            try
            {
                StyleInclude styleInclude = new StyleInclude(new Uri("avares://FFmpegView.Avalonia")) { Source = new Uri($"avares://FFmpegView.Avalonia/FFmpegView.xaml") };
                Application.Current.Styles.Add(styleInclude);
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(builder, ex.Message);
            }
        }
    }
}
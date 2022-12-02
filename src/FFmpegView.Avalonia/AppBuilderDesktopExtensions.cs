using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Markup.Xaml.Styling;
using System;

namespace FFmpegView
{
    public static class AppBuilderDesktopExtensions
    {
        public static unsafe TAppBuilder UseFFmpeg<TAppBuilder>(this TAppBuilder builder, string libffmpegDirectoryPath = null)
           where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
        {
            builder.AfterSetup((_) =>
            {
                InitXamlStyle(builder);
                Core.Instance.Initialize(libffmpegDirectoryPath);
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
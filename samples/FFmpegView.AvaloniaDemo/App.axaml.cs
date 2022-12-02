using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FFmpegView.Bass;
using System;

namespace FFmpegView.AvaloniaDemo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            BassCore.Initialize();
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new MainWindow();
            Console.WriteLine(FontManager.Current.DefaultFontFamilyName);
            Console.WriteLine(FontManager.Current.PlatformImpl.GetDefaultFontFamilyName());
            Console.WriteLine(string.Join(';', FontManager.Current.PlatformImpl.GetInstalledFontFamilyNames()));
            base.OnFrameworkInitializationCompleted();
        }
    }
}
using Avalonia;

namespace FFmpegView.AvaloniaDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            AppBuilder.Configure<App>()
                    .LogToTrace()
                    .UseFFmpeg()
                    .UsePlatformDetect()
            .StartWithClassicDesktopLifetime(args);
        }
    }
}
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FFmpegView.Bass;

namespace FFmpegView.AvaloniaDemo
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            Width = 800;
            Height = 600;

            var playerView = this.FindControl<FFmpegView>("playerView");
            playerView.SetAudioHandler(new BassAudioStreamDecoder());
            playerView.Play("http://vfx.mtime.cn/Video/2019/02/04/mp4/190204084208765161.mp4");
        }
    }
}
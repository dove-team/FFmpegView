using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FFmpegView.Bass;
using System.Collections.Generic;

namespace FFmpegView.AvaloniaDemo
{
    public partial class MainWindow : Window
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

            var audioStreamDecoder = new BassAudioStreamDecoder();
            audioStreamDecoder.Headers = new Dictionary<string, string> { { "User-Agent", "ffmpeg_demo" } };
            playerView.SetAudioHandler(audioStreamDecoder);
            playerView.Play("http://vfx.mtime.cn/Video/2019/02/04/mp4/190204084208765161.mp4");
        }
    }
}
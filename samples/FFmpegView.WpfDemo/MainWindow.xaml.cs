using FFmpegView.NAudio;
using System.Windows;
using System.Windows.Controls;

namespace FFmpegView.WpfDemo2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            playerView.SetAudioHandler(new NAudioStreamDecoder());
            playerVView.SetAudioHandler(new NAudioStreamDecoder());
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem item)
            {
                switch (item.Header.ToString())
                {
                    case "FFmpegView":
                        playerVView.Pause();
                        playerView.Play("http://vfx.mtime.cn/Video/2019/03/18/mp4/190318231014076505.mp4");
                        break;
                    case "FFmpegVisualView":
                        playerView.Pause();
                        playerVView.Play("http://vfx.mtime.cn/Video/2019/03/18/mp4/190318231014076505.mp4");
                        break;
                }
            }
        }
    }
}
namespace FFmpegView
{
    public interface IFFmpegView
    {
        bool Play();
        bool Stop();
        bool Pause();
        bool Play(string uri);
        bool SeekTo(int seekTime);
    }
}
using System.Collections.Generic;

namespace FFmpegView
{
    public interface IFFmpegView
    {
        bool Play();
        bool Stop();
        bool Pause();
        bool SeekTo(int seekTime);
        void SetHeader(Dictionary<string, string> headers);
        bool Play(string uri, Dictionary<string, string> headers = null);
    }
}
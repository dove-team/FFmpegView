using FFmpeg.AutoGen;
using System.Collections.Generic;

namespace FFmpegView
{
    public unsafe sealed class MediaItem
    {
        public MediaItem(string videoUrl)
        {
            VideoUrl = videoUrl;
            Headers = new Dictionary<string, string>();
        }
        public MediaItem(string videoUrl, Dictionary<string, string> headers)
        {
            Headers = headers;
            VideoUrl = videoUrl;
        }
        public MediaItem(string videoUrl, string audioUrl) : this(videoUrl)
        {
            AudioUrl = audioUrl;
        }
        public MediaItem(string videoUrl, string audioUrl, Dictionary<string, string> headers) : this(videoUrl, headers)
        {
            AudioUrl = audioUrl;
        }
        public string VideoUrl { get; set; }
        public string AudioUrl { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public unsafe AVDictionary* ToHeader() => Headers.ToHeader();
    }
}
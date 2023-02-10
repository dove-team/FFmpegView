using FFmpeg.AutoGen;
using System.Collections.Generic;
using System.Text;

namespace FFmpegView
{
    public static class Untils
    {
        public unsafe static AVDictionary* ToHeader(this Dictionary<string, string> headers)
        {
            AVDictionary* options = null;
            StringBuilder builder = new StringBuilder();
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                    builder.Append($"{header.Key}: {header.Value}\r\n");
            }
            ffmpeg.av_dict_set(&options, "headers", builder.ToString(), 0);
            return options;
        }
    }
}
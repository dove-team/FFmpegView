using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FFmpegView
{
    public static class Ext
    {
        public static unsafe AVDictionary* ToHeader(this Dictionary<string, string> headers)
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
        public static unsafe string av_strerror(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            return Marshal.PtrToStringAnsi((IntPtr)buffer);
        }
        public static int ThrowExceptionIfError(this int error)
        {
            if (error < 0)
                throw new ApplicationException(av_strerror(error));
            return error;
        }
    }
}
using System;

namespace FFmpegView
{
    public interface IMedia
    {
        public delegate void MediaHandler(TimeSpan duration);
        public event MediaHandler MediaCompleted;
        public delegate void MediaMsgHandler(MsgType type, string msg);
        public event MediaMsgHandler MediaMsgRecevice;
    }
}
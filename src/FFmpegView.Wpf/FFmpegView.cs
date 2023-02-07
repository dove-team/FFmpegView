using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FFmpegView.Wpf
{
    public unsafe class FFmpegView : ContentControl, IFFmpegView
    {
        private IntPtr buffer;
        private Task playTask;
        private Task audioTask;
        private readonly Image image;
        private readonly bool isInit = false;
        private AudioStreamDecoder audio;
        private readonly TimeSpan timeout;
        private WriteableBitmap writeableBitmap;
        private readonly VideoStreamDecoder video;
        private CancellationTokenSource cancellationToken;
        public FFmpegView()
        {
            image = new Image();
            Content = image;
            video = new VideoStreamDecoder();
            timeout = TimeSpan.FromTicks(10000);
            video.MediaCompleted += VideoMediaCompleted;
            video.MediaMsgRecevice += Video_MediaMsgRecevice;
            isInit = Init();
        }
        private void Video_MediaMsgRecevice(MsgType type, string msg) =>
            Debug.WriteLine($"{(type == MsgType.Error ? "Error: " : "Info: ")}{msg}");
        public void SetAudioHandler(AudioStreamDecoder decoder) => audio = decoder;
        private void VideoMediaCompleted(TimeSpan duration)
        {
            Dispatcher.InvokeAsync(DisplayVideoInfo);
        }
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

        }
        private void DrawImage(AVFrame convertedFrame)
        {
            try
            {
                buffer = (IntPtr)convertedFrame.data[0];
                int width = convertedFrame.width, height = convertedFrame.height;
                Dispatcher.InvokeAsync(() =>
                {
                    writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), buffer, bufferSize, convertedFrame.linesize[0], 0, 0);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public bool Pause()
        {
            try
            {
                audio?.Pause();
                return video.Pause();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        public bool Play()
        {
            bool state = false;
            try
            {
                state = video.Play();
                audio?.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return state;
        }
        public bool Play(string uri, Dictionary<string, string> headers = null)
        {
            if (!isInit)
            {
                Debug.WriteLine("FFmpeg : dosnot initialize device");
                return false;
            }
            bool state = false;
            try
            {
                if (video.State == MediaState.None)
                {
                    video.Headers = headers;
                    video.InitDecodecVideo(uri);
                    audio?.InitDecodecAudio(uri);
                    DisplayVideoInfo();
                    writeableBitmap = new WriteableBitmap(frameWidth, frameHeight, 96, 96, PixelFormats.Bgra32, null);
                    image.Source = writeableBitmap;
                }
                state = video.Play();
                audio?.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return state;
        }
        public void SetHeader(Dictionary<string, string> headers) => video.Headers = headers;
        public bool SeekTo(int seekTime)
        {
            try
            {
                audio?.SeekProgress(seekTime);
                return video.SeekProgress(seekTime);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        public bool Stop()
        {
            try
            {
                audio?.Stop();
                video.Stop();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        bool Init()
        {
            try
            { 
                cancellationToken = new CancellationTokenSource();
                playTask = new Task(() =>
                {
                    while (true)
                    {
                        try
                        {
                            if (video.IsPlaying && IsVisible)
                            {
                                if (video.TryReadNextFrame(out var frame))
                                {
                                    var convertedFrame = video.FrameConvert(&frame);
                                    DrawImage(convertedFrame);
                                }
                            }
                            Thread.Sleep(10); 
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }, cancellationToken.Token);
                playTask.Start();
                audioTask = new Task(() =>
                {
                    while (true)
                    { 
                        try
                        {
                            if (audio?.IsPlaying == true)
                            {
                                if (audio?.TryPlayNextFrame() == true)
                                    Thread.Sleep(audio.frameDuration.Subtract(timeout));
                            }
                            else
                                Thread.Sleep(10);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }, cancellationToken.Token);
                audioTask.Start();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FFmpeg Failed Init: " + ex.Message);
                return false;
            }
        }
        #region 视频信息
        private string codec;
        public string Codec => codec;
        private TimeSpan duration;
        public TimeSpan Duration => duration;
        private double videoFps;
        public double VideoFps => videoFps;
        private int bufferSize;
        private int frameHeight;
        public int FrameHeight => frameHeight;
        private int frameWidth;
        public int FrameWidth => frameWidth;
        private int videoBitrate;
        public int VideoBitrate => videoBitrate;
        private double sampleRate;
        public double SampleRate => sampleRate;
        private long audioBitrate;
        public long AudioBitrate => audioBitrate;
        private long audioBitsPerSample;
        public long AudioBitsPerSample => audioBitsPerSample;
        void DisplayVideoInfo()
        {
            try
            {
                duration = video.Duration;
                codec = video.CodecName;
                videoBitrate = video.Bitrate;
                frameWidth = video.FrameWidth;
                frameHeight = video.FrameHeight;
                bufferSize = frameWidth * frameHeight * 4;
                videoFps = video.FrameRate;
                if (audio != null)
                {
                    audioBitrate = audio.Bitrate;
                    sampleRate = audio.SampleRate;
                    audioBitsPerSample = audio.BitsPerSample;
                }
            }
            catch { }
        }
        #endregion
    }
}
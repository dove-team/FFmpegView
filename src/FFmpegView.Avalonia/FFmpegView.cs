using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Logging;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using PCLUntils.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegView
{
    [PseudoClasses(":empty")]
    [TemplatePart("PART_ImageView", typeof(Image))]
    public unsafe class FFmpegView : TemplatedControl, IFFmpegView
    {
        private Image image;
        private Task playTask;
        private Task audioTask;
        private Bitmap bitmap;
        private bool _isAttached = false;
        private readonly bool isInit = false;
        private AudioStreamDecoder audio;
        private readonly TimeSpan timeout;
        private readonly VideoStreamDecoder video;
        private CancellationTokenSource cancellationToken;
        public static readonly StyledProperty<Stretch> StretchProperty =
            AvaloniaProperty.Register<FFmpegView, Stretch>(nameof(Stretch), Stretch.Uniform);
        /// <summary>
        /// Gets or sets a value controlling how the video will be stretched.
        /// </summary>
        public Stretch Stretch
        {
            get => GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);
            try
            {
                cancellationToken.Cancel();
                playTask.Dispose();
                audioTask.Dispose();
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, ex.Message);
            }
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _isAttached = true;
            base.OnAttachedToVisualTree(e);
        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _isAttached = false;
            base.OnDetachedFromVisualTree(e);
        }
        static FFmpegView()
        {
            StretchProperty.Changed.AddClassHandler<FFmpegView>(OnStretchChange);
        }
        public void SetAudioHandler(AudioStreamDecoder decoder) => audio = decoder;
        private static void OnStretchChange(FFmpegView sender, AvaloniaPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue is Stretch stretch)
                    sender.image.Stretch = stretch;
            }
            catch { }
        }
        public FFmpegView()
        {
            video = new VideoStreamDecoder();
            timeout = TimeSpan.FromTicks(10000);
            video.MediaCompleted += VideoMediaCompleted;
            video.MediaMsgRecevice += Video_MediaMsgRecevice;
            isInit = Init();
        }
        private void Video_MediaMsgRecevice(MsgType type, string msg)
        {
            if (type == MsgType.Error)
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, msg);
            else
                Logger.TryGet(LogEventLevel.Information, LogArea.Control)?.Log(this, msg);
        }
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            image = e.NameScope.Get<Image>("PART_ImageView");
        }
        private void VideoMediaCompleted(TimeSpan duration) =>
                    Dispatcher.UIThread.InvokeAsync(DisplayVideoInfo);
        public double? Position => video?.Position.TotalSeconds;
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
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, ex.Message);
            }
            return state;
        }
        public bool Play(string uri)
        {
            if (!isInit)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, "FFmpeg : dosnot initialize device");
                return false;
            }
            bool state = false;
            try
            {
                if (video.State == MediaState.None)
                {
                    video.InitDecodecVideo(uri);
                    audio?.InitDecodecAudio(uri);
                    audio?.Prepare();
                    DisplayVideoInfo();
                }
                state = video.Play();
                audio?.Play();
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, ex.Message);
            }
            return state;
        }
        public bool SeekTo(int seekTime)
        {
            try
            {
                audio?.SeekProgress(seekTime);
                return video.SeekProgress(seekTime);
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, ex.Message);
                return false;
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
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, ex.Message);
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
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, ex.Message);
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
                            if (video.IsPlaying && _isAttached)
                            {
                                if (video.TryReadNextFrame(out var frame))
                                {
                                    var convertedFrame = video.FrameConvert(&frame);
                                    bitmap?.Dispose();
                                    bitmap = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, (IntPtr)convertedFrame.data[0], new PixelSize(video.FrameWidth, video.FrameHeight), new Vector(96, 96), convertedFrame.linesize[0]);
                                    Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        if (image.IsNotEmpty())
                                            image.Source = bitmap;
                                    });
                                }
                            }
                            Thread.Sleep(10);
                        }
                        catch (Exception ex)
                        {
                            Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, ex.Message);
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
                            Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, ex.Message);
                        }
                    }
                }, cancellationToken.Token);
                audioTask.Start();
                return true;
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this, "FFmpeg Failed Init: " + ex.Message);
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
        private double frameHeight;
        public double FrameHeight => frameHeight;
        private double frameWidth;
        public double FrameWidth => frameWidth;
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
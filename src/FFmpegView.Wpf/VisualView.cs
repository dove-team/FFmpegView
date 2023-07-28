using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using FFmpeg.AutoGen;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;

namespace FFmpegView.Wpf
{
    internal unsafe class VisualView : UIElement
    {
        private readonly  DrawingVisual visual;
        private WriteableBitmap writeableBitmap;
        private readonly FFmpegVisualView Owner;
        public VisualView(FFmpegVisualView owner)
        {
            Owner = owner;
            visual = new DrawingVisual();
        }
        public void Initialize()
        {
            int width = Owner.FrameWidth, height = Owner.FrameHeight;
            writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            using DrawingContext dc = visual.RenderOpen();
            dc.DrawImage(writeableBitmap, new Rect(0, 0, width, height));
        }
        protected override Visual GetVisualChild(int index) => visual;
        protected override int VisualChildrenCount => 1;
#if NET40_OR_GREATER
        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
#endif
        public void Draw(AVFrame convertedFrame)
        {
            try
            {
                var buffer = (IntPtr)convertedFrame.data[0];
                int width = convertedFrame.width, height = convertedFrame.height;
                Dispatcher.InvokeAsync(() =>
                {
                    writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), buffer, Owner.bufferSize, convertedFrame.linesize[0], 0, 0);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Draw: " + ex.Message);
            }
        }
    }
}
using Avalonia.Controls;
using Avalonia.Media;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using System;
using System.Threading;

namespace SearchAThing
{
    public abstract class OpenGlControlBase : Control
    {
        protected IWindow glWindow;
        protected GL GL;
        Rect prevBounds;

        readonly TimeSpan resizeFlushTimeout = TimeSpan.FromMilliseconds(1000);
        readonly TimeSpan resizeFlushInterval = TimeSpan.FromMilliseconds(10);
        DateTime resizeFlushStart;

        public OpenGlControlBase()
        {
            prevBounds = Bounds;
        }

        protected abstract void OnOpenGlInit();
        protected abstract void OnOpenGlDeinit();
        protected abstract void OnOpenGlRender();

        public Size BitmapSize
        {
            get
            {
                if (glWindow == null) return new Size();
                return new Size(glWindow.Size.Width, glWindow.Size.Height);
            }
        }

        Thread th = null;
        object thLck = new object();

        void UpdateViewport()
        {
            var bounds = Bounds;
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                glWindow.Size = new System.Drawing.Size((int)bounds.Width, (int)bounds.Height);
                GL.Viewport(0, 0, (uint)bounds.Width, (uint)bounds.Height);
            }
        }

        void CheckResize()
        {
            if (glWindow == null) return;

            var bounds = Bounds;

            if (!prevBounds.Equals(bounds))
            {
                prevBounds = bounds;

                UpdateViewport();                

                if (th == null)
                {
                    lock (thLck)
                    {
                        if (th == null)
                        {
                            th = new Thread(() =>
                            {
                                resizeFlushStart = DateTime.Now;
                                while ((DateTime.Now - resizeFlushStart) < resizeFlushTimeout)
                                {
                                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                                    {
                                        UpdateViewport();
                                        InvalidateVisual();
                                    });
                                    Thread.Sleep(resizeFlushInterval);
                                }
                                th = null;
                            });
                            th.Start();
                        }
                    }
                }
                else resizeFlushStart = DateTime.Now;
            }
        }

        protected unsafe void GlRender(double obj)
        {
            CheckResize();

            OnOpenGlRender();
        }

        public unsafe sealed override void Render(DrawingContext context)
        {
            if (glWindow == null)
            {
                if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

                var options = WindowOptions.Default;
                options.Size = new System.Drawing.Size((int)Bounds.Width, (int)Bounds.Height);
                options.IsVisible = true;

                glWindow = Silk.NET.Windowing.Window.Create(options);
                glWindow.WindowState = Silk.NET.Windowing.Common.WindowState.Minimized;
                glWindow.Render += GlRender;

                glWindow.Initialize();

                GL = GL.GetApi(glWindow);

                OnOpenGlInit();
            }

            CheckResize();

            OnOpenGlRender();

            if (glWindow.Size.Width > 0 && glWindow.Size.Height > 0)
            {
                using (var bitmap = new WriteableBitmap(
                    new Avalonia.PixelSize(glWindow.Size.Width, glWindow.Size.Height),
                    new Avalonia.Vector(96.0, 96.0),
                    new Avalonia.Platform.PixelFormat?(Avalonia.Platform.PixelFormat.Rgba8888)))
                {

                    using (var l = bitmap.Lock())
                    {
                        GL.PixelStore(PixelStoreParameter.PackRowLength, l.RowBytes / 4);

                        GL.ReadPixels(0, 0, (uint)glWindow.Size.Width, (uint)glWindow.Size.Height,
                            GLEnum.Bgra, GLEnum.UnsignedByte, l.Address.ToPointer());
                    }

                    context.DrawImage(bitmap,
                        new Avalonia.Rect(bitmap.Size),
                        new Avalonia.Rect(bitmap.Size),
                        BitmapInterpolationMode.LowQuality);

                    //bitmap.Save("testout.png");
                }
            }

            base.Render(context);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            if (glWindow != null)
            {
                if (th != null)
                {
                    th.Join();
                }
                this.OnOpenGlDeinit();

                glWindow.MakeCurrent();

                glWindow.Dispose();
                glWindow = null;
            }

            base.OnDetachedFromVisualTree(e);
        }

    }
}

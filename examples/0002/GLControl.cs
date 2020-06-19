using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using System;
using System.Threading;

namespace SearchAThing
{

    namespace Sci
    {


        public class GLControl : Control
        {
            public GameWindow win = null;

            const int INITIAL_W = 200;
            const int INITIAL_H = 300;

            Vector2i GLCONTROL_SIZE = new Vector2i(INITIAL_W, INITIAL_H);

            protected virtual void GetFrame(int w, int h)
            {

            }

            public GLControl()
            {
                this.IsHitTestVisible = false;

                this.LayoutUpdated += (a, b) =>
                {
                    System.Console.WriteLine($"=== LayoutUpdated winsize:{win.Size}");
                };           
            }

            protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
            {
                System.Console.WriteLine($"=== OnAttachedToVisualTree()");

                var gameWindowSettings = new GameWindowSettings
                {
                };
                var nativeWindowSettings = new NativeWindowSettings
                {
                    Size = new OpenToolkit.Mathematics.Vector2i(INITIAL_W, INITIAL_H)
                };

                GLCONTROL_SIZE = nativeWindowSettings.Size;
                win = new GameWindow(gameWindowSettings, nativeWindowSettings);
                win.IsVisible = true;
                win.Resize += (args) =>
                {
                    var w = args.Width;
                    var h = args.Height;
                    System.Console.WriteLine($"win.Resize event ({w},{h}) ; (in.Size={win.Size} win.ClientSize.Size{win.ClientSize})");
                    if (w != GLCONTROL_SIZE.X || h != GLCONTROL_SIZE.Y)
                    {
                        // GL.Viewport(0, 0, w, h);
                        // GL.Flush();
                        // win.MakeCurrent();
                        // InvalidateVisual();
                    }
                    //glreset();
                };
                makeCur();
                if (AutoMode)
                {
                    vport();
                    doFrame();
                    glflush();
                    swapbuff();
                    InvalidateVisual();
                }
            }

            protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
            {
                System.Console.WriteLine($"=== OnDetachedFromVisualTree()");
            }

            protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
            {
                System.Console.WriteLine($"=== MeasureOverride(availableSize:{availableSize})");

                var w = (int)availableSize.Width;
                var h = (int)availableSize.Height;
                win.Size = new OpenToolkit.Mathematics.Vector2i(w, h);
                GLCONTROL_SIZE = win.Size;
                System.Console.WriteLine($"  win.Size:{win.Size}");
                //if (win.IsVisible) win.SwapBuffers();
                //win.MakeCurrent();

                if (AutoMode)
                {
                    makeCur();

                    InvalidateVisual();
                }

                win.ProcessEvents();

                return availableSize;
            }

            bool first_render = true;

            public bool AutoMode { get; set; }

            public void glflush()
            {
                System.Console.WriteLine($"GL.Flush()");
                GL.Flush();
            }

            public void swapbuff()
            {
                System.Console.WriteLine($"win.SwapBuffers()");
                win.SwapBuffers();
            }

            public void vport()
            {
                var w = GLCONTROL_SIZE.X;
                var h = GLCONTROL_SIZE.Y;
                System.Console.WriteLine($"GL.Viewport(0,0,{w},{h})");
                GL.Viewport(0, 0, w, h);
            }

            public void doFrame()
            {
                var w = GLCONTROL_SIZE.X;
                var h = GLCONTROL_SIZE.Y;
                System.Console.WriteLine($"GetFrame({w},{h})");
                GetFrame(w, h);
            }

            public void makeCur()
            {
                System.Console.WriteLine($"win.MakeCurrent()");
                win.MakeCurrent();
            }

            bool makecurRequired = false;

            public override void Render(Avalonia.Media.DrawingContext context)
            {
                var w = GLCONTROL_SIZE.X;
                var h = GLCONTROL_SIZE.Y;

                System.Console.WriteLine($"=== RENDER w:{w} h:{h} ; winsize:{win.Size} clientSize:{win.ClientSize}");

                if (AutoMode)
                {
                    int iw, ih;

                    win.GetFramebufferSize(out iw, out ih);
                    System.Console.WriteLine($"Internal framebuffer size w:{iw} ih:{ih}");

                    if (makecurRequired)
                    {
                        System.Console.WriteLine($"ENFORCE MAKECUR");
                        makeCur();
                        makecurRequired = false;
                    }
                    
                    vport();
                    doFrame();
                    glflush();
                    swapbuff();
                } 

                using (var bitmap = new WriteableBitmap(
                    new Avalonia.PixelSize(w, h),
                    new Avalonia.Vector(96.0, 96.0),
                    new Avalonia.Platform.PixelFormat?(Avalonia.Platform.PixelFormat.Rgba8888)))
                {

                    using (var l = bitmap.Lock())
                    {
                        GL.PixelStore(PixelStoreParameter.PackRowLength, l.RowBytes / 4);
                        GL.ReadPixels(0, 0, w, h, PixelFormat.Bgra, PixelType.UnsignedByte, l.Address);
                    }

                    context.DrawImage(bitmap, 1.0,
                        new Avalonia.Rect(bitmap.Size),
                        new Avalonia.Rect(bitmap.Size),
                        BitmapInterpolationMode.LowQuality);
                }
                //bitmap.Save("/home/devel0/Desktop/testout.bmp.png");
            }

        }

    }

}
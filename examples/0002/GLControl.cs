using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Desktop;
using System.Threading;

namespace SearchAThing
{

    namespace Sci
    {


        public class GLControl : Control
        {
            public GameWindow win = null;

            int w = 640;
            int h = 480;

            protected virtual void GetFrame(int w, int h)
            {

            }

            public GLControl()
            {
                this.IsHitTestVisible = false;
            }

            protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
            {
                System.Console.WriteLine($"=== ATTACHED TO VISUAL TREE");

                var gameWindowSettings = new GameWindowSettings
                {
                };
                var nativeWindowSettings = new NativeWindowSettings
                {
                    Size = new OpenToolkit.Mathematics.Vector2i(w, h)
                };

                win = new GameWindow(gameWindowSettings, nativeWindowSettings);
                win.IsVisible = false;
             /*  win.Resize += (args) =>
                {                    
                    w = args.Width;
                    h = args.Height;
                    System.Console.WriteLine($"resize to w:{w} h:{h}");
                    GL.Viewport(0, 0, w, h);
                    GL.Flush();
                    win.MakeCurrent();
                };*/
                win.MakeCurrent();
            }

            protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
            {
                System.Console.WriteLine($"=== DETACHED FROM VISUAL TREE");
            }

            protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
            {
                System.Console.WriteLine($"=== MEASURE OVERRIDE");

                System.Console.WriteLine($"MeasureOverride size:{availableSize}");
                w = (int)availableSize.Width;
                h = (int)availableSize.Height;
                win.Size = new OpenToolkit.Mathematics.Vector2i(w, h);
                System.Console.WriteLine($"win.Size:{win.Size}");
                //win.MakeCurrent();

                
                //if (win.IsVisible) win.SwapBuffers();

                //win.ProcessEvents();


                return availableSize;
            }

            public override void Render(Avalonia.Media.DrawingContext context)
            {
                System.Console.WriteLine($"=== RENDER");

                this.GetFrame(w, h);
                GL.Flush();
                //if (win.IsVisible) win.SwapBuffers();

                using (var bitmap = new WriteableBitmap(
                    new Avalonia.PixelSize(w, h),
                    new Avalonia.Vector(96.0, 96.0),
                    new Avalonia.Platform.PixelFormat?(Avalonia.Platform.PixelFormat.Rgba8888)))
                {

                    using (var l = bitmap.Lock())
                    {
                        GL.PixelStore(PixelStoreParameter.PackRowLength, l.RowBytes / 4);
                        GL.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.UnsignedByte, l.Address);
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
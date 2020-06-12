using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SearchAThing
{

    namespace Sci
    {


        public class GLControl : Control
        {
            //public GameWindow win = null;

            int w = 640;
            int h = 480;

            protected virtual void GetFrame(int w, int h)
            {

            }

            public GLControl()
            {
                this.IsHitTestVisible = false;
            }

            /// <summary>
            /// opentk (https://github.com/opentk/opentk/blob/b068aac8cf36ba864b239f176f993e3889501579/src/OpenToolkit.Windowing.Desktop/NativeWindow.cs#L611)
            /// </summary>
            private static void InitializeGlBindings()
            {
                // We don't put a hard dependency on OpenToolkit.Graphics here.
                // So we need to use reflection to initialize the GL bindings, so users don't have to.

                // Try to load OpenToolkit.Graphics assembly.
                Assembly assembly;
                try
                {
                    assembly = Assembly.Load("OpenToolkit.Graphics");
                }
                catch
                {
                    // Failed to load graphics, oh well.
                    // Up to the user I guess?
                    // TODO: Should we expose this load failure to the user better?
                    return;
                }

                var provider = new GLFWBindingsContext();

                void LoadBindings(string typeNamespace)
                {
                    var type = assembly.GetType($"OpenToolkit.Graphics.{typeNamespace}.GL");
                    if (type == null)
                    {
                        return;
                    }

                    var load = type.GetMethod("LoadBindings");
                    load.Invoke(null, new object[] { provider });
                }

                LoadBindings("ES11");
                LoadBindings("ES20");
                LoadBindings("ES30");
                LoadBindings("OpenGL");
                LoadBindings("OpenGL4");
            }

            protected unsafe OpenToolkit.Windowing.GraphicsLibraryFramework.Window* WindowPtr { get; private set; }

            protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
            {
                System.Console.WriteLine($"=== ATTACHED TO VISUAL TREE");

                GLFW.Init();
                GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
                GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
                GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

                // https://www.glfw.org/docs/latest/context.html#context_offscreen
                GLFW.WindowHint(WindowHintBool.Visible, false);                
                unsafe
                {
                    WindowPtr = GLFW.CreateWindow(640, 480, "", null, null);
                    //GLFW.IconifyWindow(WindowPtr);
                    GLFW.MakeContextCurrent(WindowPtr);
                    //GLFW.HideWindow(WindowPtr);
                    // GLFW.SetWindowSizeCallback(WindowPtr, (wptr, ww, wh) =>
                    // {
                    //     Console.WriteLine($"window resized to w:{w} h:{h}");
                    // });
                }

                InitializeGlBindings();

                // DebugProc cback = (src, type, id, severity, length, message, userParam) =>
                // {
                //     var msg = Marshal.PtrToStringAuto(message);
                //     System.Console.WriteLine($"GL ERROR:" + msg);
                // };
                // GL.Enable(EnableCap.DebugOutput);
                // GL.DebugMessageCallback(cback, (IntPtr)null);

                // var gameWindowSettings = new GameWindowSettings
                // {
                // };
                // var nativeWindowSettings = new NativeWindowSettings
                // {
                //     Size = new OpenToolkit.Mathematics.Vector2i(w, h)
                // };

                // win = new GameWindow(gameWindowSettings, nativeWindowSettings);
                // win.IsVisible = true;
                // win.MakeCurrent();
            }

            //protected 

            protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
            {
                System.Console.WriteLine($"=== DETACHED FROM VISUAL TREE");
            }

            protected unsafe void MakeCurrentContext()
            {
                GLFW.MakeContextCurrent(WindowPtr);
            }

            protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
            {
                System.Console.WriteLine($"=== MEASURE OVERRIDE");

                System.Console.WriteLine($"MeasureOverride size:{availableSize}");
                w = (int)availableSize.Width;
                h = (int)availableSize.Height;
                //win.Size = new OpenToolkit.Mathematics.Vector2i(w, h);
                //System.Console.WriteLine($"win.Size:{win.Size}");
                //win.MakeCurrent();

                unsafe
                {
                    GLFW.SetWindowSizeLimits(WindowPtr, 0, 0, w, h);
                    GLFW.SetWindowSize(WindowPtr, w, h);                                        
                    GLFW.MakeContextCurrent(WindowPtr);
                }
                unsafe
                {
                    //GLFW.SwapBuffers(WindowPtr);
                }
                GL.Viewport(0, 0, w, h);
                //if (win.IsVisible) win.SwapBuffers();

                return availableSize;
            }

            public override void Render(Avalonia.Media.DrawingContext context)
            {
                System.Console.WriteLine($"=== RENDER");

                this.GetFrame(w, h);
                GL.Flush();
                //MakeCurrentContext();
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
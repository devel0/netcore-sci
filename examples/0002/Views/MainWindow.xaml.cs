using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using SearchAThing.Sci;

namespace example_avalonia_opengl.Views
{
    public class MainWindow : Avalonia.Controls.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

        }

        private void click1(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            var th = new Thread(() =>
            {
                while (true)
                {
                    ctl.cnt++;
                    // unsafe
                    // {
                    //     GLFW.MakeContextCurrent(null);
                    // }
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        //ctl.win.MakeCurrent();
                        ctl.InvalidateVisual();
                    });                    
                    Thread.Sleep(20);
                }
            });
            th.Start();
        }
    }
}
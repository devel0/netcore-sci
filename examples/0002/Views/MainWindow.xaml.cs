using System.ComponentModel;
using System.Drawing;
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
    public class MainWindow : Avalonia.Controls.Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Width = 200;
            Height = 300;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        bool _AutoMode = false;
        public bool AutoMode
        {
            get { return _AutoMode; }
            set
            {
                _AutoMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoMode"));

                var ctl = this.FindControl<GLControlTest>("glctl");
                ctl.AutoMode = value;
            }
        }

        private void click2(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            ctl.glflush();
        }


        private void click3(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            ctl.swapbuff();
        }

        private void click4(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            ctl.vport();
        }

        private void click5(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            ctl.doFrame();
        }

        private void click6(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            ctl.makeCur();
        }

        private void click7(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            ctl.InvalidateVisual();
        }

        private void click7a(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            ctl.makeCur();
            ctl.vport();
            ctl.doFrame();
            ctl.glflush();
            ctl.swapbuff();
            ctl.InvalidateVisual();
        }

        private void click7b(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            //ctl.makeCur();
            ctl.vport();
            ctl.doFrame();
            ctl.glflush();
            ctl.swapbuff();
            ctl.InvalidateVisual();
        }

        private void click1(object sender, RoutedEventArgs e)
        {
            var ctl = this.FindControl<GLControlTest>("glctl");

            if (ctl.color == Color.Red)
                ctl.color = Color.Green;
            else if (ctl.color == Color.Green)
                ctl.color = Color.Blue;
            else ctl.color = Color.Red;

            //ctl.glreset();
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SearchAThing.Sci;

namespace example_avalonia_opengl.Views
{
    public class MainWindow : Window
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
            var ctl = this.FindControl<GLControl>("glctl");

            ctl.win.MakeCurrent();

            ctl.InvalidateVisual();
        }
    }
}
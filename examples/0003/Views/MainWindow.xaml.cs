using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SearchAThing.Sci;

namespace Example.Views
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
            //var ctl = this.FindControl<OpenGlPageControl>("glctl");

             
        }
    }
}
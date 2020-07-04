using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SearchAThing.SciExamples.Controls;

namespace SearchAThing.SciExamples.Views
{
    public class MainWindow : Window
    {
        OpenGlControl glCtl = null;

        public MainWindow()
        {
            InitializeComponent();
            glCtl = this.FindControl<OpenGlControl>("GL");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void click_reset(object sender, RoutedEventArgs e)
        {
            glCtl.Reset();
        }

    }
}
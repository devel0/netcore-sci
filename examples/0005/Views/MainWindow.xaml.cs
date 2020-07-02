using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SearchAThing.SciExamples.Views
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
    }
}
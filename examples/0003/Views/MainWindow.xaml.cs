using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
    }
}
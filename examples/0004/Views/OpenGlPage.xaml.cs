using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SearchAThing.Sci.Lab.example0004;
// ReSharper disable StringLiteralTypo

namespace Example.Views
{
    public class OpenGlPage : UserControl
    {

        OpenGlPageControl glCtl = null;

        public OpenGlPage()
        {
            AvaloniaXamlLoader.Load(this);
            glCtl = this.FindControl<OpenGlPageControl>("GL");
        }

        private void click_reset(object sender, RoutedEventArgs e)
        {
            glCtl.Reset();
        }
    }
}

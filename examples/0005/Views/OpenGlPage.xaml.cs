using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
// ReSharper disable StringLiteralTypo

namespace SearchAThing.SciExamples.Views
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

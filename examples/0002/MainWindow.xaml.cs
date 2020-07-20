using System;
using System.Numerics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Window = Avalonia.Controls.Window;

namespace SearchAThing.SciExamples
{
    public class MainWindow : Window
    {

        SampleGlControl glCtl;

        public MainWindow()
        {
            InitializeComponent();

            glCtl = this.FindControl<SampleGlControl>("glCtl");
        }

        private void click_random(object sender, RoutedEventArgs e)
        {
            var rnd = new Random();
            var color = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
            glCtl.ObjColor = color;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
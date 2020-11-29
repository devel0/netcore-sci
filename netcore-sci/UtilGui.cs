using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.Styling;
using SearchAThing.Gui;
using UnitsNet;

namespace SearchAThing
{
    namespace Gui
    {

        /// <summary>
        /// helper superclass for multiplatform app<br/>
        /// example:<br/>
        /// public class MainWindow : Win<br/>
        /// {<br/>
        /// public MainWindow() : base(new[]<br/>
        /// {<br/>
        /// "resm:OxyPlot.Avalonia.Themes.Default.xaml?assembly=OxyPlot.Avalonia"<br/>
        /// })<br/>
        /// {<br/>
        /// var grRoot = new Grid() { DataContext = m, Margin = new Thickness(10) };
        /// this.Content = grRoot;   
        /// }<br/>
        /// }<br/>
        /// </summary>
        public class Win : Window
        {
            public IEnumerable<string> CustomStyleUris;

            public Win(IEnumerable<string> customStyles)
            {
                CustomStyleUris = customStyles;
            }
        }

        /// <summary>
        /// helper to manage multiplatform app
        /// <typeparam name="W"></typeparam>
        public class App<W> : Application
            where W : Win, new()
        {
            public App()
            {
            }

            public override void Initialize()
            {
                base.Initialize();
                this.Styles.AddRange(new[]
                {
                    new StyleInclude(baseUri:null) { Source = new Uri("resm:Avalonia.Themes.Default.DefaultTheme.xaml?assembly=Avalonia.Themes.Default") },
                    new StyleInclude(baseUri:null) { Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default") },
                });
            }

            public override void OnFrameworkInitializationCompleted()
            {
                var desktop = this.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                var w = new W();
                desktop.MainWindow = w;
                foreach (var s in w.CustomStyleUris)
                {
                    this.Styles.Add(
                        new StyleInclude(baseUri: null) { Source = new Uri(s) }
                    );
                }
                base.OnFrameworkInitializationCompleted();
            }
        }
    }

    public static class GuiToolkit
    {

        /// <summary>
        /// helper to create multiplatform window<br/>
        /// example<br/>
        /// GuiToolkit.CreateGui<MainWindow>();<br/>
        /// </summary>
        /// <typeparam name="W"></typeparam>
        public static void CreateGui<W>() where W : Win, new()
        {
            var buildAvaloniaApp = AppBuilder.Configure<App<W>>().UsePlatformDetect();

            buildAvaloniaApp.StartWithClassicDesktopLifetime(new string[] { });
        }

    }

    /// <summary>
    /// generic UnitsNet converter helper<br/>    
    /// example:<br/>
    /// var durationConverter = new QuantityConverter((s, c) => Duration.Parse(s, c));<br/>
    /// var tbox = new TextBox { MinWidth = 100, Margin = new Thickness(10, 0, 0, 0) };<br/>
    /// tbox[!TextBox.TextProperty] = new Binding(propname) { Mode = BindingMode.TwoWay, Converter = cvt };
    /// </summary>
    public class QuantityConverter : IValueConverter
    {
        Func<string, CultureInfo, object> parse;
        public QuantityConverter(Func<string, CultureInfo, object> parse)
        {
            this.parse = parse;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return parse((string)value, culture);
            }
            catch
            {
                return value;
            }
        }
    }
}
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

        #region ColRed

        private float _colRed = 0f;
        public static readonly DirectProperty<MainWindow, float> ColRedProperty =
            AvaloniaProperty.RegisterDirect<MainWindow, float>("ColRed", o => o.ColRed, (o, v) => o.ColRed = v);
        public float ColRed
        {
            get => _colRed;
            private set
            {
                glCtl.ObjColor = new Vector3(value, glCtl.ObjColor.Y, glCtl.ObjColor.Z);
                SetAndRaise(ColRedProperty, ref _colRed, value);
                InvalidateVisual();
            }
        }

        #endregion

        #region ColGreen

        private float _colGreen = 0f;
        public static readonly DirectProperty<MainWindow, float> ColGreenProperty =
            AvaloniaProperty.RegisterDirect<MainWindow, float>("ColGreen", o => o.ColGreen, (o, v) => o.ColGreen = v);
        public float ColGreen
        {
            get => _colGreen;
            private set
            {
                glCtl.ObjColor = new Vector3(glCtl.ObjColor.X, value, glCtl.ObjColor.Z);
                SetAndRaise(ColGreenProperty, ref _colGreen, value);
                InvalidateVisual();
            }
        }

        #endregion

        #region ColBlue

        private float _colBlue = 0f;
        public static readonly DirectProperty<MainWindow, float> ColBlueProperty =
            AvaloniaProperty.RegisterDirect<MainWindow, float>("ColBlue", o => o.ColBlue, (o, v) => o.ColBlue = v);
        public float ColBlue
        {
            get => _colBlue;
            private set
            {
                glCtl.ObjColor = new Vector3(glCtl.ObjColor.X, glCtl.ObjColor.Y, _colBlue);
                SetAndRaise(ColBlueProperty, ref _colBlue, value);
                InvalidateVisual();
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            glCtl = this.FindControl<SampleGlControl>("glCtl");
        }

        private void click_random(object sender, RoutedEventArgs e)
        {
            var rnd = new Random();
            var color = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
            ColRed = color.X;
            ColGreen = color.Y;
            ColBlue = color.Z;
            glCtl.ObjColor = color;
        }

        // private void invVisClick(object sender, RoutedEventArgs e)
        // {
        //     glCtl.InvalidateVisual();
        // }

        // private void UpdateViewportClick(object sender, RoutedEventArgs e)
        // {
        //     glCtl.UpdateViewport();
        // }

        private unsafe void OnLoad()
        {
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnInitialized()
        {             
        }

    }
}
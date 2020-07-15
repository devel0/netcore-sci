using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using static System.Math;

namespace SearchAThing
{

    public class Vector3Control : UserControl
    {

        private bool _Slider = true;
        public static readonly DirectProperty<Vector3Control, bool> SliderProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, bool>("Slider", o => o.Slider, (o, v) => o.Slider = v);
        public bool Slider
        {
            get => _Slider;
            set => SetAndRaise(SliderProperty, ref _Slider, value);
        }

        #region VMIN

        private Vector3 _VMIN;
        public static readonly DirectProperty<Vector3Control, Vector3> VMINProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, Vector3>("VMIN", o => o.VMIN, (o, v) => o.VMIN = v);
        public Vector3 VMIN
        {
            get => _VMIN;
            set
            {
                SetAndRaise(VMINProperty, ref _V, value);
                SetAndRaise(VMINXProperty, ref _VMINX, value.X);
                SetAndRaise(VMINYProperty, ref _VMINY, value.Y);
                SetAndRaise(VMINZProperty, ref _VMINZ, value.Z);
                UpdateTip();
            }
        }

        private float _VMINX;
        public static readonly DirectProperty<Vector3Control, float> VMINXProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VMINX", o => o.VMINX, (o, v) => o.VMINX = v);
        public float VMINX
        {
            get => _VMINX;
            set
            {
                SetAndRaise(VMINXProperty, ref _VMINX, value);
                SetAndRaise(VMINProperty, ref _VMIN, new Vector3(value, VMINY, VMINZ));
            }
        }

        private float _VMINY;
        public static readonly DirectProperty<Vector3Control, float> VMINYProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VMINY", o => o.VMINY, (o, v) => o.VMINY = v);
        public float VMINY
        {
            get => _VMINY;
            set
            {
                SetAndRaise(VMINYProperty, ref _VMINY, value);
                SetAndRaise(VMINProperty, ref _VMIN, new Vector3(VMINX, value, VMINZ));
            }
        }

        private float _VMINZ;
        public static readonly DirectProperty<Vector3Control, float> VMINZProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VMINZ", o => o.VMINZ, (o, v) => o.VMINZ = v);
        public float VMINZ
        {
            get => _VMINZ;
            set
            {
                SetAndRaise(VMINZProperty, ref _VMINZ, value);
                SetAndRaise(VMINProperty, ref _VMIN, new Vector3(VMINX, VMINY, value));
            }
        }

        #endregion

        #region VMAX

        private Vector3 _VMAX;
        public static readonly DirectProperty<Vector3Control, Vector3> VMAXProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, Vector3>("VMAX", o => o.VMAX, (o, v) => o.VMAX = v);
        public Vector3 VMAX
        {
            get => _VMAX;
            set
            {
                SetAndRaise(VMAXProperty, ref _VMAX, value);
                SetAndRaise(VMAXXProperty, ref _VMAXX, value.X);
                SetAndRaise(VMAXYProperty, ref _VMAXY, value.Y);
                SetAndRaise(VMAXZProperty, ref _VMAXZ, value.Z);
                UpdateTip();
            }
        }

        private float _VMAXX;
        public static readonly DirectProperty<Vector3Control, float> VMAXXProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VMAXX", o => o.VMAXX, (o, v) => o.VMAXX = v);
        public float VMAXX
        {
            get => _VMAXX;
            set
            {
                SetAndRaise(VMAXXProperty, ref _VMAXX, value);
                SetAndRaise(VMAXProperty, ref _VMAX, new Vector3(value, VMAXY, VMAXZ));
            }
        }

        private float _VMAXY;
        public static readonly DirectProperty<Vector3Control, float> VMAXYProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VMAXY", o => o.VMAXY, (o, v) => o.VMAXY = v);
        public float VMAXY
        {
            get => _VMAXY;
            set
            {
                SetAndRaise(VMAXYProperty, ref _VMAXY, value);
                SetAndRaise(VMAXProperty, ref _VMAX, new Vector3(VMAXX, value, VMAXZ));
            }
        }

        private float _VMAXZ;
        public static readonly DirectProperty<Vector3Control, float> VMAXZProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VMAXZ", o => o.VMAXZ, (o, v) => o.VMAXZ = v);
        public float VMAXZ
        {
            get => _VMAXZ;
            set
            {
                SetAndRaise(VMAXZProperty, ref _VMAXZ, value);
                SetAndRaise(VMAXProperty, ref _VMAX, new Vector3(VMAXX, VMAXY, value));
            }
        }

        #endregion

        #region V Tip

        private string _VXTip;
        public static readonly DirectProperty<Vector3Control, string> VXTipProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, string>("VXTip", o => o.VXTip, (o, v) => o.VXTip = v);
        public string VXTip
        {
            get => _VXTip;
            set
            {
                SetAndRaise(VXTipProperty, ref _VXTip, value);
            }
        }

        private string _VYTip;
        public static readonly DirectProperty<Vector3Control, string> VYTipProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, string>("VYTip", o => o.VYTip, (o, v) => o.VYTip = v);
        public string VYTip
        {
            get => _VYTip;
            set
            {
                SetAndRaise(VYTipProperty, ref _VYTip, value);
            }
        }

        private string _VZTip;
        public static readonly DirectProperty<Vector3Control, string> VZTipProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, string>("VZTip", o => o.VZTip, (o, v) => o.VZTip = v);
        public string VZTip
        {
            get => _VZTip;
            set
            {
                SetAndRaise(VZTipProperty, ref _VZTip, value);
            }
        }

        #endregion

        #region V     

        private Vector3 _V;
        public static readonly DirectProperty<Vector3Control, Vector3> VProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, Vector3>("V", o => o.V, (o, v) => o.V = v);
        public Vector3 V
        {
            get => _V;
            set
            {
                SetAndRaise(VProperty, ref _V, value);
                SetAndRaise(VXProperty, ref _VX, value.X);
                SetAndRaise(VYProperty, ref _VY, value.Y);
                SetAndRaise(VZProperty, ref _VZ, value.Z);
                UpdateTip();
            }
        }

        private float _VX;
        public static readonly DirectProperty<Vector3Control, float> VXProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VX", o => o.VX, (o, v) => o.VX = v);
        public float VX
        {
            get => _VX;
            set
            {
                SetAndRaise(VXProperty, ref _VX, value);
                SetAndRaise(VProperty, ref _V, new Vector3(value, VY, VZ));
            }
        }

        private float _VY;
        public static readonly DirectProperty<Vector3Control, float> VYProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VY", o => o.VY, (o, v) => o.VY = v);
        public float VY
        {
            get => _VY;
            set
            {
                SetAndRaise(VYProperty, ref _VY, value);
                SetAndRaise(VProperty, ref _V, new Vector3(VX, value, VZ));
            }
        }

        private float _VZ;
        public static readonly DirectProperty<Vector3Control, float> VZProperty =
            AvaloniaProperty.RegisterDirect<Vector3Control, float>("VZ", o => o.VZ, (o, v) => o.VZ = v);
        public float VZ
        {
            get => _VZ;
            set
            {
                SetAndRaise(VZProperty, ref _VZ, value);
                SetAndRaise(VProperty, ref _V, new Vector3(VX, VY, value));
            }
        }

        #endregion

        public Vector3Control()
        {
            InitializeComponent();

            V = new Vector3();
            VMIN = new Vector3(-1000, -1000, -1000);
            VMAX = new Vector3(1000, 1000, 1000);

            {
                var xTbox = this.FindControl<TextBox>("xTbox");
                var xSlider = this.FindControl<Grid>("xSlider");

                xTbox.PointerWheelChanged += xWheel;
                xSlider.PointerWheelChanged += xWheel;
            }

            {
                var yTbox = this.FindControl<TextBox>("yTbox");
                var ySlider = this.FindControl<Grid>("ySlider");

                yTbox.PointerWheelChanged += yWheel;
                ySlider.PointerWheelChanged += yWheel;
            }

            {
                var zTbox = this.FindControl<TextBox>("zTbox");
                var zSlider = this.FindControl<Grid>("zSlider");

                zTbox.PointerWheelChanged += zWheel;
                zSlider.PointerWheelChanged += zWheel;
            }

            DataContext = this;
        }

        void xWheel(object sender, PointerWheelEventArgs e)
        {
            var v = VX + (float)((VMAX.X - VMIN.X) / 20 * e.Delta.Y);
            if (v > VMAXX) VX = VMAXX;
            else if (v < VMINX) VX = VMINX;
            else VX = v;
        }

        void yWheel(object sender, PointerWheelEventArgs e)
        {
            var v = VY + (float)((VMAX.Y - VMIN.Y) / 20 * e.Delta.Y);
            if (v > VMAXY) VY = VMAXY;
            else if (v < VMINY) VY = VMINY;
            else VY = v;
        }

        void zWheel(object sender, PointerWheelEventArgs e)
        {
            var v = VZ + (float)((VMAX.Z - VMIN.Z) / 20 * e.Delta.Y);
            if (v > VMAXZ) VZ = VMAXZ;
            else if (v < VMINX) VZ = VMINZ;
            else VZ = v;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void UpdateTip()
        {
            VXTip = $"{VMIN.X} <= {V.X} <= {VMAX.X}";
            VYTip = $"{VMIN.Y} <= {V.Y} <= {VMAX.Y}";
            VZTip = $"{VMIN.Z} <= {V.Z} <= {VMAX.Z}";
        }

    }

}
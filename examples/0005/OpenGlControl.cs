using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.Threading;
using static Avalonia.OpenGL.GlConsts;
using static System.Math;
using System.Collections.Generic;
using QuantumConcepts.Formats.StereoLithography;
using System.IO;
using Avalonia.Data;
using System.ComponentModel;
using Avalonia.Controls;

namespace SearchAThing.SciExamples
{

    public class OpenGlPageControl : OpenGlControlBase, INotifyPropertyChanged
    {
        private float _lightPosX = 0.7f;
        public static readonly DirectProperty<OpenGlPageControl, float> LightPosXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("LightPosX", o => o.LightPosX, (o, v) => o.LightPosX = v);
        public float LightPosX
        {
            get => _lightPosX;
            set => SetAndRaise(LightPosXProperty, ref _lightPosX, value);
        }

        private float _lightPosY = 5000f;
        public static readonly DirectProperty<OpenGlPageControl, float> LightPosYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("LightPosY", o => o.LightPosY, (o, v) => o.LightPosY = v);
        public float LightPosY
        {
            get => _lightPosY;
            set => SetAndRaise(LightPosYProperty, ref _lightPosY, value);
        }

        private float _lightPosZ = 50f;
        public static readonly DirectProperty<OpenGlPageControl, float> LightPosZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("LightPosZ", o => o.LightPosZ, (o, v) => o.LightPosZ = v);
        public float LightPosZ
        {
            get => _lightPosZ;
            set => SetAndRaise(LightPosZProperty, ref _lightPosZ, value);
        }

        private float _amb = 0.2f;
        public static readonly DirectProperty<OpenGlPageControl, float> AmbientStrengthProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("AmbientStrength", o => o.AmbientStrength, (o, v) => o.AmbientStrength = v);
        public float AmbientStrength
        {
            get => _amb;
            set => SetAndRaise(AmbientStrengthProperty, ref _amb, value);
        }

        private float _yaw;
        public static readonly DirectProperty<OpenGlPageControl, float> YawProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Yaw", o => o.Yaw, (o, v) => o.Yaw = v);
        public float Yaw
        {
            get => _yaw;
            set => SetAndRaise(YawProperty, ref _yaw, value);
        }

        private float _pitch;
        public static readonly DirectProperty<OpenGlPageControl, float> PitchProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Pitch", o => o.Pitch, (o, v) => o.Pitch = v);
        public float Pitch
        {
            get => _pitch;
            set => SetAndRaise(PitchProperty, ref _pitch, value);
        }

        private float _roll;
        public static readonly DirectProperty<OpenGlPageControl, float> RollProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Roll", o => o.Roll, (o, v) => o.Roll = v);
        public float Roll
        {
            get => _roll;
            set => SetAndRaise(RollProperty, ref _roll, value);
        }

        private string _info;
        public static readonly DirectProperty<OpenGlPageControl, string> InfoProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, string>("Info", o => o.Info, (o, v) => o.Info = v);
        public string Info
        {
            get => _info;
            private set => SetAndRaise(InfoProperty, ref _info, value);
        }

        private string _info2;
        public static readonly DirectProperty<OpenGlPageControl, string> Info2Property =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, string>("Info2", o => o.Info2, (o, v) => o.Info2 = v);
        public string Info2
        {
            get => _info2;
            private set => SetAndRaise(Info2Property, ref _info2, value);
        }

        private bool _wireframe;
        public static readonly DirectProperty<OpenGlPageControl, bool> WireframeProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, bool>("Wireframe", o => o.Wireframe, (o, v) => o.Wireframe = v);
        public bool Wireframe
        {
            get => _wireframe;
            private set
            {
                SetAndRaise(WireframeProperty, ref _wireframe, value);
                InvalidateVisual();
            }
        }
        
        static OpenGlPageControl()
        {
            AffectsRender<OpenGlPageControl>(
                YawProperty, PitchProperty, RollProperty,
                LightPosXProperty, LightPosYProperty, LightPosZProperty,
                WireframeProperty,
                Info2Property, AmbientStrengthProperty);
        }

        private uint _vertexShader;
        private uint _fragmentShader;
        private uint _shaderProgram;
        private uint VBO;
        private uint IBO;
        private uint VAO;

        readonly int VERTEX_WITH_NORMAL_SIZE = Marshal.SizeOf<GLVertexWithNormal>();

        private string GetShader(bool fragment, string shader)
        {
            var version = (GlVersion.Type == GlProfileType.OpenGL ? RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 : 100);
            var data = "#version " + version + "\n";
            if (GlVersion.Type == GlProfileType.OpenGLES) data += "precision mediump float;\n";
            if (version >= 150)
            {
                shader = shader.Replace("attribute", "in");
                if (fragment)
                    shader = shader
                        .Replace("varying", "in")
                        .Replace("//DECLAREGLFRAG", "out vec4 outFragColor;")
                        .Replace("gl_FragColor", "outFragColor");
                else
                    shader = shader.Replace("varying", "out");
            }

            data += shader;

            return data;
        }

        private string VertexShaderSource => GetShader(false,
            "0005.vertexShader.glsl".GetEmbeddedFileContent<OpenGlPageControl>());

        private string FragmentShaderSource => GetShader(true,
            "0005.fragmentShader.glsl".GetEmbeddedFileContent<OpenGlPageControl>());

        private GLVertexWithNormal[] _points;
        private uint[] _indices;
        BBox3D bbox = new BBox3D();

        PointerPoint? mousePress = null;
        float startYaw;
        float startPitch;

        Vector3 cameraPos = new Vector3(0, 0, 2);
        Vector3 cameraTarget = new Vector3();
        Vector3 startCameraPos;
        Vector3 startCameraTarget;

        netDxf.DxfDocument dxf = null;

        //public const float PAN_FACTOR = 0.01f;

        public void Reset()
        {
            Yaw = 0;
            Pitch = 0;
            Roll = 0;
            cameraPos = new Vector3(0, 0, 2);
            cameraTarget = new Vector3();
            first_render = true;
            LightPosX = LightPosY = LightPosZ = 0;

            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        }

        bool _INITIALIZED = false;
        void CHECK_INIT()
        {
            if (_INITIALIZED) return;

            //var ctl = this.FindControl<Button>("btnReset");

            this.PointerPressed += (a, b) =>
            {
                var cp = b.GetCurrentPoint(this);
                mousePress = cp;
                //b.Pointer.Capture(this);
                System.Console.WriteLine($"pointer pressed evt:{cp.Position}");
                startYaw = Yaw;
                startPitch = Pitch;
                startCameraPos = cameraPos;
                startCameraTarget = cameraTarget;
            };

            this.PointerWheelChanged += (a, b) =>
            {
                cameraPos = cameraPos + (cameraTarget - cameraPos) / 2 * (b.Delta.Y > 0 ? 1 : -1);
                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
            };

            this.PointerMoved += (a, b) =>
            {
                var cp = b.GetCurrentPoint(this);

                if (mousePress != null)
                {
                    var curx = cp.Position.X;
                    var cury = cp.Position.Y;

                    var startx = mousePress.Position.X;
                    var starty = mousePress.Position.Y;

                    if (mousePress.Properties.IsLeftButtonPressed)
                    {
                        var deltaYaw = (float)((curx - startx) / Bounds.Width * PI);
                        Yaw = startYaw - deltaYaw;

                        var deltaPitch = (float)((cury - starty) / Bounds.Height * PI);
                        Pitch = startPitch + deltaPitch;
                    }
                    else if (mousePress.Properties.IsRightButtonPressed)
                    {
                        var cameraTargetInit = (bbox.Min + bbox.Max) / 2;
                        var cameraPosInit = cameraTargetInit + bbox.Size.Length * Vector3D.ZAxis;

                        var scaleFact = (1f - ((Vector3D)cameraTarget).Distance(cameraTargetInit));

                        var dx = (startx - curx) / Bounds.Width * bbox.Size.X;// * scaleFact;
                        var dy = (starty - cury) / Bounds.Height * bbox.Size.Y;// * scaleFact;

                        cameraPos.X = startCameraPos.X - (float)dx;
                        cameraPos.Y = startCameraPos.Y - (float)dy;

                        cameraTarget.X = startCameraTarget.X - (float)dx;
                        cameraTarget.Y = startCameraTarget.Y - (float)dy;

                        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
                    }
                }

                //System.Console.WriteLine($"pointer move evt:{cp.Position}");
                //System.Console.WriteLine($"yaw:{Yaw} pitch:{Pitch} roll:{Roll} cameraPos:{cameraPos} cameraTarget:{cameraTarget}");
            };

            this.PointerReleased += (a, b) =>
            {
                if (mousePress != null)
                {
                }
                mousePress = null;
            };

            {
                var TOL = 1e-3;

                (GLVertexWithNormal[] points, uint[] indices) GetVertexes(IList<Facet> facets)
                {
                    var vtxs = new List<GLVertexWithNormal>();
                    var idxs = new List<uint>();
                    //var cmp = new Vector3DEqualityComparer(TOL);
                    var dictIdx = new Dictionary<string, uint>();

                    /// <summary>
                    /// retrieve vertex number and provides to add to vtxs if not already present
                    /// </summary>
                    /// <param name="v">vector to add vtxs if not already present</param>
                    /// <returns>id of the vertex</returns>
                    uint GetVertIdx(Vector3D v)
                    {
                        uint res = 0;

                        var str = v.ToString(TOL);

                        if (!dictIdx.TryGetValue(str, out res))
                        {
                            res = (uint)dictIdx.Count;
                            dictIdx.Add(str, res);
                            vtxs.Add(v.ToGLTriangleVertex());
                            bbox.ApplyUnion(v);
                        }

                        return res;
                    }

                    foreach (var f in facets)
                    {
                        var i1 = GetVertIdx(f.Vertices[0]);
                        var i2 = GetVertIdx(f.Vertices[1]);
                        var i3 = GetVertIdx(f.Vertices[2]);

                        idxs.Add(i1);
                        idxs.Add(i2);
                        idxs.Add(i3);
                    }

                    return (points: vtxs.ToArray(), indices: idxs.ToArray());
                }

                var sw0 = new Stopwatch();
                sw0.Start();

                var STLmapPathfilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.stl");

                if (!File.Exists(STLmapPathfilename))
                {
                    System.Console.WriteLine($"can't find [{STLmapPathfilename}]");
                    Environment.Exit(1);
                }

                System.Console.WriteLine($"loading from file [{STLmapPathfilename}]");

                using (var stream = File.Open(STLmapPathfilename, FileMode.Open, FileAccess.Read))
                {
                    var stl = STLDocument.Read(stream);
                    System.Console.WriteLine($"took {sw0.Elapsed}");

                    System.Console.WriteLine($"computing indices");
                    sw0.Restart();

                    var q = GetVertexes(stl.Facets);

                    _points = q.points;
                    _indices = q.indices;
                }

                System.Console.WriteLine($"took {sw0.Elapsed.ToString()}");
                System.Console.WriteLine($"points:{_points.Length} indices:{_indices.Length}");

                System.Console.WriteLine($"computing normals");
                sw0.Restart();
                for (int i = 0; i < _indices.Length; i += 3)
                {
                    var a = _points[_indices[i]].Position;
                    var b = _points[_indices[i + 1]].Position;
                    var c = _points[_indices[i + 2]].Position;
                    var normal = Vector3.Normalize(Vector3.Cross(c - b, a - b));

                    _points[_indices[i]].Normal += normal;
                    _points[_indices[i + 1]].Normal += normal;
                    _points[_indices[i + 2]].Normal += normal;
                }

                for (int i = 0; i < _points.Length; i++)
                {
                    _points[i].Normal = Vector3.Normalize(_points[i].Normal);
                }

                System.Console.WriteLine($"took {sw0.Elapsed.ToString()}");
            }

            _INITIALIZED = true;

            System.Console.WriteLine($"Model Bounds: {bbox}");
        }

        public OpenGlPageControl()
        {
        }

        private void CheckError(GlInterface gl)
        {
            int err;
            while ((err = gl.GetError()) != GL_NO_ERROR)
                Console.WriteLine($"err:" + err);
        }

        public void glDebugFn(uint error, string command, int messageType, IntPtr threadLabel, IntPtr objectLabel, string message)
        {
            System.Console.WriteLine($"err:{error} cmd:{command} mex:{message}");
        }

        const int positionLocation = 0;
        const int normalLocation = 1;

        protected unsafe override void OnOpenGlInit(GlInterface GL, uint fb)
        {
            CheckError(GL);

            CHECK_INIT();

            // var flags = GL.GetIntegerv_one(GetPName.GL_CONTEXT_FLAGS);
            // if ((flags & GL_CONTEXT_FLAG_DEBUG_BIT) != 0)
            // {
            //     System.Console.WriteLine($"debug mode");
            // GL.DebugMessageCallback(glDebugFn, IntPtr.Zero);
            // GL.Enable(EnableCap.GL_DEBUG_OUTPUT);
            // }            

            CheckError(GL);

            Info = $"Renderer: {GL.GetString(StringName.GL_RENDERER)} Version: {GL.GetString(StringName.GL_VERSION)}";

            // Load the source of the vertex shader and compile it.
            _vertexShader = GL.CreateShader(ShaderType.GL_VERTEX_SHADER);
            Console.WriteLine(GL.CompileShaderAndGetError(_vertexShader, VertexShaderSource));
            CheckError(GL);

            // Load the source of the fragment shader and compile it.
            _fragmentShader = GL.CreateShader(ShaderType.GL_FRAGMENT_SHADER);
            Console.WriteLine(GL.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource));
            CheckError(GL);

            // Create the shader program, attach the vertex and fragment shaders and link the program.
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, _vertexShader);
            GL.AttachShader(_shaderProgram, _fragmentShader);

            GL.BindAttribLocationString(_shaderProgram, positionLocation, "aPos");
            GL.BindAttribLocationString(_shaderProgram, normalLocation, "aNormal");
            Console.WriteLine(GL.LinkProgramAndGetError(_shaderProgram));
            CheckError(GL);

            // Create the vertex buffer object (VBO) for the vertex data.
            VBO = GL.GenBuffer();
            // Bind the VBO and copy the vertex data into it.
            GL.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, VBO);
            CheckError(GL);
            var vertexSize = Marshal.SizeOf<GLVertexWithNormal>();
            fixed (void* pdata = _points)
                GL.BufferData(BufferTargetARB.GL_ARRAY_BUFFER, new IntPtr(_points.Length * vertexSize),
                    new IntPtr(pdata), BufferUsageARB.GL_STATIC_DRAW);
            CheckError(GL);

            IBO = GL.GenBuffer();
            GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, IBO);
            CheckError(GL);
            fixed (void* pdata = _indices)
                GL.BufferData(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, new IntPtr(_indices.Length * sizeof(uint)),
                    new IntPtr(pdata), BufferUsageARB.GL_STATIC_DRAW);
            CheckError(GL);

            var oneArr = new uint[1];
            GL.GenVertexArrays(1, oneArr);
            VAO = oneArr[0];
            GL.BindVertexArray(VAO);
            CheckError(GL);
        }

        protected override void OnOpenGlDeinit(GlInterface GL, uint fb)
        {
            // Unbind everything
            GL.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, 0);
            GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            CheckError(GL);

            // Delete all resources.
            GL.DeleteBuffers(2, new[] { VBO, IBO });
            GL.DeleteVertexArrays(1, new[] { VAO });
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteShader(_fragmentShader);
            GL.DeleteShader(_vertexShader);
            CheckError(GL);
        }

        static Stopwatch St = Stopwatch.StartNew();

        bool first_render = true;

        protected override unsafe void OnOpenGlRender(GlInterface GL, uint fb)
        {
            if (first_render)
            {
                var cameraTargetInit = (bbox.Min + bbox.Max) / 2;
                var cameraPosInit = cameraTargetInit + bbox.Size.Length * Vector3D.ZAxis;
                cameraPos = cameraPosInit;
                cameraTarget = cameraTargetInit;
            }

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            GL.Enable(EnableCap.GL_DEPTH_TEST);
            GL.DepthMask(true);
            GL.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);

            CheckError(GL);

            GL.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, VBO);
            GL.BindVertexArray(VAO);

            GL.UseProgram(_shaderProgram);
            CheckError(GL);

            var aspectRatio = (float)(Bounds.Width / Bounds.Height);
            var nearPlaneDistance = 0.01f;
            var farPlaneDistance = 1000f;

            var projection =
                Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), aspectRatio, nearPlaneDistance, farPlaneDistance);

            var view =
                //Matrix4x4.CreateTranslation(-(float)bbox.Size.X / 2, -(float)bbox.Size.Y / 2, -(float)bbox.Size.Z / 2)
                //*
                Matrix4x4.CreateLookAt(cameraPos, cameraTarget, new Vector3(0, -1, 0));

            var model =
                //Matrix4x4.CreateTranslation(-(float)bbox.Size.X / 2, -(float)bbox.Size.Y / 2, -(float)bbox.Size.Z / 2)
                //*
                Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll);

            var modelTr =
                Matrix4x4.CreateTranslation(-(float)bbox.Size.X / 2, -(float)bbox.Size.Y / 2, -(float)bbox.Size.Z / 2);

            var modelLoc = GL.GetUniformLocationString(_shaderProgram, "uModel");
            var bboxLoc = GL.GetUniformLocationString(_shaderProgram, "uBBox");
            var viewLoc = GL.GetUniformLocationString(_shaderProgram, "uView");
            var projectionLoc = GL.GetUniformLocationString(_shaderProgram, "uProjection");
            var lightPosLoc = GL.GetUniformLocationString(_shaderProgram, "LightPos");
            var ambStrengthLoc = GL.GetUniformLocationString(_shaderProgram, "Amb");

            var bboxSize = bbox.Size;
            GL.UniformMatrix4fv(modelLoc, 1, false, &model);
            GL.UniformMatrix4fv(viewLoc, 1, false, &view);
            GL.UniformMatrix4fv(projectionLoc, 1, false, &projection);

            GL.Uniform3f(bboxLoc, (float)bboxSize.X, (float)bboxSize.Y, (float)bboxSize.Z);
            GL.Uniform3f(lightPosLoc, LightPosX, LightPosY, LightPosZ);
            GL.Uniform1f(ambStrengthLoc, AmbientStrength);
            CheckError(GL);

            if (Wireframe)
                GL.PolygonMode(MaterialFace.GL_FRONT_AND_BACK, PolygonMode.GL_LINE);
            else
                GL.PolygonMode(MaterialFace.GL_FRONT_AND_BACK, PolygonMode.GL_FILL);

            CheckError(GL);

            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.GL_FLOAT, false, VERTEX_WITH_NORMAL_SIZE, IntPtr.Zero);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.GL_FLOAT, false, VERTEX_WITH_NORMAL_SIZE, new IntPtr(sizeof(Vector3)));
            CheckError(GL);

            GL.EnableVertexAttribArray(positionLocation);
            GL.EnableVertexAttribArray(normalLocation);
            CheckError(GL);

            GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, IBO);
            GL.DrawElements(PrimitiveType.GL_TRIANGLES, _indices.Length, DrawElementsType.GL_UNSIGNED_INT, IntPtr.Zero);

            //GL.DrawArrays(GL_LINE_STRIP, 0, new IntPtr(_points.Length));

            CheckError(GL);
            // if (_disco > 0.01)
            //     Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);

            Dispatcher.UIThread.Post(() =>
            {
                Info2 =
                    $"DSP:{Bounds}\r\n" +
                    $"BBOX:{bbox}\r\n" +
                    $"CT:{cameraTarget}\r\n" +
                    $"CP:{cameraPos}\r\n" +
                    $"LP:{LightPosX},{LightPosY},{LightPosZ}";
            });

            first_render = false;
        }

        int cnt = 0;
    }
}

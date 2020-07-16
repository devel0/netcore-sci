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
using QuantumConcepts.Formats.StereoLithography;
using System.IO;
using System.ComponentModel;

namespace SearchAThing.SciExamples.Controls
{

    public class OpenGlControl : OpenGlControlBase, INotifyPropertyChanged
    {

        public const double TOL = 1e-3;
        static readonly Vector3D defaultLightPos = new Vector3D(0.7, 5000, 50);

        readonly int vertexSize = Marshal.SizeOf<GLVertexWithNormal>();
        readonly int idxSize = Marshal.SizeOf<uint>();

        #region PROPERTIES                

        private float _lightPosX = (float)defaultLightPos.X;
        public static readonly DirectProperty<OpenGlControl, float> LightPosXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("LightPosX", o => o.LightPosX, (o, v) => o.LightPosX = v);
        public float LightPosX
        {
            get => _lightPosX;
            set => SetAndRaise(LightPosXProperty, ref _lightPosX, value);
        }

        private float _lightPosY = (float)defaultLightPos.Y;
        public static readonly DirectProperty<OpenGlControl, float> LightPosYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("LightPosY", o => o.LightPosY, (o, v) => o.LightPosY = v);
        public float LightPosY
        {
            get => _lightPosY;
            set => SetAndRaise(LightPosYProperty, ref _lightPosY, value);
        }

        private float _lightPosZ = (float)defaultLightPos.Z;
        public static readonly DirectProperty<OpenGlControl, float> LightPosZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("LightPosZ", o => o.LightPosZ, (o, v) => o.LightPosZ = v);
        public float LightPosZ
        {
            get => _lightPosZ;
            set => SetAndRaise(LightPosZProperty, ref _lightPosZ, value);
        }

        private float _amb = 0.2f;
        public static readonly DirectProperty<OpenGlControl, float> AmbientStrengthProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("AmbientStrength", o => o.AmbientStrength, (o, v) => o.AmbientStrength = v);
        public float AmbientStrength
        {
            get => _amb;
            set => SetAndRaise(AmbientStrengthProperty, ref _amb, value);
        }

        private float _yaw;
        public static readonly DirectProperty<OpenGlControl, float> YawProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Yaw", o => o.Yaw, (o, v) => o.Yaw = v);
        public float Yaw
        {
            get => _yaw;
            set => SetAndRaise(YawProperty, ref _yaw, value);
        }

        private float _pitch;
        public static readonly DirectProperty<OpenGlControl, float> PitchProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Pitch", o => o.Pitch, (o, v) => o.Pitch = v);
        public float Pitch
        {
            get => _pitch;
            set => SetAndRaise(PitchProperty, ref _pitch, value);
        }

        private float _roll;
        public static readonly DirectProperty<OpenGlControl, float> RollProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Roll", o => o.Roll, (o, v) => o.Roll = v);
        public float Roll
        {
            get => _roll;
            set => SetAndRaise(RollProperty, ref _roll, value);
        }

        private string _info;
        public static readonly DirectProperty<OpenGlControl, string> InfoProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, string>("Info", o => o.Info, (o, v) => o.Info = v);
        public string Info
        {
            get => _info;
            private set => SetAndRaise(InfoProperty, ref _info, value);
        }

        private string _info2;
        public static readonly DirectProperty<OpenGlControl, string> Info2Property =
            AvaloniaProperty.RegisterDirect<OpenGlControl, string>("Info2", o => o.Info2, (o, v) => o.Info2 = v);
        public string Info2
        {
            get => _info2;
            private set => SetAndRaise(Info2Property, ref _info2, value);
        }

        private bool _wireframe;
        public static readonly DirectProperty<OpenGlControl, bool> WireframeProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, bool>("Wireframe", o => o.Wireframe, (o, v) => o.Wireframe = v);
        public bool Wireframe
        {
            get => _wireframe;
            private set
            {
                SetAndRaise(WireframeProperty, ref _wireframe, value);
                InvalidateVisual();
            }
        }

        private bool _showModel = true;
        public static readonly DirectProperty<OpenGlControl, bool> ShowModelProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, bool>("ShowModel", o => o.ShowModel, (o, v) => o.ShowModel = v);
        public bool ShowModel
        {
            get => _showModel;
            private set
            {
                SetAndRaise(ShowModelProperty, ref _showModel, value);
                InvalidateVisual();
            }
        }

        #endregion

        static OpenGlControl()
        {
            AffectsRender<OpenGlControl>(
                YawProperty, PitchProperty, RollProperty,
                LightPosXProperty, LightPosYProperty, LightPosZProperty,
                WireframeProperty, ShowModelProperty,
                Info2Property, AmbientStrengthProperty);
        }

        private uint _vertexShader;
        private uint _fragmentShader;
        private uint _shaderProgram;
        private uint VBO;
        private uint VAO;
        private GLVertexWithNormal[] _points;

        // VERTEX MANAGER
        GLVertexManager vtxMgr = new GLVertexManager(TOL);
        BBox3D bbox => vtxMgr.BBox;

        public const string MESH_NAME = "mesh";
        uint[] meshIdxs;
        private uint IBO_mesh;

        public const string WCS_NAME = "wcs";
        uint[] wcsIdxs;
        private uint IBO_wcs;

        PointerPoint? mousePress = null;
        float startYaw;
        float startPitch;

        Vector3 cameraPos = new Vector3(0, 0, 2);
        Vector3 cameraTarget = new Vector3();
        Vector3 startCameraPos;
        Vector3 startCameraTarget;

        bool firstRender = true;

        public void Reset()
        {
            Yaw = 0;
            Pitch = 0;
            Roll = 0;
            cameraPos = new Vector3();
            cameraTarget = new Vector3();
            firstRender = true;

            LightPosX = (float)defaultLightPos.X;
            LightPosY = (float)defaultLightPos.Y;
            LightPosZ = (float)defaultLightPos.Z;

            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        }

        bool _INITIALIZED = false;
        /// <summary>
        /// CHECK INIT
        /// </summary>
        void CHECK_INIT()
        {
            if (_INITIALIZED) return;            

            this.PointerPressed += (a, b) =>
            {
                var cp = b.GetCurrentPoint(this);
                mousePress = cp;
                //b.Pointer.Capture(this);
                //System.Console.WriteLine($"pointer pressed evt:{cp.Position}");
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

            var swTotal = new Stopwatch();
            swTotal.Start();

            var sw0 = new Stopwatch();
            {
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

                    vtxMgr.AddFaces(MESH_NAME, stl.Facets);
                }

                System.Console.WriteLine($"took {sw0.Elapsed.ToString()}");
                System.Console.WriteLine($"points:{vtxMgr.Vtxs.Count} indices:{vtxMgr.Idxs.Count}");
            }

            vtxMgr.AddFigures(WCS_NAME,
                new[] { new Vector3D(), Vector3D.XAxis * 10 },
                new[] { new Vector3D(), Vector3D.YAxis * 10 },
                new[] { new Vector3D(), Vector3D.ZAxis * 10 });

            _INITIALIZED = true;

            System.Console.WriteLine($"computing normals");
            sw0.Restart();
            _points = vtxMgr.BuildPoints();
            System.Console.WriteLine($"took {sw0.Elapsed.ToString()}");

            System.Console.WriteLine($"MODEL LOAD TOTAL TIME: {swTotal.Elapsed}");

            System.Console.WriteLine($"Model Bounds: {vtxMgr.BBox}");
        }

        public OpenGlControl()
        {
            CHECK_INIT();
        }

        /// <summary>
        /// GL INIT
        /// </summary>
        protected unsafe override void OnOpenGlInit(GlInterface GL, uint fb)
        {
            CheckError(GL);

            CHECK_INIT();

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
            const int positionLocation = 0;
            const int normalLocation = 1;
            GL.BindAttribLocationString((uint)_shaderProgram, positionLocation, "aPos");
            GL.BindAttribLocationString(_shaderProgram, normalLocation, "aNormal");
            Console.WriteLine(GL.LinkProgramAndGetError(_shaderProgram));
            CheckError(GL);

            // create and bind vertex buffer
            {
                VBO = GL.GenBuffer();
                // Bind the VBO and copy the vertex data into it.
                GL.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, VBO);
                CheckError(GL);
                fixed (void* pdata = _points)
                    GL.BufferData(BufferTargetARB.GL_ARRAY_BUFFER, new IntPtr(_points.Length * vertexSize),
                        new IntPtr(pdata), BufferUsageARB.GL_STATIC_DRAW);
                CheckError(GL);
            }

            // create and bind index buffer
            {
                IBO_mesh = GL.GenBuffer();
                GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, IBO_mesh);
                CheckError(GL);
                meshIdxs = vtxMgr.BuildIdxs(MESH_NAME);
                fixed (void* pdata = meshIdxs)
                    GL.BufferData(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, new IntPtr(meshIdxs.Length * sizeof(uint)),
                        new IntPtr(pdata), BufferUsageARB.GL_STATIC_DRAW);
                CheckError(GL);
            }

            // create and bind index buffer
            {
                IBO_wcs = GL.GenBuffer();
                GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, IBO_wcs);
                CheckError(GL);
                wcsIdxs = vtxMgr.BuildIdxs(WCS_NAME);
                fixed (void* pdata = wcsIdxs)
                    GL.BufferData(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, new IntPtr(wcsIdxs.Length * sizeof(uint)),
                        new IntPtr(pdata), BufferUsageARB.GL_STATIC_DRAW);
                CheckError(GL);
            }

            {
                var rv = new uint[1];
                GL.GenVertexArrays(1, rv);
                VAO = rv[0];
            }
            GL.BindVertexArray(VAO);
            CheckError(GL);

            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.GL_FLOAT, false, vertexSize, IntPtr.Zero);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.GL_FLOAT, false, vertexSize, new IntPtr(sizeof(Vector3)));
            CheckError(GL);

            GL.EnableVertexAttribArray(positionLocation);
            GL.EnableVertexAttribArray(normalLocation);
            CheckError(GL);
        }

        /// <summary>
        /// GL DEINIT
        /// </summary>
        protected override void OnOpenGlDeinit(GlInterface GL, uint fb)
        {
            // Unbind everything
            GL.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, 0);
            GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            CheckError(GL);

            // Delete all resources.
            GL.DeleteBuffers(2, new[] { VBO, IBO_mesh, IBO_wcs });
            GL.DeleteVertexArrays(1, new[] { VAO });
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteShader(_fragmentShader);
            GL.DeleteShader(_vertexShader);
            CheckError(GL);
        }

        /// <summary>
        /// GL RENDER
        /// </summary>
        protected override unsafe void OnOpenGlRender(GlInterface gl, uint fb)
        {
            if (firstRender)
            {
                var modelTr = new Vector3D();

                var cameraTargetInit = bbox.Middle - bbox.Size / 2;
                var cameraPosInit = cameraTargetInit + bbox.Size.Length * Vector3D.ZAxis;

                cameraPos = cameraPosInit;
                cameraTarget = cameraTargetInit;
            }

            gl.ClearColor(0, 0, 0, 0);
            gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            gl.Enable(EnableCap.GL_DEPTH_TEST);
            gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            var GL = gl;
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

            var view = Matrix4x4.CreateLookAt(cameraPos, cameraTarget, new Vector3(0, -1, 0));

            var model = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll);

            var modelLoc = GL.GetUniformLocationString(_shaderProgram, "uModel");
            var bboxLoc = GL.GetUniformLocationString(_shaderProgram, "uBBox");
            var viewLoc = GL.GetUniformLocationString(_shaderProgram, "uView");
            var projectionLoc = GL.GetUniformLocationString(_shaderProgram, "uProjection");
            var lightPosLoc = GL.GetUniformLocationString(_shaderProgram, "LightPos");
            var objColLoc = GL.GetUniformLocationString(_shaderProgram, "ObjCol");
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

            //
            // DRAW MESH
            //
            if (ShowModel)
            {
                GL.LineWidth(1);
                GL.Uniform3f(objColLoc, 0.305f, 0.485f, 0.668f);
                GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, IBO_mesh);
                GL.DrawElements(PrimitiveType.GL_TRIANGLES, meshIdxs.Length, DrawElementsType.GL_UNSIGNED_INT, IntPtr.Zero);
            }

            //
            // DRAW WCS
            //
            GL.LineWidth(10);
            GL.Uniform1f(ambStrengthLoc, 1f);
            {
                GL.Uniform3f(objColLoc, 1f, 0, 0);
                GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, IBO_wcs);
                GL.DrawElements(PrimitiveType.GL_LINES, 2, DrawElementsType.GL_UNSIGNED_INT, IntPtr.Zero);
            }
            {
                GL.Uniform3f(objColLoc, 0, 1f, 0);
                GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, IBO_wcs);
                GL.DrawElements(PrimitiveType.GL_LINES, 2, DrawElementsType.GL_UNSIGNED_INT, new IntPtr(2 * idxSize));
            }
            {
                GL.Uniform3f(objColLoc, 0, 0, 1f);
                GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, IBO_wcs);
                GL.DrawElements(PrimitiveType.GL_LINES, 2, DrawElementsType.GL_UNSIGNED_INT, new IntPtr(4 * idxSize));
            }

            CheckError(GL);

            Dispatcher.UIThread.Post(() =>
            {
                Info2 =
                    $"DSP:{Bounds}\r\n" +
                    $"BBOX:{bbox}\r\n" +
                    $"CT:{cameraTarget}\r\n" +
                    $"CP:{cameraPos}\r\n" +
                    $"LP:{LightPosX},{LightPosY},{LightPosZ}";
            });

            firstRender = false;
        }

        #region misc utils

        private void CheckError(GlInterface gl)
        {
            int err;
            while ((err = gl.GetError()) != GL_NO_ERROR)
                Console.WriteLine($"err:" + err);
        }

        #endregion

        #region shader utils

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
            "0006.shaders.vertexShader.glsl".GetEmbeddedFileContent<OpenGlControl>());

        private string FragmentShaderSource => GetShader(true,
            "0006.shaders.fragmentShader.glsl".GetEmbeddedFileContent<OpenGlControl>());

        #endregion

    }

}

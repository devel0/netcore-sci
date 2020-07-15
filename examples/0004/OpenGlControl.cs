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
using static SearchAThing.SciToolkit;

namespace SearchAThing.SciExamples
{

    public class OpenGlPageControl : OpenGlControlBase
    {

        private float _yaw;

        public static readonly DirectProperty<OpenGlPageControl, float> YawProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Yaw", o => o.Yaw, (o, v) => o.Yaw = v);

        public float Yaw
        {
            get => _yaw;
            set
            {
                SetAndRaise(YawProperty, ref _yaw, value);
                System.Console.WriteLine($"yaw:{value}");
            }
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


        private float _disco;

        public static readonly DirectProperty<OpenGlPageControl, float> DiscoProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Disco", o => o.Disco, (o, v) => o.Disco = v);

        public float Disco
        {
            get => _disco;
            set => SetAndRaise(DiscoProperty, ref _disco, value);
        }

        private string _info;

        public static readonly DirectProperty<OpenGlPageControl, string> InfoProperty =
            AvaloniaProperty.RegisterDirect<OpenGlPageControl, string>("Info", o => o.Info, (o, v) => o.Info = v);

        public string Info
        {
            get => _info;
            private set => SetAndRaise(InfoProperty, ref _info, value);
        }

        static OpenGlPageControl()
        {
            AffectsRender<OpenGlPageControl>(YawProperty, PitchProperty, RollProperty, DiscoProperty);
        }

        private uint _vertexShader;
        private uint _fragmentShader;
        private uint _shaderProgram;
        private uint _vertexBufferObject;
        private uint _indexBufferObject;
        private uint _vertexArrayObject;

        private string GetShader(bool fragment, string shader)
        {
            var version = (GlVersion.Type == GlProfileType.OpenGL ?
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 :
                100);
            var data = "#version " + version + "\n";
            if (GlVersion.Type == GlProfileType.OpenGLES)
                data += "precision mediump float;\n";
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
            "0004.vertexShader.glsl".GetEmbeddedFileContent<OpenGlPageControl>());

        private string FragmentShaderSource => GetShader(true,
            "0004.fragmentShader.glsl".GetEmbeddedFileContent<OpenGlPageControl>());

        private readonly GLVertexWithNormal[] _points;
        private readonly uint[] _indices;
        private readonly float _minY;
        private readonly float _maxY;

        PointerPoint? mousePress = null;
        float startYaw;
        float startPitch;

        Vector3 cameraPos = new Vector3(0, 0, 2);
        Vector3 cameraTarget = new Vector3();
        Vector3 startCameraPos;
        Vector3 startCameraTarget;

        netDxf.DxfDocument dxf = null;

        public const float PAN_FACTOR = 0.01f;

        public void Reset()
        {
            Yaw = 0;
            Pitch = 0;
            Roll = 0;
            cameraPos = new Vector3(0, 0, 2);
            cameraTarget = new Vector3();

            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        }

        public OpenGlPageControl()
        {
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
                        var dx = startx - curx;
                        var dy = starty - cury;

                        cameraPos.X = startCameraPos.X - (float)dx * PAN_FACTOR;
                        cameraPos.Y = startCameraPos.Y - (float)dy * PAN_FACTOR;

                        cameraTarget.X = startCameraTarget.X - (float)dx * PAN_FACTOR;
                        cameraTarget.Y = startCameraTarget.Y - (float)dy * PAN_FACTOR;

                        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
                    }
                }

                //System.Console.WriteLine($"pointer move evt:{cp.Position}");
                System.Console.WriteLine($"yaw:{Yaw} pitch:{Pitch} roll:{Roll} cameraPos:{cameraPos} cameraTarget:{cameraTarget}");
            };

            this.PointerReleased += (a, b) =>
            {
                if (mousePress != null)
                {
                }
                mousePress = null;
            };

            {

                // if (dxf == null)
                // {
                //     dxf = new netDxf.DxfDocument();

                //     var cube = DxfKit.Cube(new Vector3D(), 1);

                //     dxf.AddEntity(cube);

                //     //dxf.Save("out.dxf", true);
                // }

                var TOL = 1e-3;

                var sw0 = new Stopwatch();
                sw0.Start();

                (GLVertexWithNormal[] points, uint[] indices) GetVertexes()
                {
                    var vtxs = new List<GLVertexWithNormal>();
                    var idxs = new List<uint>();
                    var cmp = new Vector3DEqualityComparer(TOL);
                    var dictIdx = new Dictionary<Vector3D, uint>(cmp);

                    /// <summary>
                    /// retrieve vertex number and provides to add to vtxs if not already present
                    /// </summary>
                    /// <param name="v">vector to add vtxs if not already present</param>
                    /// <returns>id of the vertex</returns>
                    uint GetVertIdx(Vector3D v)
                    {
                        uint res = 0;

                        if (!dictIdx.TryGetValue(v, out res))
                        {
                            res = (uint)dictIdx.Count;
                            dictIdx.Add(v, res);
                            vtxs.Add(v.ToGLTriangleVertex());
                        }

                        return res;
                    }

                    var boxFaces = Cube(new Vector3D(), 1);

                    int xCnt = 50;
                    int yCnt = 50;

                    foreach (var face in boxFaces)
                    {
                        for (int xi = 0; xi < xCnt; ++xi)
                        {
                            for (int yi = 0; yi < yCnt; ++yi)
                            {
                                var delta = new netDxf.Vector3(2 * xi, 2 * yi, 0);

                                var i1 = GetVertIdx(face.FirstVertex + delta);
                                var i2 = GetVertIdx(face.SecondVertex + delta);
                                var i3 = GetVertIdx(face.ThirdVertex + delta);

                                if (face.FourthVertex != null)
                                {
                                    var i4 = GetVertIdx(face.FourthVertex + delta);
                                    idxs.Add(i1);
                                    idxs.Add(i2);
                                    idxs.Add(i3);

                                    idxs.Add(i3);
                                    idxs.Add(i4);
                                    idxs.Add(i1);
                                }
                                else
                                {
                                    idxs.Add(i1);
                                    idxs.Add(i2);
                                    idxs.Add(i3);
                                }
                            }
                        }
                    }

                    return (points: vtxs.ToArray(), indices: idxs.ToArray());
                }

                var q = GetVertexes();

                System.Console.WriteLine($"vertex:{q.points.Length} indices:{q.indices.Length} generted in {sw0.Elapsed}");

                _points = q.points;
                _indices = q.indices;

                for (int i = 0; i < _indices.Length; i += 3)
                {
                    Vector3 a = _points[_indices[i]].Position;
                    Vector3 b = _points[_indices[i + 1]].Position;
                    Vector3 c = _points[_indices[i + 2]].Position;
                    var normal = Vector3.Normalize(Vector3.Cross(c - b, a - b));

                    _points[_indices[i]].Normal += normal;
                    _points[_indices[i + 1]].Normal += normal;
                    _points[_indices[i + 2]].Normal += normal;
                }

                for (int i = 0; i < _points.Length; i++)
                {
                    _points[i].Normal = Vector3.Normalize(_points[i].Normal);
                    _maxY = Math.Max(_maxY, _points[i].Position.Y);
                    _minY = Math.Min(_minY, _points[i].Position.Y);
                }

            }

        }

        private void CheckError(GlInterface gl)
        {
            int err;
            while ((err = gl.GetError()) != GL_NO_ERROR)
                Console.WriteLine(err);
        }

        protected unsafe override void OnOpenGlInit(GlInterface GL, uint fb)
        {
            CheckError(GL);

            Info = $"Renderer: {GL.GetString(StringName.GL_RENDERER)} Version: {GL.GetString(StringName.GL_VERSION)}";

            // Load the source of the vertex shader and compile it.
            _vertexShader = GL.CreateShader(ShaderType.GL_VERTEX_SHADER);
            Console.WriteLine(GL.CompileShaderAndGetError(_vertexShader, VertexShaderSource));

            // Load the source of the fragment shader and compile it.
            _fragmentShader = GL.CreateShader(ShaderType.GL_FRAGMENT_SHADER);
            Console.WriteLine(GL.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource));

            // Create the shader program, attach the vertex and fragment shaders and link the program.
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, _vertexShader);
            GL.AttachShader(_shaderProgram, _fragmentShader);
            const int positionLocation = 0;
            const int normalLocation = 1;
            GL.BindAttribLocationString(_shaderProgram, positionLocation, "aPos");
            GL.BindAttribLocationString(_shaderProgram, normalLocation, "aNormal");
            Console.WriteLine(GL.LinkProgramAndGetError(_shaderProgram));
            CheckError(GL);

            // Create the vertex buffer object (VBO) for the vertex data.
            _vertexBufferObject = GL.GenBuffer();
            // Bind the VBO and copy the vertex data into it.
            GL.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, _vertexBufferObject);
            CheckError(GL);
            var vertexSize = Marshal.SizeOf<GLVertexWithNormal>();
            fixed (void* pdata = _points)
                GL.BufferData(BufferTargetARB.GL_ARRAY_BUFFER, new IntPtr(_points.Length * vertexSize),
                    new IntPtr(pdata), BufferUsageARB.GL_STATIC_DRAW);

            _indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, _indexBufferObject);
            CheckError(GL);
            fixed (void* pdata = _indices)
                GL.BufferData(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, new IntPtr(_indices.Length * sizeof(uint)),
                    new IntPtr(pdata), BufferUsageARB.GL_STATIC_DRAW);
            CheckError(GL);

            var oneArr = new uint[1];
            GL.GenVertexArrays(1, oneArr);
            _vertexArrayObject = oneArr[0];
            GL.BindVertexArray(_vertexArrayObject);
            CheckError(GL);

            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.GL_FLOAT, false, vertexSize, IntPtr.Zero);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.GL_FLOAT, false, vertexSize, new IntPtr(sizeof(Vector3)));

            GL.EnableVertexAttribArray(positionLocation);
            GL.EnableVertexAttribArray(normalLocation);
            CheckError(GL);
        }

        protected override void OnOpenGlDeinit(GlInterface GL, uint fb)
        {
            // Unbind everything
            GL.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, 0);
            GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all resources.
            GL.DeleteBuffers(2, new[] { _vertexBufferObject, _indexBufferObject });
            GL.DeleteVertexArrays(1, new[] { _vertexArrayObject });
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteShader(_fragmentShader);
            GL.DeleteShader(_vertexShader);
        }

        static Stopwatch St = Stopwatch.StartNew();

        protected override unsafe void OnOpenGlRender(GlInterface gl, uint fb)
        {
            gl.ClearColor(0, 0, 0, 0);
            gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            gl.Enable(EnableCap.GL_DEPTH_TEST);
            gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            var GL = gl;

            GL.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, _vertexBufferObject);
            GL.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, _indexBufferObject);
            GL.BindVertexArray(_vertexArrayObject);
            GL.UseProgram(_shaderProgram);
            CheckError(GL);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height), 0.01f, 1000);

            //System.Console.WriteLine($"camerapos:{cameraPos}");
            var view = Matrix4x4.CreateLookAt(cameraPos, cameraTarget, new Vector3(0, -1, 0));
            var model = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll);
            var modelLoc = GL.GetUniformLocationString(_shaderProgram, "uModel");
            var viewLoc = GL.GetUniformLocationString(_shaderProgram, "uView");
            var projectionLoc = GL.GetUniformLocationString(_shaderProgram, "uProjection");
            var maxYLoc = GL.GetUniformLocationString(_shaderProgram, "uMaxY");
            var minYLoc = GL.GetUniformLocationString(_shaderProgram, "uMinY");
            var timeLoc = GL.GetUniformLocationString(_shaderProgram, "uTime");
            var discoLoc = GL.GetUniformLocationString(_shaderProgram, "uDisco");

            /*
            
            EQUIVALENT FORMS:
            -----------------------------------------------------------------------

            // (1) - float[] with direct array
            var vals = new[]
            {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            };
            // if want to use direct array struct fields are taken in byname sort
            // check with ObjectLayoutInspector utility ( https://github.com/SergeyTeplyakov/ObjectLayoutInspector )
            //
            // PrintLayout<Matrix4x4>();
            //            
            Type layout for 'Matrix4x4'
            Size: 64 bytes. Paddings: 0 bytes (%0 of empty space)
            |=============================|
            |   0-3: Single M11 (4 bytes) |
            |-----------------------------|
            |   4-7: Single M12 (4 bytes) |
            |-----------------------------|
            |  8-11: Single M13 (4 bytes) |
            |-----------------------------|
            | 12-15: Single M14 (4 bytes) |
            |-----------------------------|
            | 16-19: Single M21 (4 bytes) |
            |-----------------------------|
            | 20-23: Single M22 (4 bytes) |
            |-----------------------------|
            | 24-27: Single M23 (4 bytes) |
            |-----------------------------|
            | 28-31: Single M24 (4 bytes) |
            |-----------------------------|
            | 32-35: Single M31 (4 bytes) |
            |-----------------------------|
            | 36-39: Single M32 (4 bytes) |
            |-----------------------------|
            | 40-43: Single M33 (4 bytes) |
            |-----------------------------|
            | 44-47: Single M34 (4 bytes) |
            |-----------------------------|
            | 48-51: Single M41 (4 bytes) |
            |-----------------------------|
            | 52-55: Single M42 (4 bytes) |
            |-----------------------------|
            | 56-59: Single M43 (4 bytes) |
            |-----------------------------|
            | 60-63: Single M44 (4 bytes) |
            |=============================|
            gln.UniformMatrix4fv(modelLoc, 1, false, vals);

            // (2) - void* with object address            
            gln.UniformMatrix4fv_ptr(modelLoc, 1, false, &qTr.Model);

            // (3) - IntPtr with object address
            gln.UniformMatrix4fv_intptr(modelLoc, 1, false, new IntPtr(&qTr.Model));

            // (4) - IntPtr with array ptr
            fixed (void* ptr = vals)
                gln.UniformMatrix4fv_intptr(modelLoc, 1, false, new IntPtr(ptr));
            
            */

            GL.UniformMatrix4fv(modelLoc, 1, false, &model);
            GL.UniformMatrix4fv(viewLoc, 1, false, &view);
            GL.UniformMatrix4fv(projectionLoc, 1, false, &projection);
            GL.Uniform1f(maxYLoc, _maxY);
            GL.Uniform1f(minYLoc, _minY);
            GL.Uniform1f(timeLoc, (float)St.Elapsed.TotalSeconds);
            GL.Uniform1f(discoLoc, _disco);
            CheckError(GL);

            GL.DrawElements(PrimitiveType.GL_TRIANGLES, _indices.Length, DrawElementsType.GL_UNSIGNED_INT, IntPtr.Zero);

            //GL.DrawArrays(GL_LINE_STRIP, 0, new IntPtr(_points.Length));

            CheckError(GL);
            if (_disco > 0.01)
                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        }

    }
}

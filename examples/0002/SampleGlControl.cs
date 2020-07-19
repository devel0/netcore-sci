using System;
using System.Numerics;
using System.Threading;
using Avalonia;
using Silk.NET.OpenGL;
using Silk.NET.Windowing.Common;

namespace SearchAThing.SciExamples
{

    public class SampleGlControl : SearchAThing.OpenGlControlBase
    {

        static SampleGlControl()
        {
        }

        public SampleGlControl()
        {
            AffectsRender<SampleGlControl>(_ObjColorProperty);
        }

        private Vector3 _objColor = new Vector3(0f, 1f, 0f);

        public static readonly DirectProperty<SampleGlControl, Vector3> _ObjColorProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, Vector3>("ObjColor", o => o.ObjColor, (o, v) => o.ObjColor = v);

        public Vector3 ObjColor
        {
            get => _objColor;
            set => SetAndRaise(_ObjColorProperty, ref _objColor, value);
        }

        private uint Vbo;
        private uint Ebo;
        private uint Vao;
        private uint Shader;

        //Vertex shaders are run on each vertex.
        private string VertexShaderSource =>
            "0002.shaders.vertexShader.glsl".GetEmbeddedFileContent<SampleGlControl>();        

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private string FragmentShaderSource =>
            "0002.shaders.fragmentShader.glsl".GetEmbeddedFileContent<SampleGlControl>();         

        //Vertex data, uploaded to the VBO.
        private readonly float[] Vertices =
        {
            // xyz
            -1f, -1f, 0f,
            1f, -1f, 0f,
            0f, 1f, 0f
        };

        //Index data, uploaded to the EBO.
        private readonly uint[] Indices =
        {
            0, 1, 2,
        };

        protected unsafe override void OnOpenGlInit()
        {
            //Creating a vertex array.
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);

            //Initializing a vertex buffer that holds the vertex data.
            Vbo = GL.GenBuffer(); //Creating the buffer.
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo); //Binding the buffer.
            fixed (void* v = &Vertices[0])
            {
                GL.BufferData(BufferTargetARB.ArrayBuffer, (uint)(Vertices.Length * sizeof(uint)), v, BufferUsageARB.StaticDraw); //Setting buffer data.
            }

            //Initializing a element buffer that holds the index data.
            Ebo = GL.GenBuffer(); //Creating the buffer.
            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo); //Binding the buffer.
            fixed (void* i = &Indices[0])
            {
                GL.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw); //Setting buffer data.
            }

            //Creating a vertex shader.
            uint vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, VertexShaderSource);
            GL.CompileShader(vertexShader);

            //Checking the shader for compilation errors.
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling vertex shader {infoLog}");
            }

            //Creating a fragment shader.
            uint fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, FragmentShaderSource);
            GL.CompileShader(fragmentShader);

            //Checking the shader for compilation errors.
            infoLog = GL.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling fragment shader {infoLog}");
            }

            //Combining the shaders under one shader program.
            Shader = GL.CreateProgram();
            GL.AttachShader(Shader, vertexShader);
            GL.AttachShader(Shader, fragmentShader);
            GL.LinkProgram(Shader);

            //Checking the linking for errors.
            string shader = GL.GetProgramInfoLog(Shader);
            if (!string.IsNullOrWhiteSpace(shader))
            {
                Console.WriteLine($"Error linking shader {infoLog}");
            }

            //Delete the no longer useful individual shaders;
            GL.DetachShader(Shader, vertexShader);
            GL.DetachShader(Shader, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            //Tell opengl how to give the data to the shaders.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnOpenGlDeinit()
        {
            GL.DeleteBuffer(Vbo);
            GL.DeleteBuffer(Ebo);
            GL.DeleteVertexArray(Vao);
            GL.DeleteProgram(Shader);
        }

        protected override unsafe void OnOpenGlRender()
        {
            var objColLoc = GL.GetUniformLocation(Shader, "ObjCol");
            GL.Uniform3(objColLoc, ObjColor);

            //Clear the color channel.
            GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            //Bind the geometry and shader.
            GL.BindVertexArray(Vao);
            GL.UseProgram(Shader);

            //Draw the geometry.
            GL.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        }

    }


}
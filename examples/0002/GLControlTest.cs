using OpenToolkit.Graphics.OpenGL4;
using SearchAThing.Sci;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace example_avalonia_opengl
{

    public class GLControlTest : GLControl
    {

        const string VertexShaderSource = @"
            #version 330

            layout(location = 0) in vec4 position;

            void main(void)
            {
                gl_Position = position;
            }
        ";

        const string FragmentShaderSource = @"
            #version 330

            out vec4 outputColor;

            uniform vec4 inColor;

            void main(void)
            {
                outputColor = inColor;
            }
        ";

        // Points of a triangle in normalized device coordinates.
        readonly float[] Points = new float[] {
            // X, Y, Z, W
            -0.5f, 0.0f, 0.0f, 1.0f,
            0.5f, 0.0f, 0.0f, 1.0f,
            0.0f, 0.5f, 0.0f, 1.0f };

        int VertexShader;
        int FragmentShader;
        int ShaderProgram;
        int VertexBufferObject;
        int VertexArrayObject;

        Color _color = Color.Red;

        public Color color
        {
            get { return _color; }
            set
            {
                _color = value;
                System.Console.WriteLine($"set color to {value.ToString()}");
            }
        }        

        bool shader_initialized = false;

        protected override void GetFrame(int w, int h)
        {
            if (!shader_initialized)
            {
                System.Console.WriteLine("===== INIT"); ;
                VertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(VertexShader, VertexShaderSource);
                GL.CompileShader(VertexShader);

                FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(FragmentShader, FragmentShaderSource);
                GL.CompileShader(FragmentShader);

                ShaderProgram = GL.CreateProgram();
                GL.AttachShader(ShaderProgram, VertexShader);
                GL.AttachShader(ShaderProgram, FragmentShader);
                GL.LinkProgram(ShaderProgram);

                VertexBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, Points.Length * sizeof(float), Points, BufferUsageHint.StaticDraw);

                var positionLocation = GL.GetAttribLocation(ShaderProgram, "position");
                VertexArrayObject = GL.GenVertexArray();
                GL.BindVertexArray(VertexArrayObject);
                GL.VertexAttribPointer(positionLocation, 4, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(positionLocation);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
                GL.BindVertexArray(VertexArrayObject);
                GL.UseProgram(ShaderProgram);

                shader_initialized = true;
            }

            GL.ClearColor(Color.LightYellow);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            System.Console.WriteLine($"***GetFrame Color:{color} w:{w} x h:{h}");

            var colorLocation = GL.GetUniformLocation(ShaderProgram, "inColor");

            GL.Uniform4(colorLocation, color);
            //++cnt;
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

    }

}
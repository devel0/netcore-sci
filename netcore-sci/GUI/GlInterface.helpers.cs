/*
follow code from https://github.com/AvaloniaUI/Avalonia/blob/5004606400cf07f6a2cbb7a54de7aa6627d87931/src/Avalonia.OpenGL/GlInterface.cs
*/

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Platform.Interop;

namespace SearchAThing.OpenGL
{
    public unsafe partial class GlInterface
    {

        public void ShaderSourceString(uint shader, string source)
        {
            using (var b = new Utf8Buffer(source))
            {
                var ptr = b.DangerousGetHandle();
                var len = new IntPtr(b.ByteLen);
                ShaderSource(shader, 1, new IntPtr(&ptr), new IntPtr(&len));
            }
        }

        public unsafe string CompileShaderAndGetError(uint shader, string source)
        {
            ShaderSourceString(shader, source);
            CompileShader(shader);
            int compiled;
            GetShaderiv(shader, ShaderParameterName.GL_COMPILE_STATUS, &compiled);
            if (compiled != 0)
                return null;
            int logLength;
            GetShaderiv(shader, ShaderParameterName.GL_INFO_LOG_LENGTH, &logLength);
            if (logLength == 0)
                logLength = 4096;
            var logData = new byte[logLength];
            int len;
            fixed (void* ptr = logData)
                GetShaderInfoLog(shader, logLength, out len, ptr);
            return Encoding.UTF8.GetString(logData, 0, len);
        }

        public unsafe string LinkProgramAndGetError(uint program)
        {
            LinkProgram(program);
            int compiled;
            GetProgramiv(program, ProgramPropertyARB.GL_LINK_STATUS, &compiled);
            if (compiled != 0)
                return null;
            int logLength;
            GetProgramiv(program, ProgramPropertyARB.GL_INFO_LOG_LENGTH, &logLength);
            var logData = new byte[logLength];
            int len;
            fixed (void* ptr = logData)
                GetProgramInfoLog(program, logLength, out len, ptr);
            return Encoding.UTF8.GetString(logData, 0, len);
        }

        public void BindAttribLocationString(uint program, uint index, string name)
        {
            using (var b = new Utf8Buffer(name))
                BindAttribLocation(program, index, b.DangerousGetHandle());
        }

        public uint GenBuffer()
        {
            var rv = new uint[1];
            GenBuffers(1, rv);
            return rv[0];
        }

        public int GetAttribLocationString(uint program, string name)
        {
            using (var b = new Utf8Buffer(name))
                return GetAttribLocation(program, b.DangerousGetHandle());
        }

        public int GetUniformLocationString(uint program, string name)
        {
            using (var b = new Utf8Buffer(name))
                return GetUniformLocation(program, b.DangerousGetHandle());
        }

    }

}
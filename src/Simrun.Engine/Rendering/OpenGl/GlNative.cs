using System;
using System.Runtime.InteropServices;

namespace Simrun.Engine.Rendering.OpenGl;

public static class GlNative
{
    public const uint COLOR_BUFFER_BIT = 0x00004000;
    public const uint DEPTH_BUFFER_BIT = 0x00000100;
    public const uint DEPTH_TEST = 0x0B71;
    public const uint LEQUAL = 0x0203;
    public const uint FLOAT = 0x1406;
    public const uint FALSE = 0;
    public const uint TRIANGLES = 0x0004;
    public const uint ARRAY_BUFFER = 0x8892;
    public const uint ELEMENT_ARRAY_BUFFER = 0x8893;
    public const uint STATIC_DRAW = 0x88E4;
    public const uint VERTEX_SHADER = 0x8B31;
    public const uint FRAGMENT_SHADER = 0x8B30;
    public const uint COMPILE_STATUS = 0x8B81;
    public const uint LINK_STATUS = 0x8B82;
    public const uint UNSIGNED_INT = 0x1405;
    public const uint RGBA = 0x1908;
    public const uint UNSIGNED_BYTE = 0x1401;
    public const uint FRONT = 0x0404;
    public const uint DEBUG_OUTPUT = 0x92E0;
    public const uint DEBUG_OUTPUT_SYNCHRONOUS = 0x8242;
    public const uint NO_ERROR = 0;

    private delegate void GlClearColor(float r, float g, float b, float a);
    private delegate void GlClear(uint mask);
    private delegate void GlViewport(int x, int y, int width, int height);
    private delegate void GlEnable(uint cap);
    private delegate void GlDepthFunc(uint func);
    private delegate void GlClearDepth(double depth);
    private delegate uint GlCreateShader(uint type);
    private delegate void GlShaderSource(uint shader, int count, string[] source, int[] length);
    private delegate void GlCompileShader(uint shader);
    private delegate void GlGetShaderiv(uint shader, uint pname, out int param);
    private delegate void GlGetShaderInfoLog(uint shader, int bufSize, out int length, System.Text.StringBuilder infoLog);
    private delegate uint GlCreateProgram();
    private delegate void GlAttachShader(uint program, uint shader);
    private delegate void GlLinkProgram(uint program);
    private delegate void GlGetProgramiv(uint program, uint pname, out int param);
    private delegate void GlGetProgramInfoLog(uint program, int bufSize, out int length, System.Text.StringBuilder infoLog);
    private delegate void GlDeleteShader(uint shader);
    private delegate void GlUseProgram(uint program);
    private delegate int GlGetUniformLocation(uint program, string name);
    private unsafe delegate void GlUniformMatrix4fv(int location, int count, bool transpose, float* value);
    private delegate void GlUniform1f(int location, float v0);
    private delegate void GlUniform3f(int location, float v0, float v1, float v2);
    private unsafe delegate void GlGenVertexArrays(int n, uint* arrays);
    private delegate void GlBindVertexArray(uint array);
    private unsafe delegate void GlGenBuffers(int n, uint* buffers);
    private delegate void GlBindBuffer(uint target, uint buffer);
    private unsafe delegate void GlBufferData(uint target, IntPtr size, void* data, uint usage);
    private delegate void GlEnableVertexAttribArray(uint index);
    private delegate void GlVertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, IntPtr pointer);
    private delegate void GlDrawElements(uint mode, int count, uint type, IntPtr indices);
    private delegate void GlReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr data);
    private delegate void GlReadBuffer(uint mode);
    private delegate uint GlGetError();
    private delegate void GlDebugMessageCallback(DebugProc callback, IntPtr userParam);
    private delegate void GlDebugMessageControl(uint source, uint type, uint severity, int count, IntPtr ids, bool enabled);

    private static GlClearColor? _clearColor;
    private static GlClear? _clear;
    private static GlViewport? _viewport;
    private static GlEnable? _enable;
    private static GlDepthFunc? _depthFunc;
    private static GlClearDepth? _clearDepth;
    private static GlCreateShader? _createShader;
    private static GlShaderSource? _shaderSource;
    private static GlCompileShader? _compileShader;
    private static GlGetShaderiv? _getShaderiv;
    private static GlGetShaderInfoLog? _getShaderInfoLog;
    private static GlCreateProgram? _createProgram;
    private static GlAttachShader? _attachShader;
    private static GlLinkProgram? _linkProgram;
    private static GlGetProgramiv? _getProgramiv;
    private static GlGetProgramInfoLog? _getProgramInfoLog;
    private static GlDeleteShader? _deleteShader;
    private static GlUseProgram? _useProgram;
    private static GlGetUniformLocation? _getUniformLocation;
    private static GlUniformMatrix4fv? _uniformMatrix4fv;
    private static GlUniform1f? _uniform1f;
    private static GlUniform3f? _uniform3f;
    private static GlGenVertexArrays? _genVertexArrays;
    private static GlBindVertexArray? _bindVertexArray;
    private static GlGenBuffers? _genBuffers;
    private static GlBindBuffer? _bindBuffer;
    private static GlBufferData? _bufferData;
    private static GlEnableVertexAttribArray? _enableVertexAttribArray;
    private static GlVertexAttribPointer? _vertexAttribPointer;
    private static GlDrawElements? _drawElements;
    private static GlReadPixels? _readPixels;
    private static GlReadBuffer? _readBuffer;
    private static GlGetError? _getError;
    private static GlDebugMessageCallback? _debugCallback;
    private static GlDebugMessageControl? _debugControl;

    public static void Load()
    {
        _clearColor = LoadFunction<GlClearColor>("glClearColor");
        _clear = LoadFunction<GlClear>("glClear");
        _viewport = LoadFunction<GlViewport>("glViewport");
        _enable = LoadFunction<GlEnable>("glEnable");
        _depthFunc = LoadFunction<GlDepthFunc>("glDepthFunc");
        _clearDepth = LoadFunction<GlClearDepth>("glClearDepth");
        _createShader = LoadFunction<GlCreateShader>("glCreateShader");
        _shaderSource = LoadFunction<GlShaderSource>("glShaderSource");
        _compileShader = LoadFunction<GlCompileShader>("glCompileShader");
        _getShaderiv = LoadFunction<GlGetShaderiv>("glGetShaderiv");
        _getShaderInfoLog = LoadFunction<GlGetShaderInfoLog>("glGetShaderInfoLog");
        _createProgram = LoadFunction<GlCreateProgram>("glCreateProgram");
        _attachShader = LoadFunction<GlAttachShader>("glAttachShader");
        _linkProgram = LoadFunction<GlLinkProgram>("glLinkProgram");
        _getProgramiv = LoadFunction<GlGetProgramiv>("glGetProgramiv");
        _getProgramInfoLog = LoadFunction<GlGetProgramInfoLog>("glGetProgramInfoLog");
        _deleteShader = LoadFunction<GlDeleteShader>("glDeleteShader");
        _useProgram = LoadFunction<GlUseProgram>("glUseProgram");
        _getUniformLocation = LoadFunction<GlGetUniformLocation>("glGetUniformLocation");
        _uniformMatrix4fv = LoadFunction<GlUniformMatrix4fv>("glUniformMatrix4fv");
        _uniform1f = LoadFunction<GlUniform1f>("glUniform1f");
        _uniform3f = LoadFunction<GlUniform3f>("glUniform3f");
        _genVertexArrays = LoadFunction<GlGenVertexArrays>("glGenVertexArrays");
        _bindVertexArray = LoadFunction<GlBindVertexArray>("glBindVertexArray");
        _genBuffers = LoadFunction<GlGenBuffers>("glGenBuffers");
        _bindBuffer = LoadFunction<GlBindBuffer>("glBindBuffer");
        _bufferData = LoadFunction<GlBufferData>("glBufferData");
        _enableVertexAttribArray = LoadFunction<GlEnableVertexAttribArray>("glEnableVertexAttribArray");
        _vertexAttribPointer = LoadFunction<GlVertexAttribPointer>("glVertexAttribPointer");
        _drawElements = LoadFunction<GlDrawElements>("glDrawElements");
        _readPixels = LoadFunction<GlReadPixels>("glReadPixels");
        _readBuffer = LoadFunction<GlReadBuffer>("glReadBuffer");
        _getError = LoadFunction<GlGetError>("glGetError");
        _debugCallback = LoadFunction<GlDebugMessageCallback>("glDebugMessageCallback");
        _debugControl = LoadFunction<GlDebugMessageControl>("glDebugMessageControl");
    }

    public static void ClearColor(float r, float g, float b, float a) => _clearColor?.Invoke(r, g, b, a);
    public static void Clear(uint mask) => _clear?.Invoke(mask);
    public static void Viewport(int x, int y, int width, int height) => _viewport?.Invoke(x, y, width, height);
    public static void Enable(uint cap) => _enable?.Invoke(cap);
    public static void DepthFunc(uint func) => _depthFunc?.Invoke(func);
    public static void ClearDepth(double depth) => _clearDepth?.Invoke(depth);
    public static uint CreateShader(uint type) => _createShader!.Invoke(type);
    public static void ShaderSource(uint shader, string source)
    {
        var sources = new[] { source };
        var lengths = new[] { source.Length };
        _shaderSource!.Invoke(shader, 1, sources, lengths);
    }
    public static void CompileShader(uint shader) => _compileShader!.Invoke(shader);
    public static int GetShader(int shader, uint pname)
    {
        _getShaderiv!.Invoke((uint)shader, pname, out var param);
        return param;
    }
    public static string GetShaderInfo(int shader)
    {
        var sb = new System.Text.StringBuilder(1024);
        _getShaderInfoLog!.Invoke((uint)shader, sb.Capacity, out var len, sb);
        return sb.ToString(0, len);
    }
    public static uint CreateProgram() => _createProgram!.Invoke();
    public static void AttachShader(uint program, uint shader) => _attachShader!.Invoke(program, shader);
    public static void LinkProgram(uint program) => _linkProgram!.Invoke(program);
    public static int GetProgram(uint program, uint pname)
    {
        _getProgramiv!.Invoke(program, pname, out var param);
        return param;
    }
    public static string GetProgramInfo(uint program)
    {
        var sb = new System.Text.StringBuilder(1024);
        _getProgramInfoLog!.Invoke(program, sb.Capacity, out var len, sb);
        return sb.ToString(0, len);
    }
    public static void DeleteShader(uint shader) => _deleteShader?.Invoke(shader);
    public static void UseProgram(uint program) => _useProgram?.Invoke(program);
    public static int GetUniformLocation(uint program, string name) => _getUniformLocation!.Invoke(program, name);
    public unsafe static void UniformMatrix4(int location, float* value, bool transpose = true) => _uniformMatrix4fv!.Invoke(location, 1, transpose, value);
    public static void Uniform1(int location, float v0) => _uniform1f?.Invoke(location, v0);
    public static void Uniform3(int location, float x, float y, float z) => _uniform3f!.Invoke(location, x, y, z);
    public unsafe static uint GenVertexArray()
    {
        uint id;
        _genVertexArrays!.Invoke(1, &id);
        return id;
    }
    public static void BindVertexArray(uint id) => _bindVertexArray!.Invoke(id);
    public unsafe static uint GenBuffer()
    {
        uint id;
        _genBuffers!.Invoke(1, &id);
        return id;
    }
    public static void BindBuffer(uint target, uint id) => _bindBuffer!.Invoke(target, id);
    public unsafe static void BufferData(uint target, ReadOnlySpan<float> data, uint usage)
    {
        fixed (float* ptr = data)
        {
            _bufferData!.Invoke(target, new IntPtr(data.Length * sizeof(float)), ptr, usage);
        }
    }
    public unsafe static void BufferData(uint target, ReadOnlySpan<uint> data, uint usage)
    {
        fixed (uint* ptr = data)
        {
            _bufferData!.Invoke(target, new IntPtr(data.Length * sizeof(uint)), ptr, usage);
        }
    }
    public static void EnableVertexAttribArray(uint index) => _enableVertexAttribArray!.Invoke(index);
    public static void VertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, IntPtr pointer) =>
        _vertexAttribPointer!.Invoke(index, size, type, normalized, stride, pointer);
    public static void DrawElements(int count) => _drawElements!.Invoke(TRIANGLES, count, UNSIGNED_INT, IntPtr.Zero);
    public static void ReadBuffer(uint mode) => _readBuffer!.Invoke(mode);
    public static void ReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr data) =>
        _readPixels!.Invoke(x, y, width, height, format, type, data);
    public static uint GetError() => _getError is null ? NO_ERROR : _getError.Invoke();
    public static void DebugMessageCallback(DebugProc callback, IntPtr user) => _debugCallback?.Invoke(callback, user);
    public static void DebugMessageControl(uint source, uint type, uint severity, bool enabled) =>
        _debugControl?.Invoke(source, type, severity, 0, IntPtr.Zero, enabled);

    public static bool HasRequiredCore() =>
        _clearColor is not null &&
        _clear is not null &&
        _viewport is not null &&
        _enable is not null &&
        _depthFunc is not null &&
        _createShader is not null &&
        _createProgram is not null &&
        _drawElements is not null &&
        _bindVertexArray is not null &&
        _bindBuffer is not null &&
        _getError is not null;

    public delegate void DebugProc(uint source, uint type, uint id, uint severity, int length, IntPtr message, IntPtr userParam);

    private static T? LoadFunction<T>(string name) where T : class
    {
        var proc = Wgl.wglGetProcAddress(name);
        if (proc == IntPtr.Zero)
        {
            var module = NativeLibrary.Load("opengl32.dll");
            NativeLibrary.TryGetExport(module, name, out proc);
        }

        return proc == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(proc, typeof(T)) as T;
    }
}

internal static class Wgl
{
    [DllImport("opengl32.dll", EntryPoint = "wglCreateContext")]
    public static extern IntPtr wglCreateContext(IntPtr hdc);

    [DllImport("opengl32.dll", EntryPoint = "wglMakeCurrent")]
    public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

    [DllImport("opengl32.dll", EntryPoint = "wglDeleteContext")]
    public static extern bool wglDeleteContext(IntPtr hglrc);

    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress", CharSet = CharSet.Ansi)]
    public static extern IntPtr wglGetProcAddress(string name);

    private delegate bool SwapIntervalProc(int interval);
    private static SwapIntervalProc? _swapInterval;

    public static void SwapInterval(int interval)
    {
        _swapInterval ??= Marshal.GetDelegateForFunctionPointer<SwapIntervalProc>(wglGetProcAddress("wglSwapIntervalEXT"));
        _swapInterval?.Invoke(interval);
    }
}

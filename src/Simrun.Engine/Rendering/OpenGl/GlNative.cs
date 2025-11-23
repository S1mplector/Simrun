using System;
using System.Runtime.InteropServices;

namespace Simrun.Engine.Rendering.OpenGl;

internal static class GlNative
{
    public const uint COLOR_BUFFER_BIT = 0x00004000;

    private delegate void GlClearColor(float r, float g, float b, float a);
    private delegate void GlClear(uint mask);
    private delegate void GlViewport(int x, int y, int width, int height);

    private static GlClearColor? _clearColor;
    private static GlClear? _clear;
    private static GlViewport? _viewport;

    public static void Load()
    {
        _clearColor = LoadFunction<GlClearColor>("glClearColor");
        _clear = LoadFunction<GlClear>("glClear");
        _viewport = LoadFunction<GlViewport>("glViewport");
    }

    public static void ClearColor(float r, float g, float b, float a) => _clearColor?.Invoke(r, g, b, a);
    public static void Clear(uint mask) => _clear?.Invoke(mask);
    public static void Viewport(int x, int y, int width, int height) => _viewport?.Invoke(x, y, width, height);

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
}

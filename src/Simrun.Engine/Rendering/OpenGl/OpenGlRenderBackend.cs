using System;
using System.Collections.Generic;

namespace Simrun.Engine.Rendering.OpenGl;

public sealed class OpenGlRenderBackend : IRenderBackend
{
    private Win32Window? _window;
    private RenderSurface _surface;
    private bool _ready;
    private uint _program;
    private int _uMvp;
    private int _uColor;
    private readonly Dictionary<Mesh, GlMeshBuffers> _meshCache = new();

    public void Initialize(RenderSurface surface)
    {
        _surface = surface;
        _window = new Win32Window(surface);
        _window.MakeCurrent();
        GlNative.Load();
        GlNative.Viewport(0, 0, surface.Width, surface.Height);
        GlNative.Enable(GlNative.DEPTH_TEST);
        GlNative.DepthFunc(GlNative.LEQUAL);
        GlNative.ClearDepth(1.0);
        BuildPipeline();
        _ready = true;
    }

    public void Resize(RenderSurface surface)
    {
        _surface = surface;
        _window?.Resize(surface);
        GlNative.Viewport(0, 0, surface.Width, surface.Height);
    }

    public void Render(Scene scene, Camera camera)
    {
        if (!_ready || _window is null)
        {
            return;
        }

        _window.PumpMessages();

        GlNative.ClearColor(0.08f, 0.08f, 0.1f, 1f);
        GlNative.Clear(GlNative.COLOR_BUFFER_BIT | GlNative.DEPTH_BUFFER_BIT);

        GlNative.UseProgram(_program);

        var aspect = (float)_surface.Width / _surface.Height;
        var view = camera.ViewMatrix;
        var projection = camera.ProjectionMatrix(aspect);

        Span<float> matrixBuffer = stackalloc float[16];

        foreach (var renderable in scene.Renderables)
        {
            var buffers = EnsureMeshBuffers(renderable.Mesh);
            GlNative.BindVertexArray(buffers.Vao);

            var world = renderable.Transform.ToMatrix();
            var mvp = world * view * projection;
            WriteMatrix(mvp, matrixBuffer);

            unsafe
            {
                fixed (float* ptr = matrixBuffer)
                {
                    GlNative.UniformMatrix4(_uMvp, ptr);
                }
            }

            GlNative.Uniform3(_uColor, renderable.Material.Albedo.X, renderable.Material.Albedo.Y, renderable.Material.Albedo.Z);
            GlNative.DrawElements(buffers.IndexCount);
        }

        _window.Swap();
    }

    public void Shutdown()
    {
        _ready = false;
        _window?.Dispose();
        _window = null;
    }

    private void BuildPipeline()
    {
        var vertexShaderSource = @"#version 330 core
layout (location = 0) in vec3 aPos;
uniform mat4 uMvp;
void main()
{
    gl_Position = uMvp * vec4(aPos, 1.0);
}";

        var fragmentShaderSource = @"#version 330 core
out vec4 FragColor;
uniform vec3 uColor;
void main()
{
    FragColor = vec4(uColor, 1.0);
}";

        var vert = CompileShader(GlNative.VERTEX_SHADER, vertexShaderSource);
        var frag = CompileShader(GlNative.FRAGMENT_SHADER, fragmentShaderSource);

        _program = GlNative.CreateProgram();
        GlNative.AttachShader(_program, vert);
        GlNative.AttachShader(_program, frag);
        GlNative.LinkProgram(_program);

        if (GlNative.GetProgram(_program, GlNative.LINK_STATUS) == 0)
        {
            throw new InvalidOperationException($"GL program link failed: {GlNative.GetProgramInfo(_program)}");
        }

        GlNative.DeleteShader(vert);
        GlNative.DeleteShader(frag);

        _uMvp = GlNative.GetUniformLocation(_program, "uMvp");
        _uColor = GlNative.GetUniformLocation(_program, "uColor");
    }

    private static uint CompileShader(uint type, string source)
    {
        var shader = GlNative.CreateShader(type);
        GlNative.ShaderSource(shader, source);
        GlNative.CompileShader(shader);

        if (GlNative.GetShader((int)shader, GlNative.COMPILE_STATUS) == 0)
        {
            throw new InvalidOperationException($"GL shader compile failed: {GlNative.GetShaderInfo((int)shader)}");
        }

        return shader;
    }

    private GlMeshBuffers EnsureMeshBuffers(Mesh mesh)
    {
        if (_meshCache.TryGetValue(mesh, out var buffers))
        {
            return buffers;
        }

        var vao = GlNative.GenVertexArray();
        GlNative.BindVertexArray(vao);

        var vbo = GlNative.GenBuffer();
        GlNative.BindBuffer(GlNative.ARRAY_BUFFER, vbo);

        var flattened = new float[mesh.Vertices.Count * 3];
        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            var v = mesh.Vertices[i];
            var baseIdx = i * 3;
            flattened[baseIdx] = v.X;
            flattened[baseIdx + 1] = v.Y;
            flattened[baseIdx + 2] = v.Z;
        }

        GlNative.BufferData(GlNative.ARRAY_BUFFER, flattened, GlNative.STATIC_DRAW);

        var ebo = GlNative.GenBuffer();
        GlNative.BindBuffer(GlNative.ELEMENT_ARRAY_BUFFER, ebo);

        var indices = new uint[mesh.Indices.Count];
        for (var i = 0; i < mesh.Indices.Count; i++)
        {
            indices[i] = (uint)mesh.Indices[i];
        }

        GlNative.BufferData(GlNative.ELEMENT_ARRAY_BUFFER, indices, GlNative.STATIC_DRAW);

        GlNative.EnableVertexAttribArray(0);
        GlNative.VertexAttribPointer(0, 3, GlNative.FLOAT, false, 3 * sizeof(float), IntPtr.Zero);

        var created = new GlMeshBuffers(vao, vbo, ebo, indices.Length);
        _meshCache.Add(mesh, created);
        return created;
    }

    private static void WriteMatrix(System.Numerics.Matrix4x4 m, Span<float> dest)
    {
        dest[0] = m.M11; dest[1] = m.M12; dest[2] = m.M13; dest[3] = m.M14;
        dest[4] = m.M21; dest[5] = m.M22; dest[6] = m.M23; dest[7] = m.M24;
        dest[8] = m.M31; dest[9] = m.M32; dest[10] = m.M33; dest[11] = m.M34;
        dest[12] = m.M41; dest[13] = m.M42; dest[14] = m.M43; dest[15] = m.M44;
    }
}

internal readonly record struct GlMeshBuffers(uint Vao, uint Vbo, uint Ebo, int IndexCount);

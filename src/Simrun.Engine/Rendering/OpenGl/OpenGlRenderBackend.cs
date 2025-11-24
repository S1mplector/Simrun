using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Simrun.Engine.Rendering.OpenGl;

public sealed class OpenGlRenderBackend : IRenderBackend
{
    private Win32Window? _window;
    private RenderSurface _surface;
    private bool _ready;
    private uint _program;
    private int _uModel;
    private int _uColor;
    private int _uViewProj;
    private int _uLightDir;
    private int _uCameraPos;
    private int _uRoughness;
    private int _uFogColor;
    private int _uGridColor;
    private readonly Dictionary<Mesh, GlMeshBuffers> _meshCache = new();
    private static readonly GlNative.DebugProc DebugProc = OnDebugMessage;

    public void Initialize(RenderSurface surface)
    {
        _surface = surface;
        _window = new Win32Window(surface);
        _window.MakeCurrent();
        GlNative.Load();
        TryEnableDebugOutput();
        Wgl.SwapInterval(surface.VSync ? 1 : 0);
        GlNative.Viewport(0, 0, surface.Width, surface.Height);
        GlNative.Enable(GlNative.DEPTH_TEST);
        GlNative.DepthFunc(GlNative.LEQUAL);
        GlNative.ClearDepth(1.0);
        BuildPipeline();
        ValidateContext();
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

        GlNative.ClearColor(0.18f, 0.25f, 0.35f, 1f);
        GlNative.Clear(GlNative.COLOR_BUFFER_BIT | GlNative.DEPTH_BUFFER_BIT);
        CheckError("Clear");

        GlNative.UseProgram(_program);

        var aspect = (float)_surface.Width / _surface.Height;
        var view = camera.ViewMatrix;
        var projection = camera.ProjectionMatrix(aspect);
        var viewProj = projection * view;

        Span<float> matrixBuffer = stackalloc float[16];
        WriteMatrix(viewProj, matrixBuffer);
        unsafe
        {
            fixed (float* ptr = matrixBuffer)
            {
                GlNative.UniformMatrix4(_uViewProj, ptr);
            }
        }
        GlNative.Uniform3(_uLightDir, -0.5f, -1.0f, -0.3f);
        GlNative.Uniform3(_uCameraPos, camera.Transform.Position.X, camera.Transform.Position.Y, camera.Transform.Position.Z);
        GlNative.Uniform3(_uFogColor, 0.12f, 0.16f, 0.2f);
        GlNative.Uniform3(_uGridColor, 0.05f, 0.07f, 0.1f);

        foreach (var renderable in scene.Renderables)
        {
            if (renderable.IsDebug && !scene.ShowDebug)
            {
                continue;
            }

            var buffers = EnsureMeshBuffers(renderable.Mesh);
            GlNative.BindVertexArray(buffers.Vao);

            var world = renderable.Transform.ToMatrix();
            WriteMatrix(world, matrixBuffer);

            unsafe
            {
                fixed (float* ptr = matrixBuffer)
                {
                    GlNative.UniformMatrix4(_uModel, ptr);
                }
            }

            GlNative.Uniform3(_uColor, renderable.Material.Albedo.X, renderable.Material.Albedo.Y, renderable.Material.Albedo.Z);
            GlNative.Uniform1(_uRoughness, renderable.Material.Roughness);
            GlNative.DrawElements(buffers.IndexCount);
            CheckError("Draw");
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
layout (location = 1) in vec3 aNormal;
uniform mat4 uModel;
uniform mat4 uViewProj;
out vec3 vNormal;
out vec3 vWorldPos;
void main()
{
    vec4 world = uModel * vec4(aPos, 1.0);
    vWorldPos = world.xyz;
    vNormal = mat3(uModel) * aNormal;
    gl_Position = uViewProj * world;
}";

        var fragmentShaderSource = @"#version 330 core
in vec3 vNormal;
in vec3 vWorldPos;
out vec4 FragColor;
uniform vec3 uColor;
uniform vec3 uLightDir;
uniform vec3 uCameraPos;
uniform float uRoughness;
uniform vec3 uFogColor;
uniform vec3 uGridColor;
void main()
{
    vec3 N = normalize(vNormal);
    vec3 L = normalize(-uLightDir);
    float NdotL = max(dot(N, L), 0.0);
    float ambient = 0.2;
    float diffuse = (1.0 - uRoughness) * NdotL;
    vec3 base = uColor * (ambient + diffuse);

    vec2 gridUv = vWorldPos.xz * 0.1;
    vec2 cell = abs(fract(gridUv) - 0.5) * 2.0;
    float line = 1.0 - smoothstep(0.85, 1.0, max(cell.x, cell.y));
    vec3 grid = mix(base, uGridColor, line * 0.35);

    float heightFactor = clamp(vWorldPos.y * 0.02 + 0.5, 0.0, 1.0);
    vec3 sky = mix(vec3(0.10, 0.14, 0.18), vec3(0.28, 0.40, 0.55), heightFactor);

    float dist = length(vWorldPos - uCameraPos);
    float fog = clamp(exp(-dist * 0.02), 0.0, 1.0);
    vec3 color = mix(uFogColor, mix(sky, grid, 0.7), fog);
    FragColor = vec4(color, 1.0);
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

        _uModel = GlNative.GetUniformLocation(_program, "uModel");
        _uColor = GlNative.GetUniformLocation(_program, "uColor");
        _uViewProj = GlNative.GetUniformLocation(_program, "uViewProj");
        _uLightDir = GlNative.GetUniformLocation(_program, "uLightDir");
        _uCameraPos = GlNative.GetUniformLocation(_program, "uCameraPos");
        _uRoughness = GlNative.GetUniformLocation(_program, "uRoughness");
        _uFogColor = GlNative.GetUniformLocation(_program, "uFogColor");
        _uGridColor = GlNative.GetUniformLocation(_program, "uGridColor");
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

    private static void ValidateContext()
    {
        if (!GlNative.HasRequiredCore())
        {
            throw new InvalidOperationException("OpenGL core functions failed to load.");
        }
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

        var flattened = new float[mesh.Vertices.Count * 6];
        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            var v = mesh.Vertices[i];
            var n = mesh.Normals[i];
            var baseIdx = i * 6;
            flattened[baseIdx] = v.X;
            flattened[baseIdx + 1] = v.Y;
            flattened[baseIdx + 2] = v.Z;
            flattened[baseIdx + 3] = n.X;
            flattened[baseIdx + 4] = n.Y;
            flattened[baseIdx + 5] = n.Z;
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
        GlNative.VertexAttribPointer(0, 3, GlNative.FLOAT, false, 6 * sizeof(float), IntPtr.Zero);
        GlNative.EnableVertexAttribArray(1);
        GlNative.VertexAttribPointer(1, 3, GlNative.FLOAT, false, 6 * sizeof(float), new IntPtr(3 * sizeof(float)));

        var created = new GlMeshBuffers(vao, vbo, ebo, indices.Length);
        _meshCache.Add(mesh, created);
        return created;
    }

    private static void OnDebugMessage(uint source, uint type, uint id, uint severity, int length, IntPtr message, IntPtr userParam)
    {
        var text = Marshal.PtrToStringAnsi(message, length);
        Console.WriteLine($"[GL DEBUG] 0x{severity:X} 0x{type:X} {id}: {text}");
    }

    private void TryEnableDebugOutput()
    {
        try
        {
            GlNative.Enable(GlNative.DEBUG_OUTPUT);
            GlNative.Enable(GlNative.DEBUG_OUTPUT_SYNCHRONOUS);
            GlNative.DebugMessageCallback(DebugProc, IntPtr.Zero);
            GlNative.DebugMessageControl(0, 0, 0, true);
        }
        catch
        {
            // ignore if unavailable
        }
    }

    private static void CheckError(string scope)
    {
        var err = GlNative.GetError();
        if (err != GlNative.NO_ERROR)
        {
            Console.WriteLine($"[GL ERROR] {scope} -> 0x{err:X}");
        }
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

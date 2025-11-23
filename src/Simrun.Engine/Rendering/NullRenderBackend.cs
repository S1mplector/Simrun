using System;

namespace Simrun.Engine.Rendering;

/// <summary>
/// Placeholder backend that does nothing but exercises the render loop API surface.
/// Swap this for a real backend (OpenGL/Vulkan/Direct3D) in the engine host.
/// </summary>
public sealed class NullRenderBackend : IRenderBackend
{
    private RenderSurface _surface;

    public void Initialize(RenderSurface surface)
    {
        _surface = surface;
    }

    public void Resize(RenderSurface surface)
    {
        _surface = surface;
    }

    public void Render(Scene scene, Camera camera)
    {
        _ = scene;
        _ = camera;
        // Intentionally no-op.
    }

    public void Shutdown()
    {
        _surface = default;
    }
}

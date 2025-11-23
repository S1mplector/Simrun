namespace Simrun.Engine.Rendering.OpenGl;

/// <summary>
/// Placeholder for a real OpenGL backend. Implement using a windowing/context library (e.g., Silk.NET/GLFW).
/// Currently acts like a no-op to keep the host compiling without native dependencies.
/// </summary>
public sealed class OpenGlRenderBackend : IRenderBackend
{
    public void Initialize(RenderSurface surface)
    {
        _ = surface;
    }

    public void Resize(RenderSurface surface)
    {
        _ = surface;
    }

    public void Render(Scene scene, Camera camera)
    {
        _ = scene;
        _ = camera;
    }

    public void Shutdown()
    {
    }
}

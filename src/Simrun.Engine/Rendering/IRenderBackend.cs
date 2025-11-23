namespace Simrun.Engine.Rendering;

public interface IRenderBackend
{
    void Initialize(RenderSurface surface);
    void Resize(RenderSurface surface);
    void Render(Scene scene, Camera camera);
    void Shutdown();
}

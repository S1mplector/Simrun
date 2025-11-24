namespace Simrun.Engine.Rendering;

public readonly record struct RenderSurface(int Width, int Height, string Title, bool VSync = true)
{
    public RenderSurface WithSize(int width, int height) => new(width, height, Title, VSync);
    public RenderSurface WithVSync(bool enabled) => new(Width, Height, Title, enabled);
}

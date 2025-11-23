namespace Simrun.Engine.Rendering;

public readonly record struct RenderSurface(int Width, int Height, string Title)
{
    public RenderSurface WithSize(int width, int height) => new(width, height, Title);
}

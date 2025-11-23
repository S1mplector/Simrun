namespace Simrun.Engine.Rendering.OpenGl;

public sealed class OpenGlRenderBackend : IRenderBackend
{
    private Win32Window? _window;
    private RenderSurface _surface;
    private bool _ready;

    public void Initialize(RenderSurface surface)
    {
        _surface = surface;
        _window = new Win32Window(surface);
        _window.MakeCurrent();
        GlNative.Load();
        GlNative.Viewport(0, 0, surface.Width, surface.Height);
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
        GlNative.Clear(GlNative.COLOR_BUFFER_BIT);

        // TODO: upload scene geometry and draw. For now just clear the screen.

        _window.Swap();
    }

    public void Shutdown()
    {
        _ready = false;
        _window?.Dispose();
        _window = null;
    }
}

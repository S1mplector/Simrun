using System.Collections.Generic;

namespace Simrun.Engine.Rendering;

public sealed class Scene
{
    private readonly List<Renderable> _renderables = new();

    public IReadOnlyList<Renderable> Renderables => _renderables;
    public bool ShowDebug { get; set; }

    public void Add(Renderable renderable) => _renderables.Add(renderable);
}

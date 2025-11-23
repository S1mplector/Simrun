using Simrun.Presentation;
using Simrun.Engine.Rendering;
using Simrun.Presentation.Input;
using Simrun.Presentation.Cameras;

var services = GameBootstrapper.Build();
var renderSurface = new RenderSurface(1280, 720, "Simrun Prototype");
IRenderBackend renderer = new NullRenderBackend();
var loop = new GameLoop(services, renderer, renderSurface);
loop.Run();

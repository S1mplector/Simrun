# Simrun

Early scaffold for a 3D parkour speedrun game inspired by Speedrun 4. The repository is structured with a clean hexagonal architecture so the core domain remains independent from rendering, physics backends, or tooling choices.

## Project layout

- `src/Simrun.Domain` — core domain models (levels, player state, run state, value objects).
- `src/Simrun.Application` — use-cases and ports the domain depends on (physics, level loading, time, run storage).
- `src/Simrun.Infrastructure` — adapters that implement ports (in-memory levels, naive physics, time).
- `src/Simrun.Presentation` — simple host app that wires everything together and runs a demo loop.
- `docs/ARCHITECTURE.md` — deeper notes on layering and extension points.

## Getting started

1. Ensure the .NET SDK 10 is available (`dotnet --version` should show `10.x.x`).
2. Build the solution:
   ```bash
   dotnet build
   ```
3. Run the demo host (prints a lightweight fixed-step simulation to the console):
   ```bash
   dotnet run --project src/Simrun.Presentation/Simrun.Presentation.csproj
   ```

## Next steps

- Swap the `NaivePhysicsEngine` for an engine-backed implementation (Unity, Godot, Stride, or custom OpenGL).
- Replace the `InMemoryLevelRepository` with level data loaded from authored assets.
- Extend the presentation layer with rendering, input, audio, and networked ghost replays while keeping the domain untouched.

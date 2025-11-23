# Architecture

Simrun follows a hexagonal / ports-and-adapters architecture to keep the core gameplay model independent from infrastructure and presentation concerns.

## Layers

- **Domain (`Simrun.Domain`)** — entities and value objects (`LevelDefinition`, `RunState`, `PlayerState`, `Vector3`) plus light domain logic (goal detection, run lifecycle).
- **Application (`Simrun.Application`)** — use-cases (`StartRunHandler`, `TickRunHandler`) and the ports they depend on (`ILevelRepository`, `IRunStateStore`, `IPhysicsEngine`, `ITimeProvider`). Nothing here depends on concrete infrastructure.
- **Infrastructure (`Simrun.Infrastructure`)** — adapters implementing ports (in-memory and filesystem/JSON levels, naive physics, system time, run store). Swap these without touching domain code.
- **Engine (`Simrun.Engine`)** — rendering primitives and backend abstraction. Currently includes placeholder types and a null backend; intended for a custom 3D renderer.
- **Presentation (`Simrun.Presentation`)** — the host/composition root. Today it's a console loop; later it can be a custom renderer host using the engine layer, still talking only to the application layer.

## Flow

1. The host wires concrete adapters into the application layer (`GameBootstrapper`).
2. Presentation gathers player input and feeds it into application use-cases (`TickRunHandler`).
3. Use-cases call ports to talk to physics/levels/run storage; domain state is returned and forwarded back to the host for rendering/UI.

## Extension points

- Swap `NaivePhysicsEngine` for an engine-backed integration that uses colliders, rigidbodies, and raycasts.
- Replace `InMemoryLevelRepository` with authored level assets (JSON files or engine-authored scenes) or procedural generators.
- Persist runs with a database or replay file by implementing `IRunStateStore`.
- Add new use-cases (checkpointing, leaderboards, ghost playback) in the application layer without coupling to the engine.

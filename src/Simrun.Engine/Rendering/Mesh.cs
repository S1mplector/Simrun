using System;
using System.Collections.Generic;
using System.Numerics;

namespace Simrun.Engine.Rendering;

public sealed class Mesh
{
    public Mesh(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices)
    {
        if (indices.Count % 3 != 0)
        {
            throw new ArgumentException("Indices must be a multiple of 3 for triangles.", nameof(indices));
        }

        Vertices = vertices;
        Indices = indices;
    }

    public IReadOnlyList<Vector3> Vertices { get; }
    public IReadOnlyList<int> Indices { get; }

    public static Mesh CreateQuad(float size = 1f)
    {
        var half = size * 0.5f;
        var verts = new[]
        {
            new Vector3(-half, 0f, -half),
            new Vector3(half, 0f, -half),
            new Vector3(half, 0f, half),
            new Vector3(-half, 0f, half)
        };

        var tris = new[] { 0, 1, 2, 0, 2, 3 };
        return new Mesh(verts, tris);
    }
}

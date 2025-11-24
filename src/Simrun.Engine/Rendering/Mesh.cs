using System;
using System.Collections.Generic;
using System.Numerics;

namespace Simrun.Engine.Rendering;

public sealed class Mesh
{
    public Mesh(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices, IReadOnlyList<Vector3>? normals = null)
    {
        if (indices.Count % 3 != 0)
        {
            throw new ArgumentException("Indices must be a multiple of 3 for triangles.", nameof(indices));
        }

        Vertices = vertices;
        Indices = indices;
        Normals = normals ?? CreateDefaultNormals(vertices.Count);
    }

    public IReadOnlyList<Vector3> Vertices { get; }
    public IReadOnlyList<int> Indices { get; }
    public IReadOnlyList<Vector3> Normals { get; }

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
        var normals = new[]
        {
            Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector3.UnitY
        };
        return new Mesh(verts, tris, normals);
    }

    public static Mesh CreateCube(float size = 1f)
    {
        var h = size * 0.5f;
        var verts = new[]
        {
            // back
            new Vector3(-h, -h, -h),
            new Vector3(h, -h, -h),
            new Vector3(h, h, -h),
            new Vector3(-h, h, -h),
            // front
            new Vector3(-h, -h, h),
            new Vector3(h, -h, h),
            new Vector3(h, h, h),
            new Vector3(-h, h, h),
            // left
            new Vector3(-h, -h, -h),
            new Vector3(-h, h, -h),
            new Vector3(-h, h, h),
            new Vector3(-h, -h, h),
            // right
            new Vector3(h, -h, -h),
            new Vector3(h, h, -h),
            new Vector3(h, h, h),
            new Vector3(h, -h, h),
            // top
            new Vector3(-h, h, -h),
            new Vector3(h, h, -h),
            new Vector3(h, h, h),
            new Vector3(-h, h, h),
            // bottom
            new Vector3(-h, -h, -h),
            new Vector3(h, -h, -h),
            new Vector3(h, -h, h),
            new Vector3(-h, -h, h),
        };

        var normals = new[]
        {
            // back
            -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ, -Vector3.UnitZ,
            // front
            Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ,
            // left
            -Vector3.UnitX, -Vector3.UnitX, -Vector3.UnitX, -Vector3.UnitX,
            // right
            Vector3.UnitX, Vector3.UnitX, Vector3.UnitX, Vector3.UnitX,
            // top
            Vector3.UnitY, Vector3.UnitY, Vector3.UnitY, Vector3.UnitY,
            // bottom
            -Vector3.UnitY, -Vector3.UnitY, -Vector3.UnitY, -Vector3.UnitY,
        };

        var tris = new[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23
        };

        return new Mesh(verts, tris, normals);
    }

    private static IReadOnlyList<Vector3> CreateDefaultNormals(int count)
    {
        var normals = new Vector3[count];
        return normals;
    }
}

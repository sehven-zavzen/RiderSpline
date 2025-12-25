using System.Collections.Generic;
using UnityEngine;

public class ProceduralPath
{
    public readonly List<Vector3> points = new();

    public void SetPoints(List<Vector3> pts)
    {
        points.Clear();
        points.AddRange(pts);
    }

    // t: 0..1
    public Vector3 Evaluate(float t)
    {
        if (points.Count == 0) return Vector3.zero;
        if (points.Count == 1) return points[0];

        // Catmull-Rom needs 4 points; we clamp ends
        float scaled = t * (points.Count - 1);
        int i1 = Mathf.FloorToInt(scaled);
        int i2 = Mathf.Clamp(i1 + 1, 0, points.Count - 1);
        int i0 = Mathf.Clamp(i1 - 1, 0, points.Count - 1);
        int i3 = Mathf.Clamp(i1 + 2, 0, points.Count - 1);

        float u = scaled - i1;
        return CatmullRom(points[i0], points[i1], points[i2], points[i3], u);
    }

    public Vector3 EvaluateTangent(float t)
    {
        // Numerical derivative
        float dt = 0.001f;
        Vector3 p1 = Evaluate(Mathf.Clamp01(t));
        Vector3 p2 = Evaluate(Mathf.Clamp01(t + dt));
        return (p2 - p1).normalized;
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Standard Catmull-Rom
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
}

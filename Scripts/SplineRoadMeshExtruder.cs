using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineRoadMeshExtruder : MonoBehaviour
{
    [Header("Refs")]
    public RoadSplineGenerator generator;

    [Header("Road")]
    public float roadWidth = 8f;     // total width
    public float yOffset = -0.3f;     // zeminle çakışmasın
    public bool flipNormals = false;

    [Header("Sampling")]
    [Range(50, 5000)]
    public int segments = 800;       // arttır => daha pürüzsüz
    public float uvTiling = 1f;      // V boyunca tiling

    MeshFilter _mf;

    void Awake()
    {
        _mf = GetComponent<MeshFilter>();
    }

    IEnumerator Start()
    {
        // Path hazır olana kadar bekle
        float timeout = 2f;
        while (timeout > 0f &&
               (generator == null || generator.Path == null || generator.Path.points.Count < 2))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        Rebuild();
    }

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        if (generator == null || generator.Path == null || generator.Path.points.Count < 2)
        {
            Debug.LogWarning($"Rebuild skipped. Path points: {generator?.Path?.points?.Count ?? 0}");
            return;
        }

        var mesh = new Mesh();
        mesh.name = "RoadMesh_Runtime";

        int vertCount = (segments + 1) * 2;  // left+right per sample
        var verts = new Vector3[vertCount];
        var normals = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];

        // 2 triangle per segment => 6 index
        var tris = new int[segments * 6];

        float half = roadWidth * 0.5f;

        float vAcc = 0f;
        Vector3 prevCenter = generator.Path.Evaluate(1f);

        for (int i = 0; i <= segments; i++)
        {
            // uzak=1 -> yakın=0 (senin akış mantığına uyumlu)
            float t = 1f - (i / (float)segments);

            Vector3 center = generator.Path.Evaluate(t);
            center.y += yOffset;

            Vector3 tangent = generator.Path.EvaluateTangent(t);
            Vector3 forward = (-tangent).normalized; // player'a doğru baksın
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 leftPos = center - right * half;
            Vector3 rightPos = center + right * half;

            int vi = i * 2;
            verts[vi + 0] = leftPos;
            verts[vi + 1] = rightPos;

            Vector3 n = flipNormals ? Vector3.down : Vector3.up;
            normals[vi + 0] = n;
            normals[vi + 1] = n;

            if (i > 0)
                vAcc += Vector3.Distance(center, prevCenter);

            uvs[vi + 0] = new Vector2(0f, vAcc * uvTiling);
            uvs[vi + 1] = new Vector2(1f, vAcc * uvTiling);

            prevCenter = center;
        }

        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i * 2;
            int b = a + 1;
            int c = a + 2;
            int d = a + 3;

            if (!flipNormals)
            {
                // a-c-b, b-c-d
                tris[ti++] = a;
                tris[ti++] = c;
                tris[ti++] = b;

                tris[ti++] = b;
                tris[ti++] = c;
                tris[ti++] = d;
            }
            else
            {
                // ters winding
                tris[ti++] = a;
                tris[ti++] = b;
                tris[ti++] = c;

                tris[ti++] = b;
                tris[ti++] = d;
                tris[ti++] = c;
            }
        }

        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
        // mesh.RecalculateNormals(); // normals zaten up, istersen aç

        // ✅ Inspector’da None kalmasın diye güvenli atama
        _mf.sharedMesh = null;
        _mf.mesh = mesh;

        Debug.Log($"Rebuild OK. Path points: {generator.Path.points.Count}, verts: {mesh.vertexCount}, tris: {mesh.triangles.Length / 3}");
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RoadSplineGenerator : MonoBehaviour
{
    [Header("Length")]
    public int repeatSamples = 40;   // samples'ı kaç kere döndürelim (uzunluk)
    public float curveMultiplier = 2.5f;
    public float hillMultiplier = 2.0f;


    [Header("Input")]
    public string jsonFileName = "track.json"; // StreamingAssets

    [Header("Geometry / Feel")]
    public float stepDistance = 3.0f;          // her sample arası z ilerleme (metre)
    public float curveStrengthDeg = 6.0f;      // curve=1 iken sample başına kaç derece dönsün
    public float hillStrength = 4.0f;          // hill=1 iken metre cinsinden yükseklik hedefi
    public float hillSmoothing = 0.15f;        // 0..1 (yükseklik yumuşatma)
    public float maxHeadingDeg = 55f;          // aşırı dönmeyi sınırlamak için
    public float lateralDriftClamp = 35f;      // x sapması sınırı (istersen)

    [Header("Debug")]
    public bool drawGizmos = true;
    public int gizmoSamples = 200;

    public ProceduralPath Path { get; private set; } = new ProceduralPath();

    private TrackData _data;

    void Awake()
    {
        LoadAndBuild();
    }

    [ContextMenu("Load And Build")]
    public void LoadAndBuild()
    {
        string fullPath = PathCombineStreamingAssets(jsonFileName);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"JSON not found: {fullPath}");
            return;
        }

        _data = JsonUtility.FromJson<TrackData>(File.ReadAllText(fullPath));
        if (_data == null || _data.samples == null || _data.samples.Count < 2)
        {
            Debug.LogError("Invalid track data.");
            return;
        }

        var pts = BuildControlPoints(_data.samples);
        Path.SetPoints(pts);

        Debug.Log($"Built path with {pts.Count} control points from {_data.samples.Count} samples.");
    }

    private List<Vector3> BuildControlPoints(List<TrackSample> samples)
    {
        int total = samples.Count * Mathf.Max(1, repeatSamples);
        var pts = new List<Vector3>(total + 1);

        Vector3 pos = Vector3.zero;
        float headingDeg = 0f;
        float y = 0f;

        pts.Add(pos);

        for (int i = 0; i < total; i++)
        {
            var s = samples[i % samples.Count];

            float e = Mathf.Clamp01(s.energy);

            // ✅ Ekstremliği artır
            float curve = Mathf.Clamp(s.curve, -1f, 1f) * curveMultiplier * Mathf.Lerp(1f, 1.8f, e);
            float hill  = Mathf.Clamp(s.hill,  -1f, 1f) * hillMultiplier  * Mathf.Lerp(1f, 1.6f, e);

            headingDeg += curve * curveStrengthDeg;
            headingDeg = Mathf.Clamp(headingDeg, -maxHeadingDeg, maxHeadingDeg);

            float headingRad = headingDeg * Mathf.Deg2Rad;
            Vector3 forward = new Vector3(Mathf.Sin(headingRad), 0f, Mathf.Cos(headingRad));

            pos += forward * stepDistance;

            float targetY = hill * hillStrength;
            y = Mathf.Lerp(y, targetY, hillSmoothing);
            pos.y = y;

            pos.x = Mathf.Clamp(pos.x, -lateralDriftClamp, lateralDriftClamp);

            pts.Add(pos);
        }

        return pts;
    }


    private static string PathCombineStreamingAssets(string fileName)
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || Path == null || Path.points.Count < 2) return;

        Gizmos.color = Color.white;
        Vector3 prev = Path.Evaluate(0f);
        for (int i = 1; i <= gizmoSamples; i++)
        {
            float t = i / (float)gizmoSamples;
            Vector3 p = Path.Evaluate(t);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }

        // control points
        Gizmos.color = Color.yellow;
        if (Path.points != null)
        {
            foreach (var p in Path.points)
                Gizmos.DrawSphere(p, 0.25f);
        }
    }
}

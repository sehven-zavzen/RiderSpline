using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public float laneOffset;      // runtime’da toplam offset
    public float baseLaneOffset;  // note’un kendi lane offset’i (spawn’da set)

    public RoadSplineGenerator generator;
    public float speed = 12f;
    public bool faceForward = true;

    [Range(0f, 1f)]
    public float t = 1f; // spline'ın SONUNDAN başla
    
    void Update()
    {
        if (generator == null || generator.Path == null || generator.Path.points.Count < 2)
            return;

        // Dünya player’a doğru aksın
        t -= (speed * Time.deltaTime) /
            (generator.Path.points.Count * generator.stepDistance);

        if (t < 0f)
            t = 1f;

        // Spline pozisyonu
        Vector3 pos = generator.Path.Evaluate(t);

        // Spline yönleri
        Vector3 forward = -generator.Path.EvaluateTangent(t).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        // ✅ LANE OFFSET BURADA
        pos += right * laneOffset;
        
        transform.position = pos;

        if (faceForward && forward.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }

}

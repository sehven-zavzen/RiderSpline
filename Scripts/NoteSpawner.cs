using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Refs")]
    public RoadSplineGenerator generator;
    public PathFollower notePrefab;

    [Header("Spawn")]
    public float spawnEverySeconds = 0.35f;
    public float spawnT = 1f; // notelar uzak noktadan doğsun
    public float noteSpeed = 12f;

    [Header("Lanes")]
    public int laneCount = 3;
    public float laneWidth = 2.5f;

    public HitDetector hitDetector;
    float _timer;

    void Update()
    {
        if (generator == null || generator.Path == null || generator.Path.points.Count < 2) return;
        if (notePrefab == null) return;

        _timer += Time.deltaTime;
        if (_timer < spawnEverySeconds) return;
        _timer = 0f;

        SpawnOne();
    }

    void SpawnOne()
    {
        var note = Instantiate(notePrefab);

        // PathFollower ayarları
        note.generator = generator;
        note.speed = noteSpeed;
        note.t = spawnT;
        note.faceForward = false;

        // Lane seç (şimdilik random)
        int laneIndex = Random.Range(0, laneCount); // 0..laneCount-1
        int center = laneCount / 2;
        int offsetFromCenter = laneIndex - center;
        note.laneOffset = offsetFromCenter * laneWidth;

        // ✅ laneIndex’i note üstünde sakla
        var nb = note.GetComponent<NoteBehaviour>();
        if (nb != null)
            nb.laneIndex = laneIndex;

        // ✅ HitDetector’a kaydet
        hitDetector?.Register(note);

        // İlk frame’de doğru yerde dursun diye (opsiyonel)
        note.transform.position = generator.Path.Evaluate(spawnT);
    }

}

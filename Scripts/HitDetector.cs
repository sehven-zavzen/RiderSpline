using System.Collections.Generic;
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    public PlayerLaneController player;
    public Transform hitPoint;          // Player child
    public float hitDistance = 0.6f;    // metre
    public float missZBehind = -0.5f;   // hitPoint'i geçince miss (hitPoint local z referansı)

    private readonly List<PathFollower> _notes = new();

    public void Register(PathFollower noteFollower)
    {
        if (noteFollower != null && !_notes.Contains(noteFollower))
            _notes.Add(noteFollower);
    }

    void Update()
    {
        if (player == null || hitPoint == null) return;

        int playerLane = player.CurrentLaneIndex;

        for (int i = _notes.Count - 1; i >= 0; i--)
        {
            var pf = _notes[i];
            if (pf == null) { _notes.RemoveAt(i); continue; }

            var nb = pf.GetComponent<NoteBehaviour>();
            if (nb == null || nb.isHitOrMiss) { _notes.RemoveAt(i); continue; }

            // Note - hitPoint mesafesi
            float d = Vector3.Distance(pf.transform.position, hitPoint.position);

            // HitPoint'i geçip geçmedi? (hitPoint'in forward'ına göre)
            Vector3 toNote = pf.transform.position - hitPoint.position;
            float forwardDot = Vector3.Dot(hitPoint.forward, toNote); // + öndeyse, - arkadaysa

            // ✅ HIT: hitPoint civarında ve aynı lane
            if (d <= hitDistance && forwardDot >= -0.05f) // tam üstünden geçerken de yakalasın
            {
                if (nb.laneIndex == playerLane)
                {
                    nb.MarkHit();
                    _notes.RemoveAt(i);
                }
                // yanlış lane ise hiçbir şey yapma, geçsin
            }
            // ✅ MISS: hitPoint'i geçti (arkaya düştü)
            else if (forwardDot < missZBehind)
            {
                nb.MarkMiss();
                _notes.RemoveAt(i);
            }
        }
    }
}

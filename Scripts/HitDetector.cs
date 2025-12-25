using System.Collections.Generic;
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    public PlayerLaneController player;

    // Hit zamanı (player'a yakınlık)
    public float hitWindowTMin = 0.03f;
    public float hitWindowTMax = 0.08f;

    private readonly List<PathFollower> _notes = new();

    void Update()
    {
        if (player == null) return;

        CheckNotes();
    }

    // NoteSpawner her spawn’da çağırır
    public void Register(PathFollower noteFollower)
    {
        if (noteFollower != null && !_notes.Contains(noteFollower))
            _notes.Add(noteFollower);
    }

    void CheckNotes()
    {
        int playerLane = player.CurrentLaneIndex;

        for (int i = _notes.Count - 1; i >= 0; i--)
        {
            var pf = _notes[i];
            if (pf == null)
            {
                _notes.RemoveAt(i);
                continue;
            }

            var nb = pf.GetComponent<NoteBehaviour>();
            if (nb == null || nb.isHitOrMiss)
            {
                _notes.RemoveAt(i);
                continue;
            }

            float t = pf.t;

            // HIT WINDOW İÇİNDE
            if (t >= hitWindowTMin && t <= hitWindowTMax)
            {
                if (nb.laneIndex == playerLane)
                {
                    nb.MarkHit();
                }
                else
                {
                    nb.MarkMiss();
                }

                _notes.RemoveAt(i);
            }
            // Player'ı geçti → MISS
            else if (t < hitWindowTMin)
            {
                nb.MarkMiss();
                _notes.RemoveAt(i);
            }
        }
    }
}

using UnityEngine;

public class NoteBehaviour : MonoBehaviour
{
    public int laneIndex;          // 0..laneCount-1
    public bool isHitOrMiss;       // bir kez işlem görsün

    public void MarkHit()
    {
        isHitOrMiss = true;
        Debug.Log("HIT");
        Destroy(gameObject);
    }

    public void MarkMiss()
    {
        isHitOrMiss = true;
        Debug.Log("MISS");
        Destroy(gameObject);
    }
}

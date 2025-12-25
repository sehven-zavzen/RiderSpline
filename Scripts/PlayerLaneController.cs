using UnityEngine;

public class PlayerLaneController : MonoBehaviour
{
    [Header("Lane")]
    public int laneCount = 3;        // 3 lane
    public float laneWidth = 2.5f;
    public float laneChangeSpeed = 10f;

    private int _currentLane = 1;    // orta lane (0,1,2)
    private float _targetX;
    private float _currentX;
    public int CurrentLaneIndex => _currentLane;

    void Start()
    {
        _currentX = transform.localPosition.x;
        _targetX = _currentX;
    }

    void Update()
    {
        // INPUT
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            ChangeLane(-1);

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            ChangeLane(+1);

        // Yumuşak geçiş
        _currentX = Mathf.Lerp(_currentX, _targetX, Time.deltaTime * laneChangeSpeed);

        transform.localPosition = new Vector3(
            _currentX,
            transform.localPosition.y,
            transform.localPosition.z
        );
    }

    void ChangeLane(int dir)
    {
        _currentLane = Mathf.Clamp(_currentLane + dir, 0, laneCount - 1);

        // 0,1,2  →  -1,0,+1
        int center = laneCount / 2;
        int offsetFromCenter = _currentLane - center;

        _targetX = offsetFromCenter * laneWidth;
    }

    public float CurrentLaneOffset
    {
        get
        {
            int center = laneCount / 2;
            int offsetFromCenter = _currentLane - center;
            return offsetFromCenter * laneWidth;
        }
    }


}

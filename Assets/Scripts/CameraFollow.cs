using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // 플레이어 Transform
    public float yOffset = 2f; // 카메라가 플레이어보다 위쪽에 위치하도록 오프셋

    private float fixedX; // 고정할 X 좌표

    void Start()
    {
        fixedX = transform.position.x; // 시작할 때 X 위치 고정
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 newPos = transform.position;
        float targetY = player.position.y + yOffset;

        // 플레이어가 현재 카메라보다 아래에 있을 때만 따라감
        if (targetY < newPos.y)
        {
            newPos.y = targetY;
        }
        // 플레이어가 올라갈 때는 카메라 위치 유지 (추가로 점프할 때는 안 따라오게 됨)
        // 필요하면 카메라가 아래로만 움직이도록 제한하는 로직

        newPos.x = fixedX; // X축은 고정
        transform.position = newPos;
    }
}

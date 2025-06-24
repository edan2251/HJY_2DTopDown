using UnityEngine;
using TMPro;

public class PlayerMessageDisplay : MonoBehaviour
{
    public Transform playerTransform;        // 플레이어 Transform 넣을 곳
    public TextMeshProUGUI messageText;      // 캔버스 안의 TMP 텍스트

    public Vector3 offset = new Vector3(0, 2f, 0);  // 플레이어 머리 위 위치 조절

    private float displayTime = 2f;  // 메시지 표시 시간
    private float timer = 0f;

    void Update()
    {
        if (messageText.text != "")
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                messageText.text = "";
            }
            else
            {
                // 플레이어 위치 + offset을 스크린 좌표로 변환해 텍스트 위치에 반영
                Vector3 screenPos = Camera.main.WorldToScreenPoint(playerTransform.position + offset);
                messageText.transform.position = screenPos;
            }
        }
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        timer = displayTime;
    }
}

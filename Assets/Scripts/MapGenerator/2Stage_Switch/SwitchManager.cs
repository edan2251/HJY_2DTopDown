using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    public static SwitchManager Instance;

    private List<SwitchController> switches = new List<SwitchController>();

    [SerializeField] private Sprite bossDoorSprite;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterSwitch(SwitchController sw)
    {
        if (!switches.Contains(sw))
            switches.Add(sw);
    }

    public void CheckAllSwitches()
    {
        foreach (var sw in switches)
        {
            if (!sw.IsOn())
                return; // 하나라도 안 켜져있으면 리턴
        }

        Debug.Log("모든 스위치가 켜졌습니다! 완료!");

        // 보스방 문 열기
        Door bossDoor = FindBossDoor();
        if (bossDoor != null)
        {
            bossDoor.UnlockDoor();
        }
    }

    // 보스방 문 찾는 함수 예시 (씬에서 태그나 이름, 리스트 등으로 찾을 수 있음)
    private Door FindBossDoor()
    {
        Door[] doors = GameObject.FindObjectsOfType<Door>();
        foreach (var door in doors)
        {
            if (door.GetComponent<SpriteRenderer>().sprite == bossDoorSprite)
            {
                return door;
            }
        }
        return null;
    }
}

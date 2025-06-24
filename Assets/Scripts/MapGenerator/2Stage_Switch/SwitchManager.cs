using System.Collections.Generic;
using UnityEngine;

public class SwitchManager : MonoBehaviour
{
    public static SwitchManager Instance;

    private List<SwitchController> switches = new List<SwitchController>();

    public PlayerMessageDisplay messageDisplay;

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
        int onCount = 0;
        foreach (var sw in switches)
        {
            if (sw.IsOn()) onCount++;
        }

        string message = "";

        switch (onCount)
        {
            case 1:
                message = "이걸 4개 더 찾아야 한다니 . . .";
                break;
            case 2:
                message = "앞으로 3개 . . .";
                break;
            case 3:
                message = "이제 2개인가";
                break;
            case 4:
                message = "1개 남았군";
                break;
            case 5:
                message = "문쪽에서 무슨 소리가 난 것 같아 !";
                break;
        }

        if (messageDisplay != null && !string.IsNullOrEmpty(message))
        {
            messageDisplay.ShowMessage(message);
        }

        if (onCount < switches.Count) return;

        Debug.Log("모든 스위치가 켜졌습니다! 완료!");

        foreach (var door in FindBossDoors())
        {
            door.UnlockDoor();
        }
    }


    // 보스방 문 찾는 함수 예시 (씬에서 태그나 이름, 리스트 등으로 찾을 수 있음)
    private List<Door> FindBossDoors()
    {
        Door[] doors = GameObject.FindObjectsOfType<Door>();
        List<Door> result = new List<Door>();
        foreach (var door in doors)
        {
            if (door.isBossDoor)
                result.Add(door);
        }
        return result;
    }
}

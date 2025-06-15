using System.IO;
using UnityEngine;

public class VisitedRoomsResetter : MonoBehaviour
{
    string path;

    private void Awake()
    {
        path = Application.persistentDataPath + "/visitedRooms.json";
    }

    // 방문 기록 초기화 함수
    public void ResetVisitedRooms()
    {
        // 기존 데이터가 있으면 불러와서 floors 리스트는 유지하되 visitedRoomIDs를 비우기
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DungeonManager.DungeonVisitData saveData = JsonUtility.FromJson<DungeonManager.DungeonVisitData>(json);

            if (saveData != null)
            {
                foreach (var floorData in saveData.floors)
                {
                    floorData.visitedRoomIDs.Clear();
                }

                string updatedJson = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(path, updatedJson);
                Debug.Log("방문 기록 초기화 완료");
            }
            else
            {
                Debug.LogWarning("저장 데이터가 비어있음");
            }
        }
        else
        {
            Debug.LogWarning("방문 기록 파일이 존재하지 않음");
        }
    }
}
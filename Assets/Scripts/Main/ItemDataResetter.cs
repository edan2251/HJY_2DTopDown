using System.IO;
using UnityEngine;

public class ItemDataResetter : MonoBehaviour
{
    public void ResetItemData()
    {
        PlayerPrefs.SetInt("SpeedBoostCount", 0);
        PlayerPrefs.SetInt("ReviveCount", 0);
        PlayerPrefs.Save();

        Debug.Log("PlayerPrefs 아이템 데이터 초기화 완료");
    }

    //private string path;

    //private void Awake()
    //{
    //    path = Application.persistentDataPath + "/item_save.json";
    //}

    //public void ResetItemData()
    //{
    //    if (File.Exists(path))
    //    {
    //        // 0으로 초기화된 데이터 생성
    //        ItemManager.ItemSaveData resetData = new ItemManager.ItemSaveData
    //        {
    //            speedBoostCount = 0,
    //            reviveCount = 0
    //        };

    //        string updatedJson = JsonUtility.ToJson(resetData, true);
    //        File.WriteAllText(path, updatedJson);
    //        Debug.Log("아이템 저장 데이터 초기화 완료");
    //    }
    //    else
    //    {
    //        Debug.LogWarning("아이템 저장 파일이 존재하지 않음");
    //    }
    //}


}

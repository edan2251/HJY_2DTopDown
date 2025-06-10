using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTestManager : MonoBehaviour
{
    private static GameTestManager instance;

    public bool allMapVisibleMode;
    public bool DoNotMoveCameraMode;

    // 클리어횟수 저장
    public int clearCount = 0;


    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad( gameObject );
        }
        else
        {
            Destroy( gameObject );
        }
    }


    public static GameTestManager GetInstance()
    {
        return instance;
    }
}

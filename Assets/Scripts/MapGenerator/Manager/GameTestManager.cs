using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTestManager : MonoBehaviour
{
    private static GameTestManager instance;

    public bool allMapVisibleMode;
    public bool DoNotMoveCameraMode;

    // Ŭ����Ƚ�� ����
    public int clearCount = 0;


    private void Awake()
    {
        // �̱��� ���� ����
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

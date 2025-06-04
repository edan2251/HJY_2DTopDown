using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreItemObject : MonoBehaviour
{
    [SerializeField] ItemSO data;

    public int GetPoint()
    {
        return data.point;
    }

}

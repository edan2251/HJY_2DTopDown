using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private List<RoomSpawner> m_roomList = new List<RoomSpawner>();

    public FieldManager m_fieldManager;
    public GameObject m_fieldPrefabs;
    public GameObject m_startRoom;

    private int m_maxRoomCount = 14;

    public List<RoomSpawner> RoomList() { return m_roomList; }

    public void Update()
    {
        if (RoomGenerateComplete())
        {
            //Debug.Log("DONE" + m_roomList.Count);
            if (m_roomList.Count < m_maxRoomCount)
            {
                //Debug.Log("RESET");
                int count = this.transform.childCount;
                for (int i = 1; i < count; i++)
                {
                    Destroy(this.transform.GetChild(i).gameObject);
                }

                m_roomList.Clear();
                m_roomList = new List<RoomSpawner>();
                m_startRoom.GetComponent<RoomSpawner>().Start();
            }
            else
            {
                //if (m_complete == false)
                    SetRoomConnection();
            }
        }
    }

    public bool IsDuplicate(Vector2 pos)
    {
        bool isDup = false;
        for (int i = 0; i < m_roomList.Count; i++)
        {
            if (m_roomList[i].m_currentPos == pos)
            {
                isDup = true;
            }
        }

        return isDup;
    }


    public RoomSpawner FindIndex(Vector2 pos)
    {
        RoomSpawner spawner = null;
        for (int i = 0; i < m_roomList.Count; i++)
        {
            if (m_roomList[i].m_currentPos == pos)
            {
                spawner = m_roomList[i];
            }
        }

        return spawner;
    }

    public bool RoomGenerateComplete()
    {
        int completeCount = 0;
        if (m_roomList.Count <= 1)
            return false;

        for (int i = 0; i < m_roomList.Count; i++)
        {
            if (m_roomList[i].m_isRommGenerateComplete == true)
                completeCount++;
        }

        if (completeCount == m_roomList.Count)
            return true;
        else
            return false;
    }

    public bool IsOverRoomCount()
    {
        if (m_roomList.Count >= m_maxRoomCount)
            return true;
        else
            return false;
    }

    private void SetRoomConnection()
    {
        for (int i = 0; i < m_roomList.Count; i++)
        {
            if (FindIndex(m_roomList[i].m_currentPos + new Vector2(1, 0)) != null)
            {
                m_roomList[i].m_myRightIndex = 0;
            }
            if (FindIndex(m_roomList[i].m_currentPos + new Vector2(-1, 0)) != null)
            {
                m_roomList[i].m_myLeftIndex = 0;
            }
            if (FindIndex(m_roomList[i].m_currentPos + new Vector2(0, 1)) != null)
            {
                m_roomList[i].m_myBottomIndex = 0;
            }
            if (FindIndex(m_roomList[i].m_currentPos + new Vector2(0, -1)) != null)
            {
                m_roomList[i].m_myTopIndex = 0;
            }
        }
    }
}

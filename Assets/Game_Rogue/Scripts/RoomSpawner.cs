using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public Vector2 m_currentPos = new Vector2(0, 0);

    public int m_roomType = -1;
    public bool m_isRommGenerateComplete;
    public GameObject[] m_spawnPoint;

    private RoomTemplate m_template;
    private RoomManager m_roomManger;

    private int m_rand;

    public int m_myTopIndex = -1;
    public int m_myBottomIndex = -1;
    public int m_myLeftIndex = -1;
    public int m_myRightIndex = -1;

    public void Start()
    {
        m_template = GameObject.Find("RoomTemplate").GetComponent<RoomTemplate>();
        m_roomManger = GameObject.Find("RoomManager").GetComponent<RoomManager>();

        if (m_roomManger.IsDuplicate(m_currentPos) == true)
            Destroy(gameObject);

        m_roomManger.RoomList().Add(this);

        Invoke("SpawnRoom", 0.1f);
        SpawnRoom();
    }


    void SpawnRoom()
    {
        m_isRommGenerateComplete = false;

        for (int i = 0; i < m_spawnPoint.Length; i++)
        {
            GameObject obj = null;
            Vector2 addPos = Vector2.zero;

            if (m_spawnPoint[i].GetComponent<RoomType>().roomType == 0)
            {//right
                if (m_roomManger.IsOverRoomCount())
                    break;

                addPos = new Vector2(-1, 0) + this.m_currentPos;
                if (m_roomManger.IsDuplicate(addPos) == false)
                {
                    if (IsDirectRoomCountUpper(addPos))
                        break;

                    m_rand = Random.Range(0, m_template.m_rightRoom.Length);
                    obj = Instantiate(m_template.m_rightRoom[m_rand], m_spawnPoint[i].transform.position, Quaternion.identity);
                }
            }
            else if (m_spawnPoint[i].GetComponent<RoomType>().roomType == 1)
            {//left
                if (m_roomManger.IsOverRoomCount())
                    break;

                addPos = new Vector2(1, 0) + this.m_currentPos;
                if (m_roomManger.IsDuplicate(addPos) == false)
                {
                    if (IsDirectRoomCountUpper(addPos))
                        break;

                    m_rand = Random.Range(0, m_template.m_leftRooms.Length);
                    obj = Instantiate(m_template.m_leftRooms[m_rand], m_spawnPoint[i].transform.position, Quaternion.identity);
                }
            }
            else if (m_spawnPoint[i].GetComponent<RoomType>().roomType == 2)
            {//bottom
                if (m_roomManger.IsOverRoomCount())
                    break;

                addPos = new Vector2(0, -1) + this.m_currentPos;
                if (m_roomManger.IsDuplicate(addPos) == false)
                {
                    if (IsDirectRoomCountUpper(addPos))
                        break;

                    m_rand = Random.Range(0, m_template.m_bottomRooms.Length);
                    obj = Instantiate(m_template.m_bottomRooms[m_rand], m_spawnPoint[i].transform.position, Quaternion.identity);
                }
            }
            else if (m_spawnPoint[i].GetComponent<RoomType>().roomType == 3)
            {//top(위쪽방향인것들. 아래에 생김)
                if (m_roomManger.IsOverRoomCount())
                    break;

                addPos = new Vector2(0, 1) + this.m_currentPos;
                if (m_roomManger.IsDuplicate(addPos) == false)
                {
                    if (IsDirectRoomCountUpper(addPos))
                        break;

                    m_rand = Random.Range(0, m_template.m_topRooms.Length);

                    obj = Instantiate(m_template.m_topRooms[m_rand], m_spawnPoint[i].transform.position, Quaternion.identity);
                }
            }

            if (obj != null)
            {
                obj.transform.SetParent(m_roomManger.transform);
                obj.GetComponent<RoomSpawner>().m_currentPos = addPos;
            }
        }

        m_isRommGenerateComplete = true;
    }

    private bool IsDirectRoomCountUpper(Vector2 pos)
    {
        int directRoomCount = 0;

        if (m_roomManger.FindIndex(pos + new Vector2(1, 0)) != null)
        {
            directRoomCount++;
        }
        if (m_roomManger.FindIndex(pos + new Vector2(-1, 0)) != null)
        {
            directRoomCount++;
        }
        if (m_roomManger.FindIndex(pos + new Vector2(0, 1)) != null)
        {
            directRoomCount++;
        }
        if (m_roomManger.FindIndex(pos + new Vector2(0, -1)) != null)
        {
            directRoomCount++;
        }

        if (directRoomCount >= 2)
            return true;
        else
            return false;
    }
}

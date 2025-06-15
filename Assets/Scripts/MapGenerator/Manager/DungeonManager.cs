using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class DungeonManager : MonoBehaviour
{

    public struct FTileInfoByCellID
    {
        public Tilemap tilemap;
        public Vector3Int pos;

        public FTileInfoByCellID(Tilemap _tilemap, Vector3Int _pos)
        {
            tilemap = _tilemap;
            pos = _pos;
        }
    };

    private static DungeonManager instance;

    public int enemyCount;
    public int playerRoomID;
    public int difficulty;
    public int cellSize;
    public int tileNumPerCell;
    public Player player;
    public Camera mainCamera;
    public Camera minimapCamera;
    public Cell[,] cellList;
    public Tilemap tilemap;
    public Dictionary<int, List<FTileInfoByCellID>> tilemapDic;
    public Dictionary<int, List<Door>> doorDic;
    public Dictionary<int, HashSet<Cell>> sameRoomDic;  // id, 해당 id의 cell들
    public Dictionary<int, HashSet<Cell>> adjacentCellDic;  // id, 해당 id와 인접한 cell들
    public HashSet<int> isRoomVisited;

    [System.Serializable]
    public class DungeonVisitData
    {
        public List<FloorVisitData> floors = new List<FloorVisitData>();
    }

    [System.Serializable]
    public class FloorVisitData
    {
        public int floor;              // clearCount 값
        public List<int> visitedRoomIDs = new List<int>();  // 방문한 방 ID 리스트
    }

    public void SaveVisitedRoomsToJSON()
    {
        string path = Application.persistentDataPath + "/visitedRooms.json";

        DungeonVisitData saveData;

        // 기존 파일이 있으면 로드
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            saveData = JsonUtility.FromJson<DungeonVisitData>(json);
        }
        else
        {
            saveData = new DungeonVisitData();
        }

        // 현재 층의 데이터가 이미 있는지 확인
        FloorVisitData currentFloorData = saveData.floors.Find(f => f.floor == GameTestManager.GetInstance().clearCount);

        if (currentFloorData != null)
        {
            // 중복 방지하고 추가
            foreach (int id in isRoomVisited)
            {
                if (!currentFloorData.visitedRoomIDs.Contains(id))
                    currentFloorData.visitedRoomIDs.Add(id);
            }
        }
        else
        {
            // 새 층 데이터 추가
            currentFloorData = new FloorVisitData
            {
                floor = GameTestManager.GetInstance().clearCount,
                visitedRoomIDs = new List<int>(isRoomVisited)
            };
            saveData.floors.Add(currentFloorData);
        }

        // 저장
        string updatedJson = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, updatedJson);

        Debug.Log("방문 정보 저장됨: " + path);
    }

    public void LoadVisitedRoomsFromJSON()
    {
        string path = Application.persistentDataPath + "/visitedRooms.json";

        if (!File.Exists(path))
        {
            Debug.LogWarning("방문 기록 파일이 없음");
            return;
        }

        string json = File.ReadAllText(path);
        DungeonVisitData saveData = JsonUtility.FromJson<DungeonVisitData>(json);

        int currentFloor = GameTestManager.GetInstance().clearCount; // 현재 층 정보 가져오기

        FloorVisitData currentFloorData = saveData.floors.Find(f => f.floor == currentFloor);

        if (currentFloorData == null)
        {
            Debug.Log("현재 층 방문 기록 없음");
            return;
        }

        foreach (int roomID in currentFloorData.visitedRoomIDs)
        {
            // 방문한 방들은 미니맵에서 어둡게 표시
            ActivateMinimap(roomID, false);
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        sameRoomDic = new Dictionary<int, HashSet<Cell>>();
        tilemapDic = new Dictionary<int, List<FTileInfoByCellID>>();
        doorDic = new Dictionary<int, List<Door>>();
        adjacentCellDic = new Dictionary<int, HashSet<Cell>>();
        isRoomVisited = new HashSet<int>();
    }

    public static DungeonManager GetInstance()
    {
        return instance;
    }

    public void AddToSameRoomDic(Cell cell)
    {
        if (sameRoomDic.ContainsKey(cell.id))
        {
            sameRoomDic[cell.id].Add(cell);
        }
        else
        {
            sameRoomDic.Add(cell.id, new HashSet<Cell>() { cell });
        }
    }

    public void SetPlayerRoomID(int id)
    {
        playerRoomID = id;

        // 방문 기록에 현재 방 추가
        isRoomVisited.Add(id);

        // 방문 정보 저장
        SaveVisitedRoomsToJSON();
    }

    public void SetPlayerPos(Vector3Int pos)
    {
        player.transform.position = tilemap.CellToWorld(pos);
    }

    public void SetMainCameraPos()
    {
        Vector3 pos = new Vector3(0, 0, 0);

        if (!sameRoomDic.ContainsKey(playerRoomID))
            return;

        foreach (Cell cell in sameRoomDic[playerRoomID])
        {
            pos += cell.transform.position;
        }

        pos /= sameRoomDic[playerRoomID].Count;
        mainCamera.transform.position = new Vector3(pos.x, pos.y, -10);
        minimapCamera.transform.position = new Vector3(pos.x, pos.y, -10);
    }

    public void SetPlayerTransform(Vector2 pos, float size)
    {
        player.transform.position = pos;
        player.transform.localScale = new Vector3(size, size, 0);
    }

    public void AddToTilemapDic(int id, Tilemap tilemapType, Vector3Int pos)
    {
        FTileInfoByCellID tileInfo = new FTileInfoByCellID(tilemapType, pos);
        if (tilemapDic.ContainsKey(id))
        {
            tilemapDic[id].Add(tileInfo);
        }
        else
        {
            tilemapDic.Add(id, new List<FTileInfoByCellID>() { tileInfo });
        }
    }

    public void AddToDoorDic(int id, Door door)
    {
        if (doorDic.ContainsKey(id))
        {
            doorDic[id].Add(door);
        }
        else
        {
            doorDic.Add(id, new List<Door>() { door });
        }
    }

    public void SetVisibilityTiles(int id, bool isVisible)
    {
        if (tilemapDic.ContainsKey(id))
        {
            foreach (FTileInfoByCellID tileInfo in tilemapDic[id])
            {
                Tilemap tilemapType = tileInfo.tilemap;
                Vector3Int pos = tileInfo.pos;

                Color color = tilemapType.GetColor(pos);
                if (isVisible)
                {
                    color.a = 1;
                }
                else
                {
                    color.a = 0;
                }
                tilemapType.SetColor(pos, color);
            }

            foreach (Door door in doorDic[id])
            {
                door.SetVisibility(isVisible);
                if (isVisible)
                {
                    door.GetComponent<BoxCollider2D>().isTrigger = true;
                }
                else
                {
                    door.GetComponent<BoxCollider2D>().isTrigger = false;
                }
            }
        }
    }

    public void ActivateMinimap(int id, bool isActivate)
    {
        foreach (Cell cell in sameRoomDic[id])
        {
            SpriteRenderer minimapRenderer = cell.transform.Find("minimapSprite").GetComponent<SpriteRenderer>();
            cell.isVisited = true;
            if (isActivate)
            {
                minimapRenderer.color = cell.activeColor;
            }
            else
            {
                minimapRenderer.color = cell.deactiveColor;
            }
        }
    }

    public void SetVisibilityMinimap(int id, bool isActivate)
    {
        foreach (Cell cell in sameRoomDic[id])
        {
            SpriteRenderer minimapRenderer = cell.transform.Find("minimapSprite").GetComponent<SpriteRenderer>();
            if (isActivate)
            {
                Color color = minimapRenderer.color;
                color.a = 1;
                minimapRenderer.color = color;
            }
            else
            {
                Color color = minimapRenderer.color;
                color.a = 0;
                minimapRenderer.color = color;
            }
        }
    }

    public bool IsCellAdjacent(Cell prevCell, Cell postCell)
    {
        return adjacentCellDic.ContainsKey(prevCell.id) && adjacentCellDic[prevCell.id].Contains(postCell);
    }

    public void AddAdjacentID(Cell prevCell, Cell postCell)
    {
        if (adjacentCellDic.ContainsKey(prevCell.id))
        {
            adjacentCellDic[prevCell.id].Add(postCell);
        }
        else
        {
            adjacentCellDic.Add(prevCell.id, new HashSet<Cell>() { postCell });
        }
        if (adjacentCellDic.ContainsKey(postCell.id))
        {
            adjacentCellDic[postCell.id].Add(prevCell);
        }
        else
        {
            adjacentCellDic.Add(postCell.id, new HashSet<Cell>() { prevCell });
        }
    }
}

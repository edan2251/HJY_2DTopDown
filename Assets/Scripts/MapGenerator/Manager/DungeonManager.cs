
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class DungeonManager : MonoBehaviour
{
    public List<int> shortestPath = new List<int>();

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

    public Vector3Int playerSpawnCellPos;

    public GameObject startHintText;
    public TextMeshProUGUI countdownText;

    //public int enemyCount;
    public int playerRoomID = 1;    // 플레이어 방도 1로 초기화
    //public int difficulty;
    public int cellSize;
    public int tileNumPerCell;
    public Player player;
    public Camera mainCamera;
    public Camera miniMapCamera;
    public Camera fullMapCamera;
    public GameObject minimapUI;   // Canvas 아래 Minimap 오브젝트
    public GameObject fullmapUI;   // Canvas 아래 Fullmap 오브젝트

    public Cell[,] cellList;
    public Tilemap tilemap;
    public Dictionary<int, List<FTileInfoByCellID>> tilemapDic;
    public Dictionary<int, List<Door>> doorDic;
    public Dictionary<int, HashSet<Cell>> sameRoomDic;  // id, 해당 id의 cell들
    public Dictionary<int, HashSet<Cell>> adjacentCellDic;  // id, 해당 id와 인접한 cell들
    public HashSet<int> isRoomVisited;

    private Vector3 targetCameraPos;    // 카메라가 이동할 목표 위치
    private bool isCameraMoving = false;  // 카메라가 이동 중인지 상태 체크
    public float cameraMoveSpeed = 7f;   // 카메라 이동 속도 조절용

    private int previousRoomID = -1;  // 초기값 -1

    private Coroutine fadeOutCoroutine;

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

    public bool isPausedByMap = false;
    public void UpdateMapUI()
    {
        if (GameTestManager.GetInstance().clearCount == 3)
        {
            fullmapUI.SetActive(true);
            minimapUI.SetActive(false);

            fullMapCamera.gameObject.SetActive(true);
            miniMapCamera.gameObject.SetActive(false);
            Time.timeScale = 0f;
            isPausedByMap = true;
            StartCoroutine(ShowCountdown());
        }
        else
        {
            fullmapUI.SetActive(false);
            minimapUI.SetActive(true);

            fullMapCamera.gameObject.SetActive(false);
            miniMapCamera.gameObject.SetActive(true);
        }
    }

    public void ResumeFromMap()
    {
        fullmapUI.SetActive(false);
        Time.timeScale = 1f;
        isPausedByMap = false;
    }

    private IEnumerator ShowCountdown()
    {
        if (countdownText == null) yield break;

        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSecondsRealtime(1f);
            count--;
        }

        countdownText.gameObject.SetActive(false);

        if (startHintText != null)
            startHintText.SetActive(true);
    }

    public void SetFullMapCameraBounds()
    {
        if (fullMapCamera == null) return;

        Bounds totalBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool first = true;

        foreach (var room in sameRoomDic.Values)
        {
            foreach (var cell in room)
            {
                Vector3 pos = cell.transform.position;

                if (first)
                {
                    totalBounds = new Bounds(pos, Vector3.zero);
                    first = false;
                }
                else
                {
                    totalBounds.Encapsulate(pos);
                }
            }
        }

        Vector3 center = totalBounds.center;
        Vector3 size = totalBounds.size;

        fullMapCamera.transform.position = new Vector3(center.x, center.y, -10f);

        float screenAspect = fullMapCamera.aspect;
        float sizeX = size.x / screenAspect;
        float sizeY = size.y;

        fullMapCamera.orthographicSize = Mathf.Max(sizeX, sizeY) * 0.55f;
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

        int currentFloor = GameTestManager.GetInstance().clearCount;

        // 현재 층의 데이터가 이미 있는지 확인
        FloorVisitData currentFloorData = saveData.floors.Find(f => f.floor == currentFloor);

        if (currentFloorData != null)
        {
            // 중복 방지하고 추가 (ID 16 제외)
            foreach (int id in isRoomVisited)
            {
                if (id != 16 && !currentFloorData.visitedRoomIDs.Contains(id))
                    currentFloorData.visitedRoomIDs.Add(id);
            }
        }
        else
        {
            // 새 층 데이터 추가 (ID 16 제외)
            List<int> filteredRoomIDs = new List<int>();
            foreach (int id in isRoomVisited)
            {
                if (id != 16)
                    filteredRoomIDs.Add(id);
            }

            currentFloorData = new FloorVisitData
            {
                floor = currentFloor,
                visitedRoomIDs = filteredRoomIDs
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

    //카메라 부드럽게 따라가기
    public void SetMainCameraPosSmooth()
    {
        if (!sameRoomDic.ContainsKey(playerRoomID))
            return;

        Vector3 pos = Vector3.zero;
        foreach (Cell cell in sameRoomDic[playerRoomID])
        {
            pos += cell.transform.position;
        }
        pos /= sameRoomDic[playerRoomID].Count;

        targetCameraPos = new Vector3(pos.x, pos.y, -10);
        isCameraMoving = true;
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

    public void SetCameraToMapCenter()
    {
        int targetRoomID = 1;  // 1번 방으로 고정
        if (!sameRoomDic.ContainsKey(targetRoomID))
        {
            Debug.LogWarning("sameRoomDic에 1번 방이 없습니다.");
            return;
        }

        Vector3 totalPos = Vector3.zero;
        var cellListInRoom = sameRoomDic[targetRoomID];

        foreach (var cell in cellListInRoom)
        {
            totalPos += cell.transform.position;
        }

        Vector3 center = totalPos / cellListInRoom.Count;

        mainCamera.transform.position = new Vector3(center.x, center.y, -10);
        miniMapCamera.transform.position = new Vector3(center.x, center.y, -10);
    }


    private Vector3 velocity = Vector3.zero; // 카메라 이동 속도 저장용

    private void Update()
    {
        if (isCameraMoving)
        {
            // SmoothDamp를 사용한 부드러운 카메라 이동
            mainCamera.transform.position = Vector3.SmoothDamp(
                mainCamera.transform.position,
                targetCameraPos,
                ref velocity,
                0.3f // 감속 시간 (작을수록 빠르게 붙음, 0.2~0.5 추천)
            );

            miniMapCamera.transform.position = mainCamera.transform.position;

            // 목표 위치에 충분히 가까우면 이동 종료
            if (Vector3.Distance(mainCamera.transform.position, targetCameraPos) < 0.05f)
            {
                mainCamera.transform.position = targetCameraPos; // 정확히 맞춰 붙여줌
                isCameraMoving = false;
                velocity = Vector3.zero; // 속도 초기화
            }
        }
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

    public void SetPlayerRoomID(int newRoomID)
    {
        // 강제 이동 기능은 clearCount == 3일 때만 동작
        if (GameTestManager.GetInstance().clearCount == 3)
        {
            // 예외 방지
            if (shortestPath == null || shortestPath.Count == 0)
            {
                Debug.LogWarning("최단 경로가 설정되어 있지 않습니다.");
            }
            else
            {
                if (!shortestPath.Contains(newRoomID))
                {
                    Debug.Log("최단 경로가 아닌 방 입장 시도! 1번 방으로 강제 이동!");
                    newRoomID = 1;

                    MapGeneratorIssac mapGen = FindObjectOfType<MapGeneratorIssac>();
                    if (mapGen != null)
                    {
                        mapGen.InitPlayer();
                    }
                }
            }
        }

        // 기존 로직 유지
        if (playerRoomID != newRoomID)
        {
            previousRoomID = playerRoomID;
            playerRoomID = newRoomID;

            isRoomVisited.Add(newRoomID);

            foreach (var id in tilemapDic.Keys)
            {
                if (id == playerRoomID || id == previousRoomID)
                    SetVisibilityTiles(id, true);
                else
                    SetVisibilityTiles(id, false);
            }

            if (previousRoomID != -1)
            {
                FadeOutRoom(previousRoomID);
            }
        }

        SetSwitchVisibility(playerRoomID);
        SetItemBoxVisibility(playerRoomID);
        SaveVisitedRoomsToJSON();
    }

    public void SetSwitchVisibility(int activeRoomID)
    {
        foreach (var sw in FindObjectsOfType<SwitchController>())
        {
            if (sw.roomID == activeRoomID)
            {
                sw.SetVisibility(true);
            }
            else
            {
                sw.SetVisibility(false);
            }
        }
    }

    public void SetItemBoxVisibility(int activeRoomID)
    {
        foreach (var box in FindObjectsOfType<ItemBox>())
        {
            if (box.roomID == activeRoomID)
            {
                box.SetVisibility(true);
            }
            else
            {
                box.SetVisibility(false);
            }
        }
    }

    public void SetPlayerPos(Vector3Int pos)
    {
        player.transform.position = tilemap.CellToWorld(pos);
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
                color.a = isVisible ? 1f : 0f;
                tilemapType.SetColor(pos, color);
            }

            if (doorDic.ContainsKey(id))
            {
                foreach (Door door in doorDic[id])
                {
                    door.SetVisibility(isVisible);
                    door.GetComponent<BoxCollider2D>().isTrigger = isVisible;
                }
            }
        }
    }

    public void ActivateMinimap(int id, bool isActivate)
    {
        if (!sameRoomDic.ContainsKey(id))
        {
            Debug.LogWarning($"[Minimap] 존재하지 않는 Room ID {id} 접근 시도");
            return; // 없으면 무시
        }
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

    public void HighlightBossRoomOnMinimap(int id)
    {
        if (!sameRoomDic.ContainsKey(id)) return;

        foreach (Cell cell in sameRoomDic[id])
        {
            cell.activeColor = new Color(1f, 0.3f, 0f); // 주황색 계열로 미리 지정
            SpriteRenderer minimapRenderer = cell.transform.Find("minimapSprite").GetComponent<SpriteRenderer>();
            minimapRenderer.color = cell.activeColor;
        }
    }

    public void ShowShortestPathOnMinimap()
    {
        if (shortestPath == null || shortestPath.Count == 0)
            return;

        // 전체 방 순회 (sameRoomDic의 키가 방 ID)
        foreach (int id in sameRoomDic.Keys)
        {
            bool isOnShortestPath = shortestPath.Contains(id);
            ActivateMinimap(id, isOnShortestPath);
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

    public void FadeOutRoom(int roomID, float duration = 0.8f) //페이드아웃 속도 조절 줄일수록 빠름
    {
        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);

        fadeOutCoroutine = StartCoroutine(FadeOutTiles(roomID, duration));
    }

    private IEnumerator FadeOutTiles(int roomID, float duration)
    {
        if (doorDic.ContainsKey(roomID))
        {
            foreach (var door in doorDic[roomID])
            {
                door.SetVisibility(false);
                door.GetComponent<BoxCollider2D>().isTrigger = false;
            }
        }

        if (!tilemapDic.ContainsKey(roomID))
            yield break;

        float time = 0f;
        Dictionary<FTileInfoByCellID, Color> initialColors = new Dictionary<FTileInfoByCellID, Color>();

        // 초기 색상 저장
        foreach (var tileInfo in tilemapDic[roomID])
        {
            Color original = tileInfo.tilemap.GetColor(tileInfo.pos);
            initialColors[tileInfo] = original;
        }

        while (time < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            foreach (var tileInfo in tilemapDic[roomID])
            {
                Color color = initialColors[tileInfo];
                color.a = alpha;
                tileInfo.tilemap.SetColor(tileInfo.pos, color);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // 완전히 투명하게 설정
        foreach (var tileInfo in tilemapDic[roomID])
        {
            Color color = initialColors[tileInfo];
            color.a = 0f;
            tileInfo.tilemap.SetColor(tileInfo.pos, color);
        }
    }

    //최단거리 구하기
    public List<int> GetShortestPathFromStartToBoss()
    {
        int start = 1;
        int goal = 16;

        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        Queue<int> queue = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (current == goal)
                break;

            if (!adjacentCellDic.ContainsKey(current))
                continue;

            foreach (var neighborCell in adjacentCellDic[current])
            {
                int neighborID = neighborCell.id;
                if (!visited.Contains(neighborID))
                {
                    visited.Add(neighborID);
                    cameFrom[neighborID] = current;
                    queue.Enqueue(neighborID);
                }
            }
        }

        // 경로 추적
        List<int> path = new List<int>();
        int cur = goal;
        while (cur != start)
        {
            path.Add(cur);
            if (!cameFrom.ContainsKey(cur))
            {
                Debug.LogWarning("최단 경로 없음");
                return new List<int>();
            }
            cur = cameFrom[cur];
        }
        path.Add(start);
        path.Reverse();

        // 디버그 출력
        string pathLog = "최단 경로: ";
        foreach (int id in path)
        {
            pathLog += id + " -> ";
        }
        pathLog = pathLog.TrimEnd('-', '>', ' ');
        Debug.Log(pathLog);

        return path;


    }

}

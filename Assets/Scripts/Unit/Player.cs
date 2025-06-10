using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement; // 씬 변경을 위한 네임스페이스 추가
using UnityEngine.UI; // UI 관리용 네임스페이스 추가
using TMPro; // TextMeshPro 사용 가능

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    private PlayerController controller;
    private SpriteRenderer spriteRenderer;
    //private Rigidbody2D rigid;

    // 이동 방향별 스프라이트
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase bossGroundTile;

    [SerializeField] private bool isOnBossTile = false; // 보스 타일 위에 있는지 체크

    [SerializeField] private TextMeshProUGUI clearCountText; // UI에 클리어 횟수 표시



    void Start()
    {
        UpdateClearCountUI();

        controller = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 보스방 타일 정보 설정
        MapGeneratorIssac generator = FindObjectOfType<MapGeneratorIssac>();
        if (generator != null)
        {
            groundTilemap = generator.groundTilemap;
            bossGroundTile = generator.bossGroundTile;
        }
    }

    void Update()
    {
        UpdateClearCountUI();

        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 moveVelocity = moveInput.normalized * speed;
        controller.Move(moveVelocity);

        // 이동 방향에 따라 스프라이트 변경
        UpdateSprite(moveInput);

        // 보스방 타일 감지
        if (groundTilemap != null && bossGroundTile != null)
        {
            Vector3Int tilePos = groundTilemap.WorldToCell(transform.position);
            TileBase currentTile = groundTilemap.GetTile(tilePos);

            if (currentTile == bossGroundTile)
            {
                if (!isOnBossTile) // 처음 밟았을 때만 실행
                {
                    isOnBossTile = true;
                    StartCoroutine(BossTileStay()); // 2초 동안 머물러야 클리어 증가
                }
            }
            else
            {
                isOnBossTile = false; // 보스 타일에서 벗어나면 상태 초기화
            }
        }


        // 치트키 활성화 (C 키 입력 시 클리어 횟수 증가)
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameTestManager.GetInstance().clearCount++;
            Debug.Log($"치트키 사용! 현재 클리어 횟수: {GameTestManager.GetInstance().clearCount}");
        }
    }

    private void UpdateClearCountUI()
    {
        clearCountText.text = $"{GameTestManager.GetInstance().clearCount} / 3";
    }


    IEnumerator BossTileStay()
    {
        yield return new WaitForSeconds(2f); // 2초 대기

        // 2초 후 클리어 횟수 증가
        GameTestManager.GetInstance().clearCount++;
        Debug.Log($"보스 타일 도달! 현재 클리어 횟수: {GameTestManager.GetInstance().clearCount}/3");
        SceneManager.LoadScene("Dungeon");

        if (GameTestManager.GetInstance().clearCount >= 4) // 4번째에 게임 클리어
        {
            Debug.Log("게임 클리어! Test_Main 씬으로 이동합니다.");
            GameTestManager.GetInstance().clearCount = 0;
            SceneManager.LoadScene("Test_Main");
        }

        isOnBossTile = false; // 클리어 후 다시 초기화
    }


    void UpdateSprite(Vector2 direction)
    {
        if (direction.y > 0) // 위로 이동
            spriteRenderer.sprite = upSprite;
        else if (direction.y < 0) // 아래로 이동
            spriteRenderer.sprite = downSprite;
        else if (direction.x > 0) // 오른쪽 이동
            spriteRenderer.sprite = rightSprite;
        else if (direction.x < 0) // 왼쪽 이동
            spriteRenderer.sprite = leftSprite;
    }

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }
}
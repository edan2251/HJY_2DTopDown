using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    public static Player Instance;

    public float BaseSpeed = 10f;
    public float FixSpeed;
    [SerializeField] private float speed = 5f;
    public float moveSpeed
    {
        get => speed;
        set => speed = value;
    }

    private PlayerController controller;
    private SpriteRenderer spriteRenderer;

    // 이동 방향별 스프라이트
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase bossGroundTile;

    [SerializeField] private bool isOnBossTile = false; // 보스 타일 위에 있는지 체크
    [SerializeField] private TextMeshProUGUI clearCountText; // UI에 클리어 횟수 표시

    private float pauseStartTime = -1f; // minimap 정지 시작 시각
    private float pauseDelay = 3f;      // 대기 시간 (초)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        DungeonManager mapManager = FindObjectOfType<DungeonManager>();

        // minimap 때문에 멈춰있는 경우
        if (mapManager != null && mapManager.isPausedByMap)
        {
            if (pauseStartTime < 0f)
            {
                pauseStartTime = Time.unscaledTime;
            }

            float elapsed = Time.unscaledTime - pauseStartTime;

            if (elapsed >= pauseDelay)
            {
                if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                {
                    mapManager.ResumeFromMap();
                    pauseStartTime = -1f;
                    Debug.Log("3초 대기 후 이동 감지 → 게임 재개");
                }
            }
            return;
        }
        else
        {
            pauseStartTime = -1f;
        }

        UpdateClearCountUI();

        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (GameTestManager.GetInstance().clearCount == 1)
        {
            moveInput *= -1;
        }

        Vector2 moveVelocity = moveInput.normalized * speed;
        controller.Move(moveVelocity);

        UpdateSprite(moveInput);

        // 보스방 타일 감지
        if (groundTilemap != null && bossGroundTile != null)
        {
            Vector3Int tilePos = groundTilemap.WorldToCell(transform.position);
            TileBase currentTile = groundTilemap.GetTile(tilePos);

            if (currentTile == bossGroundTile)
            {
                if (!isOnBossTile)
                {
                    isOnBossTile = true;
                    StartCoroutine(BossTileStay());
                }
            }
            else
            {
                isOnBossTile = false;
            }
        }
    }

    private void UpdateClearCountUI()
    {
        clearCountText.text = $"{GameTestManager.GetInstance().clearCount} / 3";
    }

    IEnumerator BossTileStay()
    {
        yield return new WaitForSeconds(2f);

        GameTestManager.GetInstance().clearCount++;
        Debug.Log($"보스 타일 도달! 현재 클리어 횟수: {GameTestManager.GetInstance().clearCount}/2");
        SceneManager.LoadScene("Dungeon");

        if (GameTestManager.GetInstance().clearCount >= 4)
        {
            Debug.Log("게임 클리어! Test_Main 씬으로 이동합니다.");
            GameTestManager.GetInstance().clearCount = 0;
            SceneManager.LoadScene("Test_Main");
        }

        isOnBossTile = false;
    }

    void UpdateSprite(Vector2 direction)
    {
        if (direction.y > 0)
            spriteRenderer.sprite = upSprite;
        else if (direction.y < 0)
            spriteRenderer.sprite = downSprite;
        else if (direction.x > 0)
            spriteRenderer.sprite = rightSprite;
        else if (direction.x < 0)
            spriteRenderer.sprite = leftSprite;
    }
}

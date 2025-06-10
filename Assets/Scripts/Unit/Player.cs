using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement; // �� ������ ���� ���ӽ����̽� �߰�
using UnityEngine.UI; // UI ������ ���ӽ����̽� �߰�
using TMPro; // TextMeshPro ��� ����

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    private PlayerController controller;
    private SpriteRenderer spriteRenderer;
    //private Rigidbody2D rigid;

    // �̵� ���⺰ ��������Ʈ
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase bossGroundTile;

    [SerializeField] private bool isOnBossTile = false; // ���� Ÿ�� ���� �ִ��� üũ

    [SerializeField] private TextMeshProUGUI clearCountText; // UI�� Ŭ���� Ƚ�� ǥ��



    void Start()
    {
        UpdateClearCountUI();

        controller = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ������ Ÿ�� ���� ����
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

        // �̵� ���⿡ ���� ��������Ʈ ����
        UpdateSprite(moveInput);

        // ������ Ÿ�� ����
        if (groundTilemap != null && bossGroundTile != null)
        {
            Vector3Int tilePos = groundTilemap.WorldToCell(transform.position);
            TileBase currentTile = groundTilemap.GetTile(tilePos);

            if (currentTile == bossGroundTile)
            {
                if (!isOnBossTile) // ó�� ����� ���� ����
                {
                    isOnBossTile = true;
                    StartCoroutine(BossTileStay()); // 2�� ���� �ӹ����� Ŭ���� ����
                }
            }
            else
            {
                isOnBossTile = false; // ���� Ÿ�Ͽ��� ����� ���� �ʱ�ȭ
            }
        }


        // ġƮŰ Ȱ��ȭ (C Ű �Է� �� Ŭ���� Ƚ�� ����)
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameTestManager.GetInstance().clearCount++;
            Debug.Log($"ġƮŰ ���! ���� Ŭ���� Ƚ��: {GameTestManager.GetInstance().clearCount}");
        }
    }

    private void UpdateClearCountUI()
    {
        clearCountText.text = $"{GameTestManager.GetInstance().clearCount} / 3";
    }


    IEnumerator BossTileStay()
    {
        yield return new WaitForSeconds(2f); // 2�� ���

        // 2�� �� Ŭ���� Ƚ�� ����
        GameTestManager.GetInstance().clearCount++;
        Debug.Log($"���� Ÿ�� ����! ���� Ŭ���� Ƚ��: {GameTestManager.GetInstance().clearCount}/3");
        SceneManager.LoadScene("Dungeon");

        if (GameTestManager.GetInstance().clearCount >= 4) // 4��°�� ���� Ŭ����
        {
            Debug.Log("���� Ŭ����! Test_Main ������ �̵��մϴ�.");
            GameTestManager.GetInstance().clearCount = 0;
            SceneManager.LoadScene("Test_Main");
        }

        isOnBossTile = false; // Ŭ���� �� �ٽ� �ʱ�ȭ
    }


    void UpdateSprite(Vector2 direction)
    {
        if (direction.y > 0) // ���� �̵�
            spriteRenderer.sprite = upSprite;
        else if (direction.y < 0) // �Ʒ��� �̵�
            spriteRenderer.sprite = downSprite;
        else if (direction.x > 0) // ������ �̵�
            spriteRenderer.sprite = rightSprite;
        else if (direction.x < 0) // ���� �̵�
            spriteRenderer.sprite = leftSprite;
    }

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    public int roomID;
    public SpriteRenderer spriteRenderer;
    public List<ItemData> possibleItems;
    public float itemDropChance = 0.5f;  // 50% 확률 아이템 등장

    public PlayerMessageDisplay messageDisplay;

    private Transform player;
    public float interactDistance = 2.3f;

    private bool isOpened = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (messageDisplay == null)
        {
            messageDisplay = FindObjectOfType<PlayerMessageDisplay>();
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
        }
    }
    private void Update()
    {
        if (player == null) return;

        // 플레이어와 거리 체크
        if (Vector3.Distance(transform.position, player.position) > interactDistance)
            return;

        // 마우스 왼쪽 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    OpenBox();
                }
            }
        }
    }

    public void OpenBox()
    {
        if (isOpened) return;
        isOpened = true;

        float roll = Random.Range(0f, 1f);

        if (roll < 0.40f)
        {
            Debug.Log("아이템 없음");
            messageDisplay?.ShowMessage("이런, 벌써 누가 가져갔나...");
        }
        else if (roll < 0.75f)
        {
            // 속도 아이템
            ItemData speedItem = possibleItems.Find(item => item.itemType == ItemType.SpeedBoost);
            if (speedItem != null)
            {
                if (ItemManager.Instance.GetSpeedBoostCount() >= 9)
                {
                    messageDisplay?.ShowMessage("이건 너무 많이 들고 있어 . . .");
                }
                else
                {
                    ItemManager.Instance.ObtainItem(speedItem);
                    Debug.Log($"속도 아이템 획득: {speedItem.name}");
                    messageDisplay?.ShowMessage("더 빨리 달릴 수 있을 것 같아");
                }
            }
            else
            {
                Debug.Log("속도 아이템 없음");
            }
        }
        else
        {
            // 부활 아이템
            ItemData reviveItem = possibleItems.Find(item => item.itemType == ItemType.Revive);
            if (reviveItem != null)
            {
                if (ItemManager.Instance.GetReviveCount() >= 9)
                {
                    messageDisplay?.ShowMessage("이건 너무 많이 들고 있어 . . .");
                }
                else
                {
                    ItemManager.Instance.ObtainItem(reviveItem);
                    Debug.Log($"부활 아이템 획득: {reviveItem.name}");
                    messageDisplay?.ShowMessage("이걸로 조금 더 버틸 수 있겠군");
                }
            }
            else
            {
                Debug.Log("부활 아이템 없음");
            }
        }

        Destroy(gameObject);
    }


    public void SetVisibility(bool visible)
    {
        spriteRenderer.enabled = visible;
    }
}
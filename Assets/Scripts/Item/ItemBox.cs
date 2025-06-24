using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    public int roomID;
    public SpriteRenderer spriteRenderer;
    public List<ItemData> possibleItems;
    public float itemDropChance = 0.5f;  // 50% 확률 아이템 등장

    private Transform player;
    public float interactDistance = 1.8f;

    private bool isOpened = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
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
            // 40% 확률로 실패 (아이템 없음)
            Debug.Log("아이템 없음");
        }
        else if (roll < 0.75f)
        {
            // 35% 확률로 속도 아이템 획득
            ItemData speedItem = possibleItems.Find(item => item.itemType == ItemType.SpeedBoost);
            if (speedItem != null)
            {
                ItemManager.Instance.ObtainItem(speedItem);
                Debug.Log($"속도 아이템 획득: {speedItem.name}");
            }
            else
            {
                Debug.Log("속도 아이템 없음");
            }
        }
        else
        {
            // 25% 확률로 부활 아이템 획득
            ItemData reviveItem = possibleItems.Find(item => item.itemType == ItemType.Revive);
            if (reviveItem != null)
            {
                ItemManager.Instance.ObtainItem(reviveItem);
                Debug.Log($"부활 아이템 획득: {reviveItem.name}");
            }
            else
            {
                Debug.Log("부활 아이템 없음");
            }
        }

        // 상자 열리는 애니메이션, 사운드 등 처리
        Destroy(gameObject);
    }

    public void SetVisibility(bool visible)
    {
        spriteRenderer.enabled = visible;
    }
}
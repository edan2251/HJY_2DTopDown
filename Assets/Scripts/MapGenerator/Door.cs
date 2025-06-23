using System;
using UnityEngine;

[Serializable]
public class Door : CustomTileBase
{
    [SerializeField] public bool isBossDoor = false;

    private bool canCollide = false;
    private bool isLocked = true; // 잠금 상태 변수

    [SerializeField] private Cell ownerCell;
    [SerializeField] private Door nextDoor;
    [SerializeField] private Vector3Int nextDoorPos;

    [SerializeField] private Sprite disabledDoorSprite;  // 잠긴 문 이미지 (lockedSprite)

    private Sprite originalDoorSprite;

    private SpriteRenderer doorSpriteRenderer;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        doorSpriteRenderer = GetComponent<SpriteRenderer>();

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
            boxCollider.isTrigger = true;
        }

        // sprite 저장 (MapGeneratorIssac에서 설정한 이미지 기억해두기)
        if (doorSpriteRenderer != null)
            originalDoorSprite = doorSpriteRenderer.sprite;
    }

    public Door(Vector2 _posWorld) : base(_posWorld)
    {
        this.posWorld = _posWorld;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.gameObject.GetComponentInParent<Player>();

        if (player)
        {
            if (isLocked)
            {
                // 잠긴 문일 때는 통과 불가 (콜라이더 활성화 상태)
                return;
            }

            if (!canCollide)
            {
                if (nextDoor == null)
                {
                    return;
                }
                nextDoor.canCollide = true;
                DungeonManager.GetInstance().SetPlayerPos(nextDoorPos);
                DungeonManager.GetInstance().SetPlayerRoomID(nextDoor.ownerCell.id);
                DungeonManager.GetInstance().SetMainCameraPosSmooth();

                if (!GameTestManager.GetInstance().allMapVisibleMode)
                {
                    if (GameTestManager.GetInstance().clearCount != 3)  // 3스테이지가 아니면
                    {
                        DungeonManager.GetInstance().ActivateMinimap(nextDoor.ownerCell.id, true);
                        DungeonManager.GetInstance().ActivateMinimap(ownerCell.id, false);
                    }
                }
            }
        }
    }

    public void SetOriginalSprite(Sprite sprite)
    {
        originalDoorSprite = sprite;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.gameObject.GetComponentInParent<Player>();
        if (player)
        {
            canCollide = false;
        }
    }

    // 문 잠그기
    public void LockDoor()
    {
        isLocked = true;
        SetCollider(true);  // 콜라이더 활성화 (통과불가)

        if (doorSpriteRenderer != null && disabledDoorSprite != null)
            doorSpriteRenderer.sprite = disabledDoorSprite;
    }

    // 문 열기
    public void UnlockDoor()
    {
        isLocked = false;
        canCollide = false; // 다음 진입 시 이동 허용
        SetCollider(true); // 콜라이더 항상 켜두자


        if (doorSpriteRenderer != null && originalDoorSprite != null)
            doorSpriteRenderer.sprite = originalDoorSprite;
    }

    private void SetCollider(bool enabled)
    {
        if (boxCollider != null)
        {
            boxCollider.enabled = enabled;
            boxCollider.isTrigger = !isLocked;  // 잠긴 문은 충돌체로 막고, 열린 문은 트리거로 통과 가능하게
        }
    }

    // 외부에서 잠금 상태 확인 가능
    public bool IsLocked()
    {
        return isLocked;
    }

    // 프로퍼티
    public Cell OwnerCell
    {
        get { return ownerCell; }
        set { ownerCell = value; }
    }

    public Door NextDoor
    {
        get { return nextDoor; }
        set { nextDoor = value; }
    }

    public Vector3Int NextDoorPos
    {
        get { return nextDoorPos; }
        set { nextDoorPos = value; }
    }
}

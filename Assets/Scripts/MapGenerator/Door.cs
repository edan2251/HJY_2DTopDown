using System;
using UnityEngine;

[Serializable]
public class Door : CustomTileBase
{
    //private bool isEnabled = true;
    private bool canCollide = false;
    [SerializeField] private Cell ownerCell;
    [SerializeField] private Door nextDoor;
    [SerializeField] private Vector3Int nextDoorPos;
    [SerializeField] private Sprite dafaultDoorSprite;
    [SerializeField] private Sprite disabledDoorSprite;
    private SpriteRenderer doorSpriteRenderer;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        doorSpriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = true;
            boxCollider2D.isTrigger = true;
        }
    }

    public Door( Vector2 _posWorld ) : base( _posWorld )
    {
        this.posWorld = _posWorld;
    }

    private void OnTriggerEnter2D( Collider2D other )
    {
        Player player = other.gameObject.GetComponentInParent<Player>();
        if (player && !canCollide)
        {
            nextDoor.canCollide = true;
            DungeonManager.GetInstance().SetPlayerPos( nextDoorPos );
            DungeonManager.GetInstance().SetPlayerRoomID( nextDoor.ownerCell.id );
            DungeonManager.GetInstance().SetMainCameraPos();

            if (!GameTestManager.GetInstance().allMapVisibleMode)
            {
                DungeonManager.GetInstance().SetVisibilityTiles( nextDoor.ownerCell.id, true );
                DungeonManager.GetInstance().SetVisibilityTiles( ownerCell.id, false );

                DungeonManager.GetInstance().ActivateMinimap( nextDoor.ownerCell.id, true );
                DungeonManager.GetInstance().ActivateMinimap( ownerCell.id, false );
            }


        }
    }

    private void OnTriggerExit2D( Collider2D other )
    {
        Player player = other.gameObject.GetComponentInParent<Player>();
        if (player)
        {
            canCollide = false;
        }
    }


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

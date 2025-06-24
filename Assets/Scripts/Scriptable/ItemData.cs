using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;  // 여기서 enum 타입을 참조
    public Sprite icon;

    [Header("Speed Boost Option")]
    public float speedMultiplier;
    public float duration;

    [Header("Revive Option")]
    public float reviveTime;
}
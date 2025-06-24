using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.IO;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    [System.Serializable]
    public class ItemSaveData
    {
        public int speedBoostCount;
        public int reviveCount;
    }

    public int GetSpeedBoostCount() => speedBoostCount;
    public int GetReviveCount() => reviveCount;

    public ItemData speedBoostItemData;
    public ItemData reviveItemData;

    private string SavePath => Path.Combine(Application.persistentDataPath, "item_save.json");

    public ItemData GetReviveItemData()
    {
        return reviveItemData;
    }

    [Header("아이템 UI 버튼")]
    public Button speedBoostButton;
    public Button reviveButton;

    [Header("아이템 개수 텍스트")]
    public TextMeshProUGUI speedBoostCountText;
    public TextMeshProUGUI reviveCountText;

    private Image speedBoostButtonImage;
    private Image reviveButtonImage;

    private int speedBoostCount = 0;
    private int reviveCount = 0;

    private bool isSpeedBoostActive = false;

    public PlayerMessageDisplay messageDisplay;

    private void OnApplicationQuit()
    {
        SaveItems();
    }

    private void Awake()
    {

        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 버튼 클릭 이벤트 연결
        speedBoostButton.onClick.AddListener(UseSpeedBoost);

        speedBoostButtonImage = speedBoostButton.GetComponent<Image>();
        reviveButtonImage = reviveButton.GetComponent<Image>();

        LoadItems();
    }

    private void Start()
    {
        // 초기 UI 세팅
        UpdateSpeedBoostUI();
        UpdateReviveUI();
    }

    private void SetButtonColor(Image img, float alpha)
    {
        img.color = new Color(1f, 1f, 1f, alpha); // 완전한 흰색 기준
    }

    public void ObtainItem(ItemData itemData)
    {
        switch (itemData.itemType)
        {
            case ItemType.SpeedBoost:
                if (speedBoostCount >= 9)
                {
                    return;
                }

                speedBoostItemData = itemData;
                speedBoostCount++;
                UpdateSpeedBoostUI();
                break;

            case ItemType.Revive:
                if (reviveCount >= 9)
                {
                    return;
                }

                reviveItemData = itemData;
                reviveCount++;
                UpdateReviveUI();
                break;
        }

        SaveItems();
    }

    private void UpdateSpeedBoostUI()
    {
        speedBoostCountText.text = speedBoostCount.ToString();

        bool hasItem = speedBoostCount > 0;

        // 버튼은 항상 보이게
        speedBoostButton.gameObject.SetActive(true);
        speedBoostButton.interactable = true; // 항상 클릭 가능

        // 불투명도는 아이템 있으면 1, 없으면 0.5
        SetButtonColor(speedBoostButtonImage, hasItem ? 1f : 0.3f);
    }

    private void UpdateReviveUI()
    {
        reviveCountText.text = reviveCount.ToString();

        // reviveButton은 자동 발동용이라 그냥 비활성화 유지
        reviveButton.gameObject.SetActive(true);
        reviveButton.interactable = false;

        SetButtonColor(reviveButtonImage, reviveCount > 0 ? 1f : 0.3f);
    }

    private void UseSpeedBoost()
    {
        messageDisplay.ShowMessage("서둘러야겠어!");

        if (isSpeedBoostActive || speedBoostCount <= 0) return;

        speedBoostCount--;
        UpdateSpeedBoostUI();

        SaveItems();

        StartCoroutine(SpeedBoostCoroutine());
    }

    private IEnumerator SpeedBoostCoroutine()
    {
        isSpeedBoostActive = true;

        float originalSpeed = Player.Instance.moveSpeed;
        Player.Instance.moveSpeed *= speedBoostItemData.speedMultiplier;

        yield return new WaitForSeconds(speedBoostItemData.duration);

        Player.Instance.moveSpeed = originalSpeed;
        isSpeedBoostActive = false;

        UpdateSpeedBoostUI();
        messageDisplay.ShowMessage("벌써 지친거 같아. . .");
    }

    // 부활 아이템은 자동 발동이므로 별도 버튼 클릭 함수는 필요 없고,
    // 부활 처리 시 아래 함수에서 개수를 차감하면 됨
    public bool TryUseReviveItem()
    {
        if (reviveCount > 0)
        {
            reviveCount--;
            UpdateReviveUI();

            SaveItems();

            return true;
        }
        return false;
    }

    private void SaveItems()
    {
        ItemSaveData data = new ItemSaveData
        {
            speedBoostCount = this.speedBoostCount,
            reviveCount = this.reviveCount
        };

        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(SavePath, json);
    }

    private void LoadItems()
    {
        if (System.IO.File.Exists(SavePath))
        {
            string json = System.IO.File.ReadAllText(SavePath);
            ItemSaveData data = JsonUtility.FromJson<ItemSaveData>(json);
            this.speedBoostCount = data.speedBoostCount;
            this.reviveCount = data.reviveCount;
        }
        else
        {
            speedBoostCount = 0;
            reviveCount = 0;
        }

        UpdateSpeedBoostUI();
        UpdateReviveUI();
    }
}

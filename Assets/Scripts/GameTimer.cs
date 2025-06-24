using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public float[] stageTimeLimits = { 30f, 30f, 30f, 30f };
    public TextMeshProUGUI timerText;
    private float timeLeft;
    private bool timerRunning = false;
    private bool isBlinking = false;

    void Start()
    {
        int stage = GameTestManager.GetInstance().clearCount;
        timeLeft = stageTimeLimits[stage];
        timerRunning = true;
        isBlinking = false;

        timerText.DOKill();
        timerText.color = Color.white;
    }

    void Update()
    {
        if (!timerRunning) return;

        timeLeft -= Time.deltaTime;
        timeLeft = Mathf.Max(0, timeLeft);
        UpdateTimerUI();

        if (timeLeft <= 0)
        {
            timerRunning = false;
            OnTimeOver();
        }
    }

    void UpdateTimerUI()
    {
        int seconds = Mathf.CeilToInt(timeLeft % 60f);
        timerText.text = $"{seconds:D2}";

        if (timeLeft <= 10f)
        {
            timerText.color = new Color(1f, 0f, 0f, 1f);
            if (timeLeft <= 5f && !isBlinking)
            {
                isBlinking = true;
                timerText.DOFade(0.2f, 0.25f).SetLoops(-1, LoopType.Yoyo);
            }
        }
        else
        {
            timerText.DOKill();
            timerText.color = Color.white;
            isBlinking = false;
        }
    }

    void OnTimeOver()
    {
        if (ItemManager.Instance.TryUseReviveItem())
        {
            float extraTime = ItemManager.Instance.GetReviveItemData()?.reviveTime ?? 10f; // null일 경우 기본 10
            ReviveExtendTime(extraTime);
            return;
        }

        Debug.Log("시간 종료! 게임 오버");
        SceneManager.LoadScene("Test_Main");
    }

    // 부활 시 호출: 시간 연장하고 다시 타이머 재가동
    public void ReviveExtendTime(float extraTime)
    {
        timeLeft += extraTime;
        timerRunning = true;
        isBlinking = false;
        timerText.DOKill();
        timerText.color = Color.white;

        Debug.Log($"부활! 타이머 {extraTime}초 연장됨.");
    }
}

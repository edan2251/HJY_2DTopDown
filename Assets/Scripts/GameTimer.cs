using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameTimer : MonoBehaviour
{
    public float[] stageTimeLimits = { 30f, 30f, 30f, 30f }; // 각 스테이지 시간 제한
    public TextMeshProUGUI timerText; // UI에 표시될 텍스트
    private float timeLeft;
    private bool timerRunning = false;
    private bool isBlinking = false;

    void Start()
    {
        int stage = GameTestManager.GetInstance().clearCount;
        timeLeft = stageTimeLimits[stage];
        timerRunning = true;
        isBlinking = false;

        // DOTween 깜빡임 초기화
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
            TriggerGameOver();
        }
    }

    void UpdateTimerUI()
    {
        int seconds = Mathf.CeilToInt(timeLeft % 60f);
        timerText.text = $"{seconds:D2}";

        if (timeLeft <= 10f)
        {
            // 텍스트 색상을 빨간색으로 고정 (알파 1)
            timerText.color = new Color(1f, 0f, 0f, 1f);

            // 5초 이하부터 깜빡이기 시작
            if (timeLeft <= 5f && !isBlinking)
            {
                isBlinking = true;
                timerText.DOFade(0.2f, 0.25f).SetLoops(-1, LoopType.Yoyo);
            }
        }
        else
        {
            // 10초 초과 시 원상복구
            timerText.DOKill(); // 깜빡임 중지
            timerText.color = Color.white;
            isBlinking = false;
        }
    }



    void TriggerGameOver()
    {
        Debug.Log("시간 종료! 게임 오버");
        // 씬 전환 or Game Over 연출
        SceneManager.LoadScene("Test_Main"); // GameOver 씬 만들기
    }
}

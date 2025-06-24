using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class StageIntroManager : MonoBehaviour
{
    public Image blackPanel;
    public TextMeshProUGUI stageText;

    private void Start()
    {
        int stage = GameTestManager.GetInstance().clearCount;
        string[] messages = {
        "1층\n빠르게 탈출하세요",
        "2층\n거꾸로 움직입니다",
        "3층\n촛불을 모두 켜세요",
        "4층\n길을 외우세요"
        };

        if (stage >= 0 && stage < messages.Length)
        {
            ShowStageMessage(messages[stage]);
        }
    }

    private void ShowStageMessage(string message)
    {
        stageText.text = message;
        stageText.alpha = 0;
        blackPanel.color = new Color(0, 0, 0, 1);

        // 텍스트 페이드 인 (UnscaledTime 기반)
        stageText.DOFade(1f, 1f).SetUpdate(true);

        // 2초 후 페이드 아웃
        DOTween.Sequence()
            .AppendInterval(2f)
            .Append(stageText.DOFade(0f, 1f).SetUpdate(true))
            .Join(blackPanel.DOFade(0f, 1f).SetUpdate(true))
            .SetUpdate(true); // 시퀀스 전체도 UnscaledTime 사용
    }
}

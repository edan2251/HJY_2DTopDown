using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class FadeSceneChanger : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        // 시작할 때 화면을 어둡게 했다가 밝게 전환 (페이드 인)
        fadeImage.color = new Color(0, 0, 0, 1f);
        fadeImage.DOFade(0f, fadeDuration).SetUpdate(true);
    }

    public void ChangeSceneWithFade(string sceneName)
    {
        // 화면 어둡게 만들고, 전환 완료 시 씬 로드
        fadeImage.DOFade(1f, fadeDuration).SetUpdate(true).OnComplete(() =>
        {
            if (sceneName == "Test_Main")
            {
                var gameManager = GameTestManager.GetInstance();
                if (gameManager != null)
                {
                    gameManager.clearCount = 0;
                    Debug.Log("메인으로 이동: clearCount 초기화됨");
                }
            }

            SceneManager.LoadScene(sceneName);
        });
    }
}

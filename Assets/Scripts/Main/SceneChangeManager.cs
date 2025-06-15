using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangeManager : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    private void Start()
    {
        // 씬 시작 시 자동 페이드인
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeIn()
    {
        float t = fadeDuration;
        Color c = fadeImage.color;
        while (t > 0)
        {
            t -= Time.deltaTime;
            c.a = t / fadeDuration;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0;
        fadeImage.color = c;
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = t / fadeDuration;
            fadeImage.color = c;
            yield return null;
        }

        var gameManager = GameTestManager.GetInstance();
        if (gameManager != null)
        {
            gameManager.clearCount = 0; // 초기화
        }
        // else는 아무 처리도 안 함 (조용히 무시)

        SceneManager.LoadScene(sceneName);
    }
}

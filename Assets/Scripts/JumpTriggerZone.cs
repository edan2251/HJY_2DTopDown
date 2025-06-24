using UnityEngine;
using System.Collections;

public class JumpTriggerZone : MonoBehaviour
{
    public string sceneToLoad = "Dungeon";
    public FadeSceneChanger fadeChanger; // 인스펙터에 직접 할당할 참조

    public PlayerMessageDisplay messageDisplay;

    private bool playerInside = false;
    private bool sceneChanged = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void Update()
    {
        if (!playerInside || sceneChanged) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (fadeChanger != null)
            {
                sceneChanged = true;
                messageDisplay.ShowMessage("이곳을 탈출해겠어..!");
                StartCoroutine(DelayedSceneChange(0.5f));
            }
            else
            {
                Debug.LogWarning("FadeSceneChanger가 할당되지 않았습니다.");
            }
        }
    }
    private IEnumerator DelayedSceneChange(float delay)
    {
        yield return new WaitForSeconds(delay);
        fadeChanger.ChangeSceneWithFade(sceneToLoad);
    }
}

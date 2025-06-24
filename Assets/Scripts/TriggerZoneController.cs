using UnityEngine;
using System.Collections;

public class TriggerZoneController : MonoBehaviour
{
    public ItemDataResetter itemDataResetter;
    public VisitedRoomsResetter visitedRoomsResetter;
    public QuitMenu quitMenu;

    public PlayerMessageDisplay messageDisplay;

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    private void Update()
    {
        if (!playerInside) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (itemDataResetter != null)
            {
                itemDataResetter.ResetItemData();
                messageDisplay.ShowMessage("더 이상 필요 없어 !");
            }
            else if (visitedRoomsResetter != null)
            {
                visitedRoomsResetter.ResetVisitedRooms();
                messageDisplay.ShowMessage("던전에 대한 기억이 없어졌어 . . .");
            }
            else if (quitMenu != null)
            {
                messageDisplay.ShowMessage("평생 이곳에서\n 살지 뭐 . . .");
                StartCoroutine(DelayedQuit(0.5f));  // 1.5초 뒤에 실행
            }
        }
    }

    private IEnumerator DelayedQuit(float delay)
    {
        yield return new WaitForSeconds(delay);
        quitMenu.ExitGame();
    }
}

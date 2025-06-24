using UnityEngine;

public class MainSceneIntro : MonoBehaviour
{
    public PlayerMessageDisplay messageDisplay;

    void Start()
    {
        if (messageDisplay == null) return;

        var manager = GameTestManager.GetInstance();

        if (manager.isReturned)
        {
            if (manager.isFailed)
            {
                messageDisplay.ShowMessage("망했어.. 너무 느렸나봐.. ");
            }
            else
            {
                messageDisplay.ShowMessage("맙소사, 또 이곳이야.. ");
            }

            manager.isReturned = false;
            manager.isFailed = false;
        }
        else
        {
            messageDisplay.ShowMessage("여긴 어디지 .. ?");
        }
    }
}

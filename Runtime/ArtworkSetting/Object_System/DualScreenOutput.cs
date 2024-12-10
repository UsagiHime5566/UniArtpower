using UnityEngine;

public class DualScreenOutput : MonoBehaviour
{
    void Start()
    {
        // 如果存在第二個螢幕
        if (Display.displays.Length > 1)
        {
            foreach (var dp in Display.displays)
            {
                dp.Activate();
            }

            // 設置遊戲視窗的位置和大小以填滿第二個螢幕
            //Screen.SetResolution(Display.displays[1].systemWidth, Display.displays[1].systemHeight, FullScreenMode.FullScreenWindow);
            //Screen.fullScreen = true;

            Debug.LogWarning("Dual Screen.");
        }
        else
        {
            Debug.LogWarning("No second display detected.");
        }
    }
}

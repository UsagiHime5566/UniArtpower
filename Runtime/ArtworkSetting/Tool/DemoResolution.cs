using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoResolution : MonoBehaviour
{
    [HimeLib.HelpBox] public string helpBox = "在輸出資料夾裡創一個demo.txt檔案, 就可以切換到Demo模式.";
    public string demoFileName = "demo.txt";
    public Vector2Int resolutionDemo = new Vector2Int(540, 960);
    public Vector2Int resolutionRelease = new Vector2Int(1080, 1920);
    void Start()
    {
        SetResolution();
    }

    public void SetResolution(){
        var testmode = string.Format("{0}/../{1}", Application.dataPath, demoFileName);
        if(System.IO.File.Exists(testmode)){
            Screen.SetResolution(resolutionDemo.x, resolutionDemo.y, FullScreenMode.Windowed);
        } else {
            Screen.SetResolution(resolutionRelease.x, resolutionRelease.y, FullScreenMode.FullScreenWindow);
        }
    }
}

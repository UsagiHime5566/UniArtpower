using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CaptureRectToFile : MonoBehaviour
{
    [HimeLib.HelpBox] public string tip = "該Canvas 必須是 Camera Mode, 並附加Camera 上去";
    [Header("要儲存的區域")] public RectTransform ToSaveArea;
    [Header("該區域的左下角")] public RectTransform LeftDownCorner;
    [Header("該區域的右上角")] public RectTransform RightUpCorner;

    public Action OnPreSave;
    public Action<string> OnSaved;

    public void StartCapture(){
        OnPreSave?.Invoke();
        StartCoroutine(CaptureByUI(ToSaveArea, "background"));
    }

    public IEnumerator CaptureByUI(RectTransform UIRect, string mFileName)
    {
        //等待帧画面渲染结束
        yield return new WaitForEndOfFrame();

        Vector3 LeftDown = Camera.main.WorldToScreenPoint(LeftDownCorner.position);
        Vector3 RightUp = Camera.main.WorldToScreenPoint(RightUpCorner.position);

        int width = (int)(RightUp.x - LeftDown.x);
        int height = (int)(RightUp.y - LeftDown.y);

        Debug.Log($"w:{width} , h:{height}");

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // //左下角为原点（0, 0）
        Debug.Log($"Catch screen start at x:{LeftDown.x} , y:{LeftDown.y}");

        //从屏幕读取像素, leftBtmX/leftBtnY 是读取的初始位置,width、height是读取像素的宽度和高度
        tex.ReadPixels(new Rect(LeftDown.x, LeftDown.y, width, height), 0, 0);

        //执行读取操作 , EncodeToPNG 比上 JPG 慢非常多
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();

    #if UNITY_EDITOR
        //保存
        // string path = Application.dataPath + "/../" + mFileName + ".png";
        // System.IO.File.WriteAllBytes(path, bytes);
        // Debug.Log($"Save to path:{path}");
    #endif

        string encodedText = System.Convert.ToBase64String (bytes);
        OnSaved?.Invoke(encodedText);
    }
}

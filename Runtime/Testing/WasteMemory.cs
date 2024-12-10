using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WasteMemory : MonoBehaviour
{
    void Update()
    {
        if(Input.GetMouseButton(0)){
            DoWasteMemory();
        }
    }

    void DoWasteMemory(){
        GenerateTransparentPNG("temp", 640, 480);
    }

    void GenerateTransparentPNG(string fileName, int width, int height)
    {
        // 建立新的Texture2D，尺寸為width * height，並設置像素格式為RGBA32，即包含Alpha通道的格式
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // 將所有像素設置為透明
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0, 0, 0, 0); // 透明色
        }
        texture.SetPixels(pixels);

        // 將Texture2D應用到材質上
        texture.Apply();
    }
}

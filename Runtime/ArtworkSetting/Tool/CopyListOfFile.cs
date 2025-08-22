using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CopyListOfFile : MonoBehaviour
{
    [HimeLib.HelpBox] public string tip = "結案用工具, 將專案裡用到的所有素材匯出到指定的資料夾, 播放後開始複製";

    [Header("檔案列表, 通常是由Editor.log 那邊去複製")]
    [Multiline] public string data;

    // 範例資料
    // 190.8 kb	 0.1% Assets/Sound/標點符號/待機play按紐vfx.wav
    // 165.4 kb	 0.1% Assets/Sound/標點符號/引導影片入場.wav
    // 143.1 kb	 0.1% Assets/Sound/標點符號/結果畫面.wav
    // 138.3 kb	 0.1% Assets/Textures/0321_你是哪種標點符號/按鈕/按鈕_前言.png
    // 135.9 kb	 0.1% Assets/Textures/0328/banner手指@2x.png
    // 109.0 kb	 0.0% Assets/Sound/標點符號/題目按鈕vfx.wav
    // 101.3 kb	 0.0% Assets/Textures/0310標點符號程式demo素材/Q5/btn_4.png
    // 101.3 kb	 0.0% Assets/Textures/0310標點符號程式demo素材/Q5/btn_1.png
    // 101.2 kb	 0.0% Assets/Textures/0310標點符號程式demo素材/Q5/btn_2.png
    // 101.2 kb	 0.0% Assets/Textures/0310標點符號程式demo素材/Q5/btn_5.png
    // 101.0 kb	 0.0% Assets/Textures/0310標點符號程式demo素材/Q5/btn_3.png

    public List<string> allowedExtensions; // 允許的副檔名列表
    public string destinationFolder; // 目標資料夾
    void Start()
    {
        Copy();
    }

    void Copy()
    {
        string[] lines = data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); // 同時考慮 \n 和 \r

        foreach (string line in lines)
        {
            foreach (string extension in allowedExtensions)
            {
                if (line.Contains("Assets") && line.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    // 擷取 "Assets" 之後的文字資料
                    int startIndex = line.IndexOf("Assets");
                    string assetPath = line.Substring(startIndex).Trim(); // 去除空白

                    // 取得檔案完整路徑
                    string fullPath = Path.Combine(Application.dataPath, "..", assetPath);

                    // 確認檔案存在
                    if (File.Exists(fullPath))
                    {
                        // 建立目標檔案路徑
                        string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(assetPath));

                        // 複製檔案
                        File.Copy(fullPath, destinationPath, true);

                        Debug.Log("檔案複製成功：" + destinationPath);
                    }
                    else
                    {
                        Debug.LogError("檔案不存在：" + fullPath);
                    }
                    break;
                }
            }
        }
    }
    
    void Go()
    {
        string[] lines = data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); // 同時考慮 \n 和 \r

        foreach (string line in lines)
        {
            foreach (string extension in allowedExtensions)
            {
                if (line.Contains("Assets") && line.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    // 擷取 "Assets" 之後的文字資料
                    int startIndex = line.IndexOf("Assets");
                    string assetName = line.Substring(startIndex);

                    Debug.Log(assetName);
                    break;
                }
            }
        }
    }

    void Go2()
    {
        string[] lines = data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); // 同時考慮 \n 和 \r

        foreach (string line in lines)
        {
            // foreach (var item in line.ToCharArray())
            // {
            //     Debug.Log((int)item + "|" + item);
            // }
            // Debug.Log(line);
            // Debug.Log(line.EndsWith(".cs"));

            if (line.Contains("Assets") && (line.EndsWith(".cs")))
            {
                // 擷取 "Assets" 之後的文字資料
                int startIndex = line.IndexOf("Assets");
                string assetName = line.Substring(startIndex);

                Debug.Log(assetName);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoadBackground : MonoBehaviour
{
    [Header("要貼到的目標 Mesh")] public MeshRenderer targetMesh;
    [Header("要貼到的目標 RawImage")] public RawImage targetRawImage;
    [Header("檔案名稱(*.jpg , *.png)")] public string fileName = @"background";
    [HimeLib.HelpBox] public string tip = "檔案尺寸必須是1920x1080";
    string fileSuffixJPG = @".jpg";
    string fileSuffixPNG = @".png";
    Texture2D targetTex;

    string useUrl;
    void Start()
    {
        //From Local path
        string root = Application.dataPath + "/../";
        useUrl = root + fileName + fileSuffixPNG;
        if (!System.IO.File.Exists(useUrl))
            useUrl = root + fileName + fileSuffixJPG;

        if (!System.IO.File.Exists(useUrl)){
            Debug.LogError("No Background file found in : " + useUrl);
            return;
        }

        Debug.Log("Use Background file : " + useUrl);
        DoLoadBackground();

        //From internet
        //string netUrl = "https://i.imgur.com/DpRAzV5.png";
        //StartCoroutine(DownloadImage(netUrl));
    }

    public void DoLoadBackground(){
        StartCoroutine(DownloadImage(useUrl, LoadImageToMesh));
    }

    IEnumerator DownloadImage(string MediaUrl, System.Action<Texture2D> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }

    void LoadImageToMesh(Texture2D tex){
        if(targetMesh)
            targetMesh.material.mainTexture = tex;
        if(targetRawImage){
            targetRawImage.color = Color.white;
            targetRawImage.texture = tex;
            targetRawImage.enabled = true;
        }
    }
}

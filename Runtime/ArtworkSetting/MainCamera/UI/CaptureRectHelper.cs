using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CaptureRectToFile))]
public class CaptureRectHelper : MonoBehaviour
{
    CaptureRectToFile captureRectToFile;
    WaitForSeconds wait = new WaitForSeconds(0.5f);
    string lastEncodedBase64 = string.Empty;
    System.Action QueueAction;

    [Header("要執行截圖的按鈕")] public Button BTN_DoCapture;
    [Header("呼叫javascript上傳")] public Button BTN_PassJavaToUpload;
    [Header("截圖前要隱藏的物件")] public List<GameObject> toHide;

    void Awake(){
        captureRectToFile = GetComponent<CaptureRectToFile>();
    }

    void Start()
    {
        //執行截圖
        BTN_DoCapture?.onClick.AddListener(StartCapture);

        //暫存截圖結果
        captureRectToFile.OnSaved += CaptureCallback;

        //執行上傳
        BTN_PassJavaToUpload?.onClick.AddListener(UploadImage);
    }

    IEnumerator AsyncStartCapture(){
        captureRectToFile.StartCapture();

        if(BTN_DoCapture){
            BTN_DoCapture.interactable = false;
            yield return wait;
            BTN_DoCapture.interactable = true;
        }
    }

    void ObjectsActive(bool val){
        if(toHide.Count == 0)
            return;

        foreach (var item in toHide)
        {
            if(item == null)
                continue;

            item.SetActive(val);
        }
    }

    void CaptureCallback(string x){
        lastEncodedBase64 = x;
        if(QueueAction != null){
            QueueAction?.Invoke();
            QueueAction = null;
        }
    }

    [ContextMenu("Start Capture")]
    void StartCapture(){
        QueueAction += delegate {
            ObjectsActive(true);
        };

        ObjectsActive(false);
        StartCoroutine(AsyncStartCapture());
    }

    [ContextMenu("Upload Image")]
    void UploadImage(){
        Application.ExternalCall("ToUpload", lastEncodedBase64);
        Debug.Log($"Upload base64:\n {lastEncodedBase64}");
    }
}

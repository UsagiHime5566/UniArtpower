using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScriptsCamera))]
public class WebcamHelper : MonoBehaviour
{
    ScriptsCamera scriptsCamera;

    [Header("UI ref")]
    public RectTransform flipOrNot;
    [Header("重新打開Webcam")] public Dropdown DRD_CamList;
    [Header("重新打開Webcam")] public Button BTN_Restart;
    [Header("翻轉Webcam")] public Button BTN_Flip;
    [Header("截圖並儲存於根目錄")] public Button BTN_CatchPhoto;

    private void Awake() {
        scriptsCamera = GetComponent<ScriptsCamera>();
    }

    void Start()
    {
        //設定 - 可用相機列表
        DRD_CamList.options.Clear();
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData(){
                text = devices[i].name
            };
            DRD_CamList.options.Add(data);
        }

        //設定 - 重啟相機
        BTN_Restart.onClick.AddListener(() => {
            RestartWebcam();
        });

        //相機設備選用 - 設定與紀錄
        int id = SystemConfig.Instance.GetData<int>(StringManager.Webcam.DeviceID);
        DRD_CamList.value = id;
        scriptsCamera.WebcamIndex = DRD_CamList.value;  //如果 id 大於 DropValue, 他會自己限縮, 因此要讀取 DropValue
        DRD_CamList.captionText.text = DRD_CamList.options[DRD_CamList.value].text;

        DRD_CamList.onValueChanged.AddListener(x => {
            SystemConfig.Instance.SaveData(StringManager.Webcam.DeviceID, x);
            scriptsCamera.WebcamIndex = x;
        });

        //相機翻轉 - 設定與紀錄
        flipOrNot.localScale = SystemConfig.Instance.GetData<Vector3>(StringManager.Webcam.FlipWebcam, Vector3.one);
        BTN_Flip.onClick.AddListener(() => {
            flipOrNot.localScale = new Vector3(flipOrNot.localScale.x * -1, 1, 1);
            SystemConfig.Instance.SaveData(StringManager.Webcam.FlipWebcam, flipOrNot.localScale);
        });

        //選用功能 - 相機截圖
        BTN_CatchPhoto?.onClick.AddListener(() => {
            scriptsCamera.DoTakePhotoSave(flipOrNot.localScale.x < 0);
        });
    }

    async void RestartWebcam(){
        BTN_Restart.interactable = false;
        scriptsCamera.StopWebcam();
        await Task.Delay(3000);
        scriptsCamera.StartWebcam(delegate {
            BTN_Restart.interactable = true;
        });
    }
}

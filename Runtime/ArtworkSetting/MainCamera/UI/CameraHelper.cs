using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CameraController))]
public class CameraHelper : MonoBehaviour
{
    [Header("資訊 - 畫面位置")] public Text TXT_Pos;
    [Header("資訊 - 畫面方向")] public Text TXT_Rot;
    [Header("翻轉畫面")] public Button BTN_FlipCam;
    [Header("重置畫面")] public Button BTN_ResetCam;

    Camera attachCamera;
    CameraController attachCameraController;


    [Header("系統資訊")]
    public bool flipHorizontal;

    [Header("預設值")]
    public Vector3 defaultPos =  new Vector3(0, 1, -10);
    public Quaternion defaultRot = Quaternion.Euler(0, 0, 0);

    [EasyButtons.Button]
    void SetToDefault(){
        defaultPos = transform.position;
        defaultRot = transform.rotation;
    }
    
    void Awake()
    {
        attachCamera = GetComponent<Camera>();
        attachCameraController = GetComponent<CameraController>();
    }

    void Start(){

        //畫面位置 - 設定與紀錄
        attachCamera.transform.localPosition = SystemConfig.Instance.GetData<Vector3>(StringManager.CameraView.Pos, defaultPos);
        attachCamera.transform.localRotation = SystemConfig.Instance.GetData<Quaternion>(StringManager.CameraView.Rot, defaultRot);
        ApplicationDelegate.instance.ToDoOnQuit += SaveInfos;

        //畫面翻轉 - 設定與紀錄
        flipHorizontal = SystemConfig.Instance.GetData<bool>(StringManager.CameraView.FlipCamera, false);
        BTN_FlipCam.onClick.AddListener(() => {
            flipHorizontal = !flipHorizontal;
            SystemConfig.Instance.SaveData(StringManager.CameraView.FlipCamera, flipHorizontal);
        });

        //設定 - 重置畫面
        BTN_ResetCam.onClick.AddListener(delegate {
            attachCamera.transform.localPosition = defaultPos;
            attachCamera.transform.localRotation = defaultRot;
            attachCameraController.ResetCoord();
        });
    }

    void Update(){
        TXT_Pos.text = "Position: " + attachCamera.transform.localPosition.ToString();
        TXT_Rot.text = "Rotation: " + attachCamera.transform.localEulerAngles.ToString();
    }

    void OnPreCull()
    {
        attachCamera.ResetWorldToCameraMatrix();
        attachCamera.ResetProjectionMatrix();
        Vector3 scale = new Vector3(flipHorizontal ? -1 : 1, 1, 1);
        //Vector3 scale = new Vector3(1,flipHorizontal ? -1 : 1,  1);
        //Vector3 scale = new Vector3(1,1 , flipHorizontal ? -1 : 1);
        attachCamera.projectionMatrix = attachCamera.projectionMatrix * Matrix4x4.Scale(scale);
    }
    void OnPreRender()
    {
        GL.invertCulling = flipHorizontal;
    }
    void OnPostRender()
    {
        GL.invertCulling = false;
    }

    void SaveInfos(){
        SystemConfig.Instance.SaveData(StringManager.CameraView.Pos, transform.position);
        SystemConfig.Instance.SaveData(StringManager.CameraView.Rot, transform.rotation);
    }
}

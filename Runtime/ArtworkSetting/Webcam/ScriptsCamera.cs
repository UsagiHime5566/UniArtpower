using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

//這份Code 必定要存在
//因為使用到WebCamTexture 才能夠使裝置註冊 相機使用權限
public class ScriptsCamera : MonoBehaviour
{
    [HimeLib.HelpBox] public string scriptTip = "這份為PC用，若要在行動裝置上的話使用套件NatCam";

    [Header("自動化設定")]
    [Tooltip("是否執行後就開啟相機")] public bool runInStart = false;

    [Header("Webcam Input Infos")]
    [Tooltip("要附加的UI物件")] public RawImage drawPixels;
    [Tooltip("縮放模式，擴張到'滿版'或是縮小至'全部都看的到'")] public AspectMode C_Mode;


    [Header("Windwos")]
    [Tooltip("系統要選用哪一個像機號碼")] public int WebcamIndex;
    [Tooltip("系統啟動後會將所有的像機列表在此")] public List<string> WebcamList;
    [Tooltip("顯示器畫面是不是橫放，若是PC的話，通常都是橫放(畫面寬大於高)")] public bool isLandscape;
    [Tooltip("手機的相機根據設備都會轉角度，但PC的不會，若PC要模擬像機轉角度，將角度填入此處")] public float CameraRot;

    
    public enum AspectMode
    {
        Expand = 0,
        Shrink = 1,
    }
    bool camAvailable;
    WebCamTexture webcamTexture;
    Vector2 LastTextureSize = Vector2.zero;

    //public 
    public WebCamTexture WBC => webcamTexture;
    public System.Action OnBackgroundWrited;
    public System.Action<string> OnBackgroundBase64Get;
    
    
    async void Start(){
        drawPixels.enabled = false;

        await Task.Delay(1000);

        // 避免Editor 時期停止測試後還繼續執行
        if(this == null)
            return;

        if(runInStart)
            StartWebcam(null);
    }
    
    void OnApplicationQuit() {
        StopWebcam();
    }

    public async void StartWebcam(System.Action callback){
        
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("No camera detected");
            camAvailable = false;
            return;
        }

        if(webcamTexture == null)
        {
            #if UNITY_STANDALONE_WIN
                for (int i = 0; i < devices.Length; i++)
                    WebcamList.Add(devices[i].name);

                webcamTexture = new WebCamTexture(devices[WebcamIndex].name, Screen.width, Screen.height);
                Debug.Log($"Use Webcam : {devices[WebcamIndex].name}");
            #else
                for (int i = 0; i < devices.Length; i++)
                {
                    if (devices [i].isFrontFacing) {
                        webcamTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
                        Debug.Log($"Use Webcam : {devices[i].name}");
                        break;
                    }
                    if(i > 0 && i == devices.Length - 1){
                        webcamTexture = new WebCamTexture(devices[1].name, Screen.width, Screen.height);
                        Debug.Log($"Guess Webcam : {devices[1].name}");
                    }
                }
            #endif
        }

        if (webcamTexture == null) {
            Debug.Log("Unable to find back camera");
            return;
        }

        if(webcamTexture.isPlaying){
            Debug.Log("Webcam are running.");
            callback?.Invoke();
            return;
        }

        webcamTexture.Play();
        drawPixels.enabled = true;
        drawPixels.color = Color.white;
        drawPixels.texture = webcamTexture;
        camAvailable = true;

        await Task.Yield();

        callback?.Invoke();
    }

    public void PauseWebcam(){
        drawPixels.enabled = false;

        if(webcamTexture == null)
            return;

        webcamTexture.Pause();
    }

    public void StopWebcam(){
        drawPixels.enabled = false;

        if(webcamTexture == null)
            return;

        webcamTexture.Pause();
        webcamTexture.Stop();
        webcamTexture = null;
        camAvailable = false;
    }

    

    void Update()
    {
        if (!camAvailable)
            return;

        if(webcamTexture == null)
            return;

        if(!webcamTexture.isPlaying)
            return;

        if(LastTextureSize == new Vector2(webcamTexture.width, webcamTexture.height))
            return;

        float scaleY = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
        drawPixels.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
        
    #if UNITY_EDITOR
        drawPixels.transform.localRotation = Quaternion.Euler(0, 0, -CameraRot);
    #else
        drawPixels.transform.localRotation = Quaternion.Euler(0, 0, -webcamTexture.videoRotationAngle);
    #endif

        Vector2 source = new Vector2((float)webcamTexture.width, (float)webcamTexture.height);
        Vector2 ToFit = new Vector2((float)Screen.height, (int)Screen.width);
        if(isLandscape)
            ToFit = new Vector2((float)Screen.width, (int)Screen.height);

        LastTextureSize = source;

        drawPixels.rectTransform.sizeDelta = GetAspectSize(source, ToFit, C_Mode);

        Debug.Log($"Set up new Size {drawPixels.rectTransform.sizeDelta}");



        // 判断是否是竖屏，竖屏时由于旋转的关系，需要将width和height调换
        // if (webcamTexture.videoRotationAngle % 180 != 0)
        //     fitter.aspectRatio = (float)webcamTexture.height / (float)webcamTexture.width;
        //     //background.rectTransform.sizeDelta = new Vector2(Screen.height, Screen.width);
        // else
        //     fitter.aspectRatio = (float)webcamTexture.width / (float)webcamTexture.height;
            //background.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);

        //JudgeCamera();
    }

    Vector2 GetAspectSize(Vector2 source, Vector2 ToFit, AspectMode mode){
        Vector2 result = Vector2.one;

        //數值越高越長
        float ratio = source.x / source.y;
        float screenRatio = ToFit.x / ToFit.y;
        
        if(mode == AspectMode.Expand){

            if(ratio > screenRatio){
                //此處照片比畫面長, 固定螢幕短邊, 計算長邊
                result = new Vector2(ToFit.y / source.y * source.x, ToFit.y);
            }
            else {
                //此處照片比畫面短, 固定螢幕長邊, 計算短邊
                result = new Vector2(ToFit.x, ToFit.x / source.x * source.y);
            }
        } else {

            if(ratio > screenRatio){
                //相反
                result = new Vector2(ToFit.x, ToFit.x / source.x * source.y);
            }
            else {
                //相反
                result = new Vector2(ToFit.y / source.y * source.x, ToFit.y);
            }
        }

        return result;
    }

//     void JudgeCamera(){
//         //int rotAngle = -runningWebcam.videoRotationAngle;
//         //while( rotAngle < 0 ) rotAngle += 360;
//         //while( rotAngle > 360 ) rotAngle -= 360;

//         float scaleY = webcamTexture.videoVerticallyMirrored ? -1f : 1f; // Find if the camera is mirrored or not  
//         background.rectTransform.localScale = new Vector3(1f, scaleY, 1f); // Swap the mirrored camera  

//         if (webcamTexture != null && lastRotationAngle != webcamTexture.videoRotationAngle)
//         {
//             OnOrientationChanged();
//             lastRotationAngle = webcamTexture.videoRotationAngle;
//         }
//     }

//     private void OnOrientationChanged()
//     {
//         // 旋转rawimage，为什么加一个负号呢？因为rawimage的z轴是背对图像的，直接使用videoRotationAngle旋转，相对于图片是逆时针旋转
//         background.transform.localRotation = Quaternion.Euler(0, 0, -webcamTexture.videoRotationAngle);

//         // 判断是否是竖屏，竖屏时由于旋转的关系，需要将width和height调换
//         if (webcamTexture.videoRotationAngle % 180 != 0)
//             background.rectTransform.sizeDelta = new Vector2(Screen.height, Screen.width);
//         else
//             background.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);

//         /*
//         if (runningWebcam.videoRotationAngle % 180 != 0)
//             background.rectTransform.sizeDelta = new Vector2(Display.main.systemHeight, Display.main.systemWidth);
//         else
//             background.rectTransform.sizeDelta = new Vector2(Display.main.systemWidth, Display.main.systemHeight);
//         */
//     }


    // Get Photo functions
    public void DoTakePhotoBase64(bool flip = false){
        if(webcamTexture == null)
            return;

        if(!webcamTexture.isPlaying)
            return;

        StartCoroutine(TakePhotoBase64(flip));
    }

    public void DoTakePhotoSave(bool flip = false){
        if(webcamTexture == null)
            return;

        if(!webcamTexture.isPlaying)
            return;

        StartCoroutine(TakePhotoSave(flip));
    }

    IEnumerator TakePhotoBase64(bool flip)  // Start this Coroutine on some button click
    {
        // NOTE - you almost certainly have to do this here:
        yield return new WaitForEndOfFrame(); 
        
    #if FMSTREAM
        byte[] bytes = GetPhotoBytesFaster(flip);
    #else
        byte[] bytes = GetPhotoBytes(flip);
    #endif
        string encodedText = System.Convert.ToBase64String (bytes);

        //Debug.Log($"Saved Base64.");

        OnBackgroundBase64Get?.Invoke(encodedText);
    }

    IEnumerator TakePhotoSave(bool flip)  // Start this Coroutine on some button click
    {
        // NOTE - you almost certainly have to do this here:
        yield return new WaitForEndOfFrame(); 

        byte[] bytes = GetPhotoBytes(flip);

        //Write out the PNG. Of course you have to substitute your_path for something sensible
        string root = Application.dataPath + "/../";
        System.IO.File.WriteAllBytes(root + "background.jpg", bytes);
        
        Debug.Log($"Save file to path: {root}");

        OnBackgroundWrited?.Invoke();
    }

    byte[] GetPhotoBytes(bool flip){

        // it's a rare case where the Unity doco is pretty clear,
        // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
        // be sure to scroll down to the SECOND long example on that doco page 

        Texture2D photo = new Texture2D(webcamTexture.width, webcamTexture.height);
        if(flip){

            int xN = photo.width;
            int yN = photo.height;
 
            for(int i = 0; i < xN; i++) {
                for(int j = 0; j < yN; j++) {
                    photo.SetPixel(xN-i-1, j, webcamTexture.GetPixel(i,j));
                }
            }
        } else {
            photo.SetPixels(webcamTexture.GetPixels());
        }
        photo.Apply();

        //Encode to a PNG
        byte[] bytes = photo.EncodeToJPG();

        return bytes;
    }

#if FMSTREAM

    byte[] GetPhotoBytesFaster(bool flip){

        Texture2D photo = new Texture2D(webcamTexture.width, webcamTexture.height);
        if(flip){

            int xN = photo.width;
            int yN = photo.height;
 
            for(int i = 0; i < xN; i++) {
                for(int j = 0; j < yN; j++) {
                    photo.SetPixel(xN-i-1, j, webcamTexture.GetPixel(i,j));
                }
            }
        } else {
            photo.SetPixels(webcamTexture.GetPixels());
        }
        photo.Apply();

        byte[] outputBytes = photo.FMEncodeToJPG(75);

        return outputBytes;
    }

    async void AsyncGetPhotoBytes(bool flip){

        Texture2D photo = new Texture2D(webcamTexture.width, webcamTexture.height);
        if(flip){

            int xN = photo.width;
            int yN = photo.height;
 
            for(int i = 0; i < xN; i++) {
                for(int j = 0; j < yN; j++) {
                    photo.SetPixel(xN-i-1, j, webcamTexture.GetPixel(i,j));
                }
            }
        } else {
            photo.SetPixels(webcamTexture.GetPixels());
        }
        photo.Apply();

        byte[] outputBytes;
        byte[] RawTextureData = photo.GetRawTextureData();
        string encodedText = "";
        int _width = photo.width;
        int _height = photo.height;
        
        await Task.Run(() =>
        {
            outputBytes = RawTextureData.FMRawTextureDataToJPG(_width, _height, 75);
            encodedText = System.Convert.ToBase64String (outputBytes);
        });

        Debug.Log($"Saved Base64.");
        OnBackgroundBase64Get?.Invoke(encodedText);
    }

#endif
}
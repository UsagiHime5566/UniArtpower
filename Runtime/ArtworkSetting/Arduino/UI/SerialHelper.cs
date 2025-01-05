//Only Project setting API to .Net 4.X Can Use Serial Port
#if NET_4_6
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(UniArduinoBase))]
public class SerialHelper : MonoBehaviour
{
    [HimeLib.HelpBox] public string tip = "內建10秒後 自動啟動Arduino";
    [Header("UI Comp 設置 (TXT_Debug 可以不用設置)")]
    public Dropdown DD_Speed;
    public Dropdown DD_Serial;
    public Button BTN_Restart;
    public Text TXT_State;
    public Text TXT_Debug;

    [Header("啟動後送一個訊號, 空值則不送")]
    public string readyToSend = "c";

    [Header("Google 工具 (頁面URL, 頁面欄位id)")]
    public string googleSheetUrl;
    public string postId;
    public bool postSend = false;
    public bool postRecieve = false;
    public bool postLog = false;


    [Header("模擬收訊測試")]
    public string messageToRecieve;
    Queue<string> queueMessage;
    int maxQueueNum = 10;


    [EasyButtons.Button("Emu Recieve")]
    void EmuRecieved(){
        arduinoInteractive.OnRecieveData?.Invoke(messageToRecieve);
    }

    UniArduinoBase arduinoInteractive;
    string[] baudRate = {"300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "28800", "31250", "38400", "57600", "115200"};

    void Awake(){
        arduinoInteractive = GetComponent<UniArduinoBase>();
        queueMessage = new Queue<string>();
    }

    IEnumerator Start()
    {
        DD_Speed.SetLabels(baudRate);
        DD_Speed.SetSavedData("COMSpeed", x => {
            int rate = 9600;
            if(!int.TryParse(DD_Speed.options[x].text, out rate))
                return;

            arduinoInteractive.baudRate = rate;
        });

        string[] ports = SerialPort.GetPortNames(); 
        DD_Serial.SetLabels(ports);
        DD_Serial.SetSavedData("ReadCOM", x => {
            arduinoInteractive.comName = DD_Serial.options[x].text;
        });

        BTN_Restart.onClick.AddListener(delegate {
            RestartArduino();
        });


        // Debug message
        if(TXT_Debug){
            TXT_Debug.text = "";
            arduinoInteractive.OnRecieveData += DoQueueMessage;
            arduinoInteractive.OnDebugLogs += DoQueueMessage;
        }

        //post to google
        arduinoInteractive.OnSendData += x => {
            if(postSend) SendToGoogle(x);
        };
        arduinoInteractive.OnRecieveData += x => {
            if(postRecieve) SendToGoogle(x);
        };
        arduinoInteractive.OnDebugLogs += x => {
            if(postLog) SendToGoogle(x);
        };

        yield return new WaitForSeconds(10);

        arduinoInteractive.ConnectToArduino();
        BTN_Restart.interactable = true;
        StartCoroutine(CheckStatus());

        yield return new WaitForSeconds(5);
        
        if(!string.IsNullOrEmpty(readyToSend)) arduinoInteractive.SendData(readyToSend);
    }

    async void RestartArduino(){
        arduinoInteractive.CloseArduino();
        await Task.Delay(5000);
        if(this == null) return;
        arduinoInteractive.ConnectToArduino();
        await Task.Delay(5000);
        if(!string.IsNullOrEmpty(readyToSend)) arduinoInteractive.SendData(readyToSend);
    }

    WaitForSeconds wait = new WaitForSeconds(1.0f);
    IEnumerator CheckStatus(){
        while(true){
            yield return wait;

            if(arduinoInteractive.IsArduinoConnect()){
                TXT_State.text = "Online";
                TXT_State.color = Color.cyan;
            } else {
                TXT_State.text = "Offline";
                TXT_State.color = Color.red;
            }
        }
    }

    void DoQueueMessage(string x){
        queueMessage.Enqueue($"{x} ({System.DateTime.Now})");
        if(queueMessage.Count > maxQueueNum){
            queueMessage.Dequeue();
        }

        TXT_Debug.text = "";
        foreach (var item in queueMessage)
        {
            TXT_Debug.text += item + "\n";
        }
    }

    public void SendToGoogle(string msg){
        if(string.IsNullOrEmpty(msg))
            return;
        StartCoroutine(PostTool(msg));
    }

    IEnumerator PostTool(string msg){
        WWWForm form = new WWWForm();
        form.AddField("entry." + postId, msg);   //Copy from google form origin html code

        using (UnityWebRequest www = UnityWebRequest.Post(googleSheetUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Post tool: " + www.error);
            }
            else
            {
                //Debug.Log("Form upload complete!");
                //Debug.Log(www.downloadHandler.text);
            }
        }
    }
}

#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UniArduinoBase : MonoBehaviour
{
    [HimeLib.HelpBox] public string tip = "Arduino 端送訊息必須使用 Serial.println(\"string\");";
    /*
    **Arduino 端讀取方式**
    String s = "";
    while (Serial.available()) {
        char c = Serial.read();
        if(c!='\n'){
            s += c;
        }
        // 沒有延遲的話 UART 串口速度會跟不上Arduino的速度，會導致資料不完整
        delay(5);
    }
    */

    [Header("自動化設定")]
    [Tooltip("是否執行後就連線Arduino?")] public bool runInStart = false;

    [Header("Arduino 設置參數")]
    public int baudRate = 9600;
    public string comName = "COM1";

    [Header("Runtime params")]
    public bool isConnected = false;

    // Arduino 事件
    public Action<string> OnSendData;
    public Action<string> OnRecieveData;
	public Action<string> OnDebugLogs;


    public virtual bool ConnectToArduino(){
        return false;
    }

    public virtual void CloseArduino(){}

    public virtual void SendData(string data){}

    public virtual bool IsArduinoConnect(){
        return false;
    }
}

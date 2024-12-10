//Only Project setting API to .Net 4.X Can Use Serial Port
#if NET_4_6
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ArduinoInteractive : MonoBehaviour
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
    [Tooltip("是否執行後就開啟相機")] public bool runInStart = false;

    [Header("目前參數")]
    [SerializeField] bool ArduinoPortState = false;
    public int baudRate = 9600;
    public string comName = "COM1";
    

    //public delegate
    public Action<string> OnRecieveData;
	public Action<string> OnArduinoLogs;
	public bool ArduinoPortIsOpen => GetArduinoPortState();

    //private works
    SerialPort arduinoPort;
    Thread recThread;
	Action passToMainThread;

    async void Start()
    {
        ArduinoPortState = false;
        await Task.Delay(1000);

        // 避免Editor 時期停止測試後還繼續執行
        if(this == null)
            return;

        if(runInStart)
            StartSerial();
    }

    void Update ()
	{
		if(passToMainThread != null){
			passToMainThread.Invoke();
			passToMainThread = null;
		}
	}

    public bool StartSerial()
	{
		arduinoPort = new SerialPort( comName, baudRate );
		
		if( arduinoPort.IsOpen == false )
		{
			try {
				arduinoPort.Open();
			} catch(System.Exception e){
				Debug.LogError(e.Message.ToString());
				ArduinoPortState = false;
				return false;
			}

			if(recThread != null)
				recThread.Abort();

			recThread = new Thread (RecieveThread);
			recThread.Start ();

			DebugLog( $"Open port '{comName}' sucessful!!" );
		}
		else
		{
			DebugLog( "Port already opened!!" );
			return false;
		}

		ArduinoPortState = true;
		return true;
	}

    public void SendData(string data){
		if(arduinoPort == null){
			DebugLog(">> Can't Send (Port is null)");
			return;
		}

		if(!arduinoPort.IsOpen){
			DebugLog(">> Can't Send (Port is disconnect)");
			return;
		}
        
        //因為是 WriteLine, 所以送出去的資訊會包含\n
		arduinoPort.WriteLine(data);
	}

    void RecieveThread(){
		while (true) {
			if(arduinoPort == null){
				Thread.Sleep (10);
				continue;
			}

			if (arduinoPort.IsOpen) {
				try {
					string arduinoData = arduinoPort.ReadLine();
					Debug.Log(" >> Read arduino data : " + arduinoData );
					if(!string.IsNullOrEmpty(arduinoData)){
						passToMainThread += () => {
							OnRecieveData?.Invoke(arduinoData);
						};
					}
				}
				catch {}
			}
			else
			{
				ArduinoPortState = false;
                break;
			}

			Thread.Sleep (10);
		}
	}

    public void CloseArduino(){
		if(recThread != null)
			recThread.Abort();

		if(arduinoPort == null)
			return;
			
		arduinoPort.Close();
		ArduinoPortState = false;
	}

    void OnApplicationQuit() {
        CloseArduino();
    }

	bool GetArduinoPortState(){
		if(arduinoPort == null)
			return false;

		if(!arduinoPort.IsOpen)
			return false;

		return true;
	}

	void DebugLog(string msg){
		Debug.Log(msg);
		OnArduinoLogs?.Invoke(msg);
	}
}

#endif
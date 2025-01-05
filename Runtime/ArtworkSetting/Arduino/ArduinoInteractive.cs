//Only Project setting API to .Net 4.X Can Use Serial Port
#if NET_4_6
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ArduinoInteractive : UniArduinoBase
{
    //private works
    SerialPort arduinoPort;
    Thread recThread;
	Action passToMainThread;

    async void Start()
    {
        isConnected = false;
        await Task.Delay(1000);

        // 避免Editor 時期停止測試後還繼續執行
        if(this == null)
            return;

        if(runInStart)
            ConnectToArduino();
    }

    void Update ()
	{
		if(passToMainThread != null){
			passToMainThread.Invoke();
			passToMainThread = null;
		}
	}

    public override bool ConnectToArduino()
	{
		arduinoPort = new SerialPort( comName, baudRate );
		
		if( arduinoPort.IsOpen == false )
		{
			try {
				arduinoPort.Open();
			} catch(System.Exception e){
				Debug.LogError(e.Message.ToString());
				isConnected = false;
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

		isConnected = true;
		return true;
	}

    public override void SendData(string data){
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
					//Debug.Log(" >> Read arduino data : " + arduinoData );
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
				isConnected = false;
                break;
			}

			Thread.Sleep (10);
		}
	}

    public override void CloseArduino(){
		if(recThread != null)
			recThread.Abort();

		if(arduinoPort == null)
			return;
			
		arduinoPort.Close();
		isConnected = false;
	}

    void OnApplicationQuit() {
        CloseArduino();
    }

	public override bool IsArduinoConnect(){
		if(arduinoPort == null)
			return false;

		if(!arduinoPort.IsOpen)
			return false;

		if(!isConnected)
			return false;

		return true;
	}

	void DebugLog(string msg){
		Debug.Log(msg);
		OnDebugLogs?.Invoke(msg);
	}
}

#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationDelegate : HimeLib.SingletonMono<ApplicationDelegate>
{
    public System.Action ToDoOnQuit;

    void Start(){
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void OnApplicationQuit() {
        ToDoOnQuit?.Invoke();
        SystemConfig.Instance.SaveValues();
    }

    private void OnApplicationPause(bool pauseStatus) {
        if(pauseStatus){
            SystemConfig.Instance.SaveValues();
        }
    }
}
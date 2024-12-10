using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGenerateBat :MonoBehaviour
{
    /// <summary>
    /// 建立自動開機開程式bat, 建議程式執行時運行此指令
    /// </summary>
    void Start()
    {
#if !UNITY_EDITOR
            //example : GetDirectoryName('C:\MyDir\MySubDir\myfile.ext') returns 'C:\MyDir\MySubDir'
            //example : GetDirectoryName('C:\MyDir\MySubDir') returns 'C:\MyDir'
            string exePath = Path.GetDirectoryName(Application.dataPath);
            string batName = exePath + "/" + Application.productName + ".bat";
            var file = File.Open(batName, FileMode.Create, FileAccess.ReadWrite);
            var writer = new StreamWriter(file);
            writer.WriteLine("@echo off");
            writer.WriteLine("echo !!!");
            writer.WriteLine("echo Wait for system prepare...");
            writer.WriteLine("ping 127.0.0.1 -n 10 -w 1000");
            writer.WriteLine("cd /D " + exePath);
            //writer.WriteLine("setlocal");
            //writer.WriteLine("set regkey=\"HKEY_CURRENT_USER\\Software\\" + Application.companyName + "\\" + Application.productName + "\"");
            //writer.WriteLine("reg add %regkey% /v \"Screenmanager Resolution Width_h182942802\" /T REG_DWORD /D 1920 /f");
            //writer.WriteLine("reg add %regkey% /v \"Screenmanager Resolution Height_h2627697771\" /T REG_DWORD /D 1080 /f");
            //writer.WriteLine("endlocal");
            //writer.WriteLine(Application.productName + ".exe -screen-width 1920 -screen-height 1080 -screen-fullscreen 1");
            //writer.WriteLine(Application.productName + ".exe -screen-fullscreen 1 -timestamps");
            //使用timestamps過度雜亂, 互動畫面不只1920*1080 因此不預設參數, 使用 start 才能把小黑窗自己消失

            writer.WriteLine("start " + Application.productName + ".exe -screen-fullscreen 1");

            writer.Flush();
            file.Close();
#endif
    }

    /// <summary>
    /// 執行立即重開程式, 需搭配 CreateBatFile() 產生的bat檔案
    /// </summary>
    public void RestartApplication()
    {
        System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".bat")); //new program
        Application.Quit(); //kill current process
    }
}